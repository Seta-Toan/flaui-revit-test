using NUnit.Framework;
using Revit.Automation.Drivers;

namespace Revit.UiTests.Setup;

[SetUpFixture]
public sealed class GlobalSetup
{
    public static RevitProcess? Revit;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Revit = new RevitProcess();
        Revit.StartOrAttach();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Revit?.Dispose();
    }
}
