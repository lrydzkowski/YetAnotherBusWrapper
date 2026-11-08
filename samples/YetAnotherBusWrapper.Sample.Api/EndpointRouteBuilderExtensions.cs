using YetAnotherBusWrapper.Sample.Api.Features.Audit;
using YetAnotherBusWrapper.Sample.Api.Features.Notifications;
using YetAnotherBusWrapper.Sample.Api.Features.Orders;
using YetAnotherBusWrapper.Sample.Api.Features.Payments;
using YetAnotherBusWrapper.Sample.Api.Features.Prices;
using YetAnotherBusWrapper.Sample.Api.Features.Received;
using YetAnotherBusWrapper.Sample.Api.Features.Reports;
using YetAnotherBusWrapper.Sample.Api.Features.Shipments;

namespace YetAnotherBusWrapper.Sample.Api;

internal static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapSampleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOrderEndpoints();
        endpoints.MapPaymentEndpoints();
        endpoints.MapShipmentEndpoints();
        endpoints.MapReportEndpoints();
        endpoints.MapAuditEndpoints();
        endpoints.MapPriceEndpoints();
        endpoints.MapNotificationEndpoints();
        endpoints.MapReceivedMessageEndpoints();

        return endpoints;
    }
}
