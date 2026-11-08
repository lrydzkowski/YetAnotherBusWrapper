using YetAnotherBusWrapper.Tests.Integration.Common.Messages;

namespace YetAnotherBusWrapper.Tests.Integration.Features.Batching.Data;

internal record SentReportDto(int Number, int ContentLength)
{
    public static SentReportDto From(ReportGenerated message)
    {
        return new SentReportDto(message.Number, message.Content.Length);
    }
}
