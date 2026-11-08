using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

namespace YetAnotherBusWrapper.Tests.Integration.Common.TestCollections;

[CollectionDefinition(CollectionName)]
public class EmulatorTestCollection3 : ICollectionFixture<ServiceBusEmulatorFixture>
{
    public const string CollectionName = "Integration.Emulator.3";
}
