namespace YetAnotherBusWrapper.Tests.Integration.Features.Sending.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<ISendingTestCase> Generate()
    {
        yield return new T01SingleMessage();
        yield return new T02ConfiguredJsonSerializerOptions();
        yield return new T03ReplacedMessageSerializer();
        yield return new T04MessagePropertiesFromContent();
        yield return new T05ActionRunsAfterSerialization();
        yield return new T06SanitizedMessageIdLongerThanLimit();
        yield return new T07SanitizedMessageIdWithoutLettersOrDigits();
        yield return new T08SanitizedMessageIdWithNonAsciiLetters();
    }
}
