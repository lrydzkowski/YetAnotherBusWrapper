using Microsoft.Extensions.Options;
using YetAnotherBusWrapper.Naming;
using YetAnotherBusWrapper.Sample.Api.ServiceBus.Options;

namespace YetAnotherBusWrapper.Sample.Api.ServiceBus;

internal sealed class ConfiguredInstanceNameProvider : IInstanceNameProvider
{
    private readonly string _instanceName;

    public ConfiguredInstanceNameProvider(IOptions<SampleOptions> options)
    {
        string? instanceName = options.Value.InstanceName;
        _instanceName = string.IsNullOrWhiteSpace(instanceName) ? Environment.MachineName : instanceName;
    }

    public string GetInstanceName()
    {
        return _instanceName;
    }
}
