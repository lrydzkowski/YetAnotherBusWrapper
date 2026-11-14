namespace YetAnotherBusWrapper.Options;

internal static class AdministrationConnectionStringResolver
{
    public static string Resolve(string connectionString, string? administrationConnectionString)
    {
        return string.IsNullOrWhiteSpace(administrationConnectionString)
            ? connectionString
            : administrationConnectionString;
    }
}
