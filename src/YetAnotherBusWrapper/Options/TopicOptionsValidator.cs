using Microsoft.Extensions.Options;

namespace YetAnotherBusWrapper.Options;

internal sealed class TopicOptionsValidator : IValidateOptions<TopicOptions>
{
    public ValidateOptionsResult Validate(string? name, TopicOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Skip;
        }

        List<string> failures = [];
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            failures.Add($"Topic '{name}' requires {nameof(TopicOptions.ConnectionString)}.");
        }

        if (string.IsNullOrWhiteSpace(options.TopicName))
        {
            failures.Add($"Topic '{name}' requires {nameof(TopicOptions.TopicName)}.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
