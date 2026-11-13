using Microsoft.Extensions.Options;

namespace YetAnotherBusWrapper.Options;

internal sealed class QueueConsumerOptionsValidator : IValidateOptions<QueueConsumerOptions>
{
    private readonly IOptionsFactory<QueueOptions> _queueOptionsFactory;

    public QueueConsumerOptionsValidator(IOptionsFactory<QueueOptions> queueOptionsFactory)
    {
        _queueOptionsFactory = queueOptionsFactory;
    }

    public ValidateOptionsResult Validate(string? name, QueueConsumerOptions options)
    {
        if (IsQueueValidAndDisabled(name))
        {
            return ValidateOptionsResult.Skip;
        }

        List<string> failures = [.. OptionValueRules.GetConsumerProcessingFailures($"Queue consumer '{name}'", options)];

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private bool IsQueueValidAndDisabled(string? name)
    {
        if (name is null)
        {
            return false;
        }

        try
        {
            return !_queueOptionsFactory.Create(name).Enabled;
        }
        catch (OptionsValidationException)
        {
            return false;
        }
    }
}
