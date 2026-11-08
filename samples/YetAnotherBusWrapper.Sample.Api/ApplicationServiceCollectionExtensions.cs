using System.Text.Json.Serialization;
using YetAnotherBusWrapper.Sample.Api.Features.Orders;
using YetAnotherBusWrapper.Sample.Api.Features.Prices;
using YetAnotherBusWrapper.Sample.Api.Features.Received;

namespace YetAnotherBusWrapper.Sample.Api;

internal static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddSampleApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ReceivedMessageLog>();
        services.AddSingleton<PriceCache>();
        services.AddSingleton<OrderService>();
        services.AddProblemDetails();
        services.ConfigureHttpJsonOptions(
            options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
        );

        return services;
    }
}
