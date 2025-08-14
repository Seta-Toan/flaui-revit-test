using System.Diagnostics;
using FlaUI.Core;
using FlaUI.UIA3;
using Revit.Automation.Config;
using Revit.Automation.Waits;

namespace Revit.Automation.Drivers;

public class RevitProcess : IDisposable
{
    public Application? App { get; private set; }
    public UIA3Automation? Uia { get; private set; }

    public void StartOrAttach()
    {
        // Ưu tiên attach nếu đã chạy
        var existing = Process.GetProcessesByName("Revit").FirstOrDefault();
        App = existing != null ? Application.Attach(existing) : Application.Launch(TestConfig.RevitExe);

        Uia = new UIA3Automation();

        // Chờ MainWindow sẵn sàng (có title, enabled)
        UiWait.Until(() =>
        {
            var mw = App!.GetMainWindow(Uia, retry: false);
            return mw != null && mw.Properties.IsEnabled.ValueOrDefault;
        }, TestConfig.StartTimeout, TestConfig.PollInterval);
    }

    public FlaUI.Core.AutomationElements.Window MainWindow =>
        App!.GetMainWindow(Uia!, retry: false);

    public void Dispose()
    {
        Uia?.Dispose();
        // Không Kill Revit để tiết kiệm thời gian giữa các test; 
        // nếu cần, bạn có thể đóng ở OneTimeTearDown.
    }
}
