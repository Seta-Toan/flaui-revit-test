using NUnit.Framework;
using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Revit.Automation.Dialogs;
using Revit.Automation.Waits;
using Revit.Automation.Config;

namespace Revit.UiTests.Setup;

[TestFixture]
[Category("CompleteWorkflow")]
public class CompleteWorkflowTest
{
    [Test]
    [Timeout(180000)] // 3 phút cho toàn bộ workflow
    [Retry(1)]
    public void E2E_Revit_Startup_To_Project_Selection()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Khởi động Revit và tới trang chọn project...");

        // 1) Khởi động/attach Revit từ GlobalSetup
        var revit = GlobalSetup.Revit;
        Assert.That(revit.App, Is.Not.Null, "Revit application chưa khởi tạo");
        Assert.That(revit.Uia, Is.Not.Null, "UIA automation chưa khởi tạo");

        // 2) GlobalSetup đã xử lý startup dialogs rồi, bỏ qua bước này
        TestContext.Progress.WriteLine("ℹ️ CompleteWorkflowTest: GlobalSetup đã xử lý startup dialogs");

        // 3) Kiểm tra main window sẵn sàng
        Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
        var mainWindow = revit.MainWindow;
        Assert.That(mainWindow, Is.Not.Null, "Không tìm thấy main window");

        // 4) Xác nhận đang ở trang chọn project (Start/Home)
        var onProjectSelection = UiWaits.Until(
            () => IsProjectSelectionVisible(mainWindow),
            TestConfig.DefaultTimeout,
            TestConfig.PollInterval
        );
        Assert.That(onProjectSelection, Is.True, "Không thấy trang chọn project (Start/Home)");

        TestContext.Progress.WriteLine($"✅ E2E hoàn tất trong {(DateTime.Now - startTime).TotalSeconds:F1}s");
    }

    private static bool IsProjectSelectionVisible(Window mainWindow)
    {
        try
        {
            // Recent section
            var recent = mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Group).And(cf.ByName("Recent")))
                ?? mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Group).And(cf.ByName("Recent Projects")))
                ?? mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Pane).And(cf.ByName("Recent")))
                ?? mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Pane).And(cf.ByName("Recent Projects")));
            if (recent != null) return true;

            // Nút "New" cục bộ (tránh Autodesk Cloud)
            var newButtons = mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
            if (newButtons.Any(b =>
            {
                try
                {
                    var name = b.Name ?? "";
                    var lower = name.ToLower();
                    return !string.IsNullOrEmpty(name)
                           && lower.Contains("new")
                           && !lower.Contains("autodesk")
                           && !lower.Contains("cloud");
                }
                catch { return false; }
            })) return true;

            // Các candidate item thể hiện danh sách project/template
            var candidates = mainWindow.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.ListItem)
                 .Or(cf.ByControlType(ControlType.Hyperlink))
                 .Or(cf.ByControlType(ControlType.Button))
                 .Or(cf.ByControlType(ControlType.Text)));

            if (candidates.Any(e =>
            {
                try
                {
                    var n = e.Name ?? "";
                    var lower = n.ToLower();
                    return n.EndsWith(".rvt", StringComparison.OrdinalIgnoreCase)
                           || lower.Contains("project")
                           || lower.Contains("revit")
                           || lower.Contains("template");
                }
                catch { return false; }
            })) return true;

            return false;
        }
        catch
        {
            return false;
        }
    }
}