namespace YetAnotherBusWrapper.Sending;

public static class MessageIdSanitizer
{
    private const int MaxLength = 128;

    public static string Sanitize(string? value)
    {
        string sanitizedValue = new(
            (value ?? string.Empty)
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .Take(MaxLength)
            .ToArray()
        );

        return sanitizedValue.Length > 0 ? sanitizedValue : Guid.NewGuid().ToString();
    }
}
