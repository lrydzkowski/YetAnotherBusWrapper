using System.Text.RegularExpressions;
using Argon;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Services;

internal static partial class VerifySettingsBuilder
{
    [GeneratedRegex(@"Endpoint=sb://[^""\s]+")]
    private static partial Regex ConnectionStringRegex();

    [GeneratedRegex(@"(localhost|127\.0\.0\.1):\d+")]
    private static partial Regex PortRegex();

    public static VerifySettings Build()
    {
        VerifySettings settings = new();
        settings.DontIgnoreEmptyCollections();
        settings.AddExtraSettings(
            jsonSerializerSettings =>
            {
                jsonSerializerSettings.NullValueHandling = NullValueHandling.Include;
                jsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            }
        );
        settings.ScrubInlineGuids();
        settings.ScrubLinesWithReplace(
            line =>
            {
                line = line.Replace(@"\r\n", @"\n");
                line = ConnectionStringRegex().Replace(line, "{ConnectionString}");
                line = PortRegex().Replace(line, "$1:{Port}");

                return line;
            }
        );

        return settings;
    }
}
