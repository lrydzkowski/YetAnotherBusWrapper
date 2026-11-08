using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Options;

namespace YetAnotherBusWrapper.Naming;

internal sealed partial class EntityNameResolver
{
    private const int MaxQueueOrTopicNameLength = 260;
    private const int MaxSubscriptionNameLength = 50;
    private const int MaxKeptInstanceSuffixLength = 16;
    private const int HashedInstanceSuffixLength = 16;

    private readonly IInstanceNameProvider _instanceNameProvider;
    private readonly ILogger<EntityNameResolver> _logger;

    private readonly ConcurrentDictionary<(EntityKind Kind, string ConfiguredName, bool UseInstanceSuffix), string>
        _resolvedNames = new();

    public EntityNameResolver(IInstanceNameProvider instanceNameProvider, ILogger<EntityNameResolver> logger)
    {
        _instanceNameProvider = instanceNameProvider;
        _logger = logger;
    }

    public string ResolveQueueName(QueueOptions options)
    {
        return Resolve(EntityKind.Queue, options.QueueName!, options.UseInstanceSuffix);
    }

    public string ResolveTopicName(TopicOptions options)
    {
        return Resolve(EntityKind.Topic, options.TopicName!, options.UseInstanceSuffix);
    }

    public string ResolveSubscriptionName(SubscriptionOptions options)
    {
        return Resolve(EntityKind.Subscription, options.SubscriptionName!, options.UseInstanceSuffix);
    }

    public string Resolve(EntityKind kind, string configuredName, bool useInstanceSuffix)
    {
        return _resolvedNames.GetOrAdd(
            (kind, configuredName, useInstanceSuffix),
            key => BuildName(key.Kind, key.ConfiguredName, key.UseInstanceSuffix)
        );
    }

    private string BuildName(EntityKind kind, string configuredName, bool useInstanceSuffix)
    {
        string baseName = Sanitize(configuredName);
        if (baseName.Length == 0)
        {
            throw new InvalidOperationException($"{kind} name '{configuredName}' contains no letters or digits.");
        }

        int maxLength = kind == EntityKind.Subscription ? MaxSubscriptionNameLength : MaxQueueOrTopicNameLength;
        if (!useInstanceSuffix)
        {
            return CutToMaxLength(kind, baseName, maxLength);
        }

        string instanceSuffix = BuildInstanceSuffix(kind);

        return AppendInstanceSuffix(kind, baseName, instanceSuffix, maxLength);
    }

    private string BuildInstanceSuffix(EntityKind kind)
    {
        string instanceName = _instanceNameProvider.GetInstanceName();
        string instanceSuffix = Sanitize(instanceName);
        if (instanceSuffix.Length == 0)
        {
            throw new InvalidOperationException(
                $"Instance name '{instanceName}' used as the {kind} name suffix contains no letters or digits."
            );
        }

        return instanceSuffix;
    }

    private static string Sanitize(string name)
    {
        string withoutInvalidCharacters = new(
            name.Where(character => char.IsAsciiLetterOrDigit(character) || character == '-').ToArray()
        );

        return withoutInvalidCharacters.Trim('-').ToLowerInvariant();
    }

    private string CutToMaxLength(EntityKind kind, string name, int maxLength)
    {
        if (name.Length <= maxLength)
        {
            return name;
        }

        string cutName = Cut(name, maxLength);
        LogEntityNameCut(kind, name, maxLength, cutName);

        return cutName;
    }

    private string AppendInstanceSuffix(EntityKind kind, string baseName, string instanceSuffix, int maxLength)
    {
        string name = $"{baseName}-{instanceSuffix}";
        if (name.Length <= maxLength)
        {
            return name;
        }

        string suffix = instanceSuffix.Length > MaxKeptInstanceSuffixLength
            ? HashInstanceSuffix(instanceSuffix)
            : instanceSuffix;
        string cutName = $"{Cut(baseName, maxLength - suffix.Length - 1)}-{suffix}";
        LogEntityNameCut(kind, name, maxLength, cutName);

        return cutName;
    }

    private static string Cut(string name, int maxLength)
    {
        return name[..maxLength].TrimEnd('-');
    }

    private static string HashInstanceSuffix(string instanceSuffix)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(instanceSuffix));

        return Convert.ToHexString(hash)[..HashedInstanceSuffixLength].ToLowerInvariant();
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message =
            "{EntityKind} name '{EntityName}' is longer than {MaxLength} characters and was cut to '{CutEntityName}'"
    )]
    private partial void LogEntityNameCut(
        EntityKind entityKind,
        string entityName,
        int maxLength,
        string cutEntityName
    );
}
