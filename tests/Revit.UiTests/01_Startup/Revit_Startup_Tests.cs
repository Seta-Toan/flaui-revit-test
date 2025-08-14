using NUnit.Framework;
using Revit.UiTests.Setup;

namespace Revit.UiTests.Startup;

[TestFixture]
[Category("Startup")]
public class RevitStartupTests
{
    [Test]
    [Retry(1)]
    [Timeout(60000)]
    public void Revit_MainWindow_ShouldBeVisible()
    {
        var mw = GlobalSetup.Revit!.MainWindow;
        Assert.That(mw, Is.Not.Null);
        Assert.That(mw.IsEnabled, Is.True);
    }
}
