using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YetAnotherBusWrapper.Tests.Integration.Common.Logging;

namespace YetAnotherBusWrapper.Tests.Integration.Common.Host;

internal static class TestHostBuilder
{
    public static IHost Build(LogCollector logCollector, Action<IServiceCollection> configureServices)
    {
        HostApplicationBuilder builder = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(
            new HostApplicationBuilderSettings()
        );
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddProvider(new LogCollectorProvider(logCollector));
        configureServices(builder.Services);

        return builder.Build();
    }
}
