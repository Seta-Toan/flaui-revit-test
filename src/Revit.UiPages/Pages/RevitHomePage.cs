using System;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Revit.Automation.Core.Config;
using Revit.Automation.Core.Utils;
using NUnit.Framework;

namespace Revit.UiPages.Pages;

public class RevitHomePage
{
    private readonly Window _mainWindow;
    private readonly UIA3Automation _automation;

    public RevitHomePage(Window mainWindow, UIA3Automation automation)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _automation = automation ?? throw new ArgumentNullException(nameof(automation));
    }

    public bool IsLoaded()
    {
        try
        {
            TestContext.Progress.WriteLine("🔍 RevitHomePage: Kiểm tra trang home có load xong...");
            
            var isRecentVisible = UiWaits.Until(() =>
            {
                var recentText = _mainWindow.FindFirstDescendant(cf => cf.ByName("Recent"));
                return recentText != null && recentText.IsAvailable;
            }, TimeSpan.FromSeconds(10), TestConfig.PollInterval);

            if (isRecentVisible)
            {
                TestContext.Progress.WriteLine("✅ RevitHomePage: Trang home đã load xong");
                return true;
            }

            TestContext.Progress.WriteLine("⚠️ RevitHomePage: Trang home chưa load xong");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi kiểm tra trang home: {ex.Message}");
            return false;
        }
    }

    public bool ClickNewProject()
    {
        try
        {
            TestContext.Progress.WriteLine("🔍 RevitHomePage: Tìm và click 'New ...' button...");

            var newButtonFound = UiWaits.Until(() =>
            {
                foreach (var buttonName in TestConfig.UiAliases.NewButtonNames)
                {
                    var newButton = _mainWindow.FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.Button).And(cf.ByName(buttonName)));
                    
                    if (newButton != null && newButton.IsAvailable)
                    {
                        TestContext.Progress.WriteLine($"✅ RevitHomePage: Tìm thấy 'New ...' button: '{buttonName}'");
                        newButton.Click();
                        return true;
                    }
                }
                return false;
            }, TimeSpan.FromSeconds(10), TestConfig.PollInterval);

            if (newButtonFound)
            {
                TestContext.Progress.WriteLine("✅ RevitHomePage: Đã click 'New ...' button");
                return true;
            }

            TestContext.Progress.WriteLine("❌ RevitHomePage: Không tìm thấy 'New ...' button");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi click New: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Tạo project mới thông qua New Project dialog
    /// </summary>
    public bool CreateNewProject(string projectName = "TestProject", string templatePath = null!)
    {
        try
        {
            TestContext.Progress.WriteLine("🔍 RevitHomePage: Bắt đầu tạo project mới...");

            var newButtonClicked = ClickNewProject();
            if (!newButtonClicked)
            {
                TestContext.Progress.WriteLine("❌ RevitHomePage: Không thể click New Project button");
                return false;
            }

            var newProjectDialog = new Revit.UiPages.Dialogs.NewProjectDialog(_automation);
            var projectCreated = newProjectDialog.CreateNewProject(projectName, templatePath);
            
            if (!projectCreated)
            {
                TestContext.Progress.WriteLine("❌ RevitHomePage: Không thể tạo project mới");
                return false;
            }
            System.Threading.Thread.Sleep(10000);
            TestContext.Progress.WriteLine("✅ RevitHomePage: Project mới đã được tạo thành công");
            return true;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi tạo project mới: {ex.Message}");
            return false;
        }
    }
}
