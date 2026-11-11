namespace YetAnotherBusWrapper.Extensions;

internal static class StringExtensions
{
    public static bool EqualsOrdinal(this string? value, string? other)
    {
        return string.Equals(value, other, StringComparison.Ordinal);
    }
}
