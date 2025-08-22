using NUnit.Framework;
using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Revit.UiTests.Setup;
using Revit.UiPages.Pages;

namespace Revit.UiTests;

[TestFixture]
[Category("CompleteWorkflow")]
public class CompleteWorkflowTest
{
    [Test, Order(1)]
    [Timeout(120000)]
    [Retry(1)]
    public void E2E_Revit_Startup_To_Project_Selection()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Khởi động Revit và tới trang chọn project...");

        var revit = GlobalSetup.Revit;
        Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
        var mainWindow = revit.MainWindow;
        Assert.That(mainWindow, Is.Not.Null, "Không tìm thấy main window");

        var homePage = new RevitHomePage(mainWindow, revit.Uia!);
        
        var onProjectSelection = homePage.IsLoaded();
        Assert.That(onProjectSelection, Is.True, "Home page chưa load xong");

        var duration = DateTime.Now - startTime;
        TestContext.Progress.WriteLine($"✅ E2E hoàn tất trong {duration.TotalSeconds:F1}s");
    }

    [Test, Order(2)]
    [Timeout(70000)] 
    [Retry(1)]
    public void E2E_Click_New_Project()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Click New Project...");

        var revit = GlobalSetup.Revit;
        Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
        var mainWindow = revit.MainWindow;
        Assert.That(mainWindow, Is.Not.Null, "Không tìm thấy main window");

        var homePage = new RevitHomePage(mainWindow, revit.Uia!);
        Assert.That(homePage.IsLoaded(), Is.True, "Home page chưa load xong");
        
        var newProjectClicked = homePage.ClickNewProject();
        Assert.That(newProjectClicked, Is.True, "Không thể click New Project button");

        TestContext.Progress.WriteLine($"✅ E2E New Project hoàn tất trong {(DateTime.Now - startTime).TotalSeconds:F1}s");
    }

    [Test, Order(3)]
    [Timeout(120000)]
    [Retry(1)]
    public void E2E_Create_New_Project_And_Save_To_TestData()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Tạo project mới và lưu vào test data...");

        var revit = GlobalSetup.Revit;
        Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
        var mainWindow = revit.MainWindow;
        Assert.That(mainWindow, Is.Not.Null, "Không tìm thấy main window");

        var homePage = new RevitHomePage(mainWindow, revit.Uia!);
        Assert.That(homePage.IsLoaded(), Is.True, "Home page chưa load xong");
        
        // 1. Tạo project mới (đã bao gồm Ctrl+S để hiển thị Save As dialog)
        var projectCreated = homePage.CreateNewProject("TestProject_Automated");
        Assert.That(projectCreated, Is.True, "Không thể tạo project mới");

        TestContext.Progress.WriteLine("✅ E2E Create New Project hoàn tất - Save As dialog đã được hiển thị");
        TestContext.Progress.WriteLine($"✅ E2E Create New Project hoàn tất trong {(DateTime.Now - startTime).TotalSeconds:F1}s");
    }
}
