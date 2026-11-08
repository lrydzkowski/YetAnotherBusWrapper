using System.Runtime.CompilerServices;
using DiffEngine;

namespace YetAnotherBusWrapper.Tests.Integration.Common;

internal static class TestInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        DiffRunner.MaxInstancesToLaunch(100);
    }
}
