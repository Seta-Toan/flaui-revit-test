using NUnit.Framework;
using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Revit.UiPages.Dialogs;
using Revit.Automation.Core.Utils;
using Revit.Automation.Core.Config;
using Revit.UiTests.Setup;

namespace Revit.UiTests;

[TestFixture]
[Category("CompleteWorkflow")]
public class CompleteWorkflowTest
{
    [Test, Order(1)]
    [Timeout(700000)]
    [Retry(1)]
    public void E2E_Revit_Startup_To_Project_Selection()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Khởi động Revit và tới trang chọn project...");

        // 1) Khởi động Revit từ GlobalSetup
        var revit = GlobalSetup.Revit;
        Assert.That(revit.App, Is.Not.Null, "Revit application chưa khởi tạo");
        Assert.That(revit.Uia, Is.Not.Null, "UIA automation chưa khởi tạo");

        // 2) DIALOGS ĐÃ ĐƯỢC XỬ LÝ BỞI GLOBALSETUP
        TestContext.Progress.WriteLine("✅ CompleteWorkflowTest: Startup dialogs đã được xử lý bởi GlobalSetup");

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
        Assert.That(onProjectSelection, Is.True, "Không ở trang project selection");

        TestContext.Progress.WriteLine($"✅ E2E hoàn tất trong {(DateTime.Now - startTime).TotalSeconds:F1}s");
    }

    [Test, Order(2)]
    [Timeout(120000)] 
    [Retry(1)]
    public void E2E_Open_Sample_Project()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Mở project...");

        var revit = GlobalSetup.Revit;
        Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
        var mainWindow = revit.MainWindow;
        Assert.That(mainWindow, Is.Not.Null, "Không tìm thấy main window");

        var homePage = new Revit.UiPages.Pages.RevitHomePage(mainWindow, revit.Uia!);
        Assert.That(homePage.IsLoaded(), Is.True, "Home page chưa load xong");
        
        var sampleProjectOpened = homePage.OpenFirstAvailableProject();
        Assert.That(sampleProjectOpened, Is.True, "Không thể mở project");

        TestContext.Progress.WriteLine($"✅ E2E Sample Project hoàn tất trong {(DateTime.Now - startTime).TotalSeconds:F1}s");

        var resetSuccessful = homePage.ReturnToProjectSelection();
        Assert.That(resetSuccessful, Is.True, "Không thể chuyển về trang project selection bằng Ctrl+D");
    }

    [Test, Order(3)]
    [Timeout(120000)] // 2 phút  
    [Retry(1)]
    public void E2E_Click_New_Project()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Click New Project...");

        var revit = GlobalSetup.Revit;
        Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
        var mainWindow = revit.MainWindow;
        Assert.That(mainWindow, Is.Not.Null, "Không tìm thấy main window");

        var homePage = new Revit.UiPages.Pages.RevitHomePage(mainWindow, revit.Uia!);
        Assert.That(homePage.IsLoaded(), Is.True, "Home page chưa load xong");
        
        var newProjectClicked = homePage.ClickNewProject();
        Assert.That(newProjectClicked, Is.True, "Không thể click New Project button");

        TestContext.Progress.WriteLine($"✅ E2E New Project hoàn tất trong {(DateTime.Now - startTime).TotalSeconds:F1}s");
    }

    private bool IsProjectSelectionVisible(Window mainWindow)
    {
        try
        {
            // Tìm text "Recent" để xác nhận đang ở home page
            var recentText = mainWindow.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Text).And(cf.ByName("Recent")));
            
            if (recentText != null && recentText.IsAvailable)
            {
                TestContext.Progress.WriteLine("✅ Tìm thấy 'Recent' text - đang ở project selection page");
                return true;
            }
            
            // Backup: tìm "Revit 2026" text
            var revitText = mainWindow.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Text).And(cf.ByName("Revit 2026")));
                
            if (revitText != null && revitText.IsAvailable)
            {
                TestContext.Progress.WriteLine("✅ Tìm thấy 'Revit 2026' text - đang ở project selection page");
                return true;
            }
            
            TestContext.Progress.WriteLine("⚠️ Không tìm thấy markers của project selection page");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ Lỗi khi kiểm tra project selection: {ex.Message}");
            return false;
        }
    }
}
