using YetAnotherBusWrapper.Tests.Integration.Common.Emulator;

namespace YetAnotherBusWrapper.Tests.Integration.Common.TestCollections;

[CollectionDefinition(CollectionName)]
public class EmulatorTestCollection1 : ICollectionFixture<ServiceBusEmulatorFixture>
{
    public const string CollectionName = "Integration.Emulator.1";
}
