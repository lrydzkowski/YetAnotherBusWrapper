namespace YetAnotherBusWrapper.Naming;

internal sealed class EnvironmentInstanceNameProvider : IInstanceNameProvider
{
    private const string WebsiteInstanceIdVariable = "WEBSITE_INSTANCE_ID";

    public string GetInstanceName()
    {
        string? websiteInstanceId = Environment.GetEnvironmentVariable(WebsiteInstanceIdVariable);
        if (!string.IsNullOrWhiteSpace(websiteInstanceId))
        {
            return websiteInstanceId.Trim().ToLowerInvariant();
        }

        return Environment.MachineName.ToLowerInvariant();
    }
}
