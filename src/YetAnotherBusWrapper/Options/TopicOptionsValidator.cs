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

        string entity = $"Topic '{name}'";
        failures.AddRange(
            OptionValueRules.GetDuplicateDetectionWindowFailures(entity, options.DuplicateDetectionWindow)
        );
        failures.AddRange(OptionValueRules.GetClientRetryFailures(entity, options.ClientRetry));

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
