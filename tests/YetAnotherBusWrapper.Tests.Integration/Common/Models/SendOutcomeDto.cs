using YetAnotherBusWrapper.Abstractions;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Models;

internal record SendOutcomeDto(SendResult? Result, ThrownException? ThrownException)
{
    public static async Task<SendOutcomeDto> CaptureAsync(Func<Task<SendResult>> sendAsync)
    {
        try
        {
            return new SendOutcomeDto(await sendAsync(), null);
        }
        catch (Exception exception)
        {
            return new SendOutcomeDto(null, ThrownException.From(exception));
        }
    }
}
