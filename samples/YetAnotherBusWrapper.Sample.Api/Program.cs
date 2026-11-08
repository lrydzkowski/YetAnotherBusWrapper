using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using YetAnotherBusWrapper;
using YetAnotherBusWrapper.Sample.Api;
using YetAnotherBusWrapper.Sample.Api.HealthChecks;
using YetAnotherBusWrapper.Sample.Api.ServiceBus;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSampleApplication();
builder.Services.AddSampleServiceBus(builder.Configuration);
builder.Services.AddHealthChecks().AddYetAnotherBusWrapper();

WebApplication app = builder.Build();
app.MapSampleEndpoints();
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = HealthReportJsonWriter.WriteAsync });
app.Run();
