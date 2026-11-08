namespace YetAnotherBusWrapper.Tests.Integration.Features.EntityNames.Data.TestCases;

internal static class TestCasesGenerator
{
    public static IEnumerable<IEntityNamesTestCase> Generate()
    {
        yield return new T01WithoutInstanceSuffix();
        yield return new T02WithInstanceSuffix();
        yield return new T03InvalidCharacters();
        yield return new T04NameLongerThanLimit();
        yield return new T05LongNameWithInstanceSuffix();
        yield return new T06LongInstanceName();
        yield return new T07LeadingAndTrailingHyphens();
        yield return new T08CutEndingOnHyphen();
    }
}
