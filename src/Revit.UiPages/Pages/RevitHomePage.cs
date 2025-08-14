using System;
using System.Linq;
using FlaUI.Core;
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

    public bool OpenExistingProject(string projectName)
    {
        try
        {
            TestContext.Progress.WriteLine($"🔍 RevitHomePage: Tìm và mở project '{projectName}'...");

            // Tìm project card theo tên
            var projectFound = UiWaits.Until(() =>
            {
                var allElements = _mainWindow.FindAllDescendants();
                foreach (var element in allElements)
                {
                    try
                    {
                        var name = element.Name ?? "";
                        if (name.Contains(projectName) && element.IsAvailable)
                        {
                            TestContext.Progress.WriteLine($"✅ RevitHomePage: Tìm thấy project '{name}'");
                            element.AsButton()?.Click();
                            return true;
                        }
                    }
                    catch { }
                }
                return false;
            }, TimeSpan.FromSeconds(15), TestConfig.PollInterval);

            if (projectFound)
            {
                TestContext.Progress.WriteLine($"✅ RevitHomePage: Đã click vào project '{projectName}'");
                
                // Chờ trang project load
                TestContext.Progress.WriteLine("⏳ RevitHomePage: Chờ 50 giây để trang project load...");
                System.Threading.Thread.Sleep(50000); 
                TestContext.Progress.WriteLine("✅ RevitHomePage: Hoàn tất chờ project load");
                
                return true;
            }

            TestContext.Progress.WriteLine($"❌ RevitHomePage: Không tìm thấy project '{projectName}'");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi mở project: {ex.Message}");
            return false;
        }
    }

    public bool ClickNewProject()
    {
        try
        {
            TestContext.Progress.WriteLine("🔍 RevitHomePage: Tìm và click 'New ...' button...");

            // Tìm button "New ..." trong Models section
            var newButtonFound = UiWaits.Until(() =>
            {
                var newButton = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Button).And(cf.ByName("New ...")));
                
                if (newButton != null && newButton.IsAvailable)
                {
                    TestContext.Progress.WriteLine("✅ RevitHomePage: Tìm thấy 'New ...' button");
                    newButton.Click();
                    return true;
                }
                return false;
            }, TimeSpan.FromSeconds(10), TestConfig.PollInterval);

            if (newButtonFound)
            {
                TestContext.Progress.WriteLine("✅ RevitHomePage: Đã click 'New ...' button");
                return true;
            }

            //  Fallback - tìm theo text content
            TestContext.Progress.WriteLine("🔍 RevitHomePage: Fallback - tìm theo text 'New'...");
            var textNewFound = UiWaits.Until(() =>
            {
                var allElements = _mainWindow.FindAllDescendants();
                foreach (var element in allElements)
                {
                    try
                    {
                        var name = element.Name ?? "";
                        if ((name == "New ..." || name.Contains("New")) && element.IsAvailable)
                        {
                            TestContext.Progress.WriteLine($"✅ RevitHomePage: Tìm thấy New element: '{name}'");
                            element.AsButton()?.Click();
                            return true;
                        }
                    }
                    catch { }
                }
                return false;
            }, TimeSpan.FromSeconds(10), TestConfig.PollInterval);

            if (textNewFound)
            {
                TestContext.Progress.WriteLine("✅ RevitHomePage: Đã click New button (fallback)");
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

    public bool ClickOpenProject()
    {
        try
        {
            TestContext.Progress.WriteLine("🔍 RevitHomePage: Tìm và click 'Open ...' button...");

            var openButtonFound = UiWaits.Until(() =>
            {
                var openButton = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Button).And(cf.ByName("Open ...")));
                
                if (openButton != null && openButton.IsAvailable)
                {
                    TestContext.Progress.WriteLine("✅ RevitHomePage: Tìm thấy 'Open ...' button");
                    openButton.Click();
                    return true;
                }
                return false;
            }, TimeSpan.FromSeconds(10), TestConfig.PollInterval);

            if (openButtonFound)
            {
                TestContext.Progress.WriteLine("✅ RevitHomePage: Đã click 'Open ...' button");
                return true;
            }

            TestContext.Progress.WriteLine("❌ RevitHomePage: Không tìm thấy 'Open ...' button");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi click Open: {ex.Message}");
            return false;
        }
    }
}
