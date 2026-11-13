using Microsoft.Extensions.Options;

namespace YetAnotherBusWrapper.Options;

internal sealed class QueueOptionsValidator : IValidateOptions<QueueOptions>
{
    public ValidateOptionsResult Validate(string? name, QueueOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Skip;
        }

        List<string> failures = [];
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            failures.Add($"Queue '{name}' requires {nameof(QueueOptions.ConnectionString)}.");
        }

        if (string.IsNullOrWhiteSpace(options.QueueName))
        {
            failures.Add($"Queue '{name}' requires {nameof(QueueOptions.QueueName)}.");
        }

        string entity = $"Queue '{name}'";
        failures.AddRange(OptionValueRules.GetMaxDeliveryCountFailures(entity, options.MaxDeliveryCount));
        failures.AddRange(
            OptionValueRules.GetDuplicateDetectionWindowFailures(entity, options.DuplicateDetectionWindow)
        );
        failures.AddRange(OptionValueRules.GetClientRetryFailures(entity, options.ClientRetry));

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
