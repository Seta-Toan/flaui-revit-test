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
    public bool UseKeyboardShortcut()
    {
        try
        {
            TestContext.Progress.WriteLine("⌨️ RevitHomePage: Sử dụng tổ hợp phím Ctrl+D...");
            
            // Focus vào main window trước
            _mainWindow.Focus();
            System.Threading.Thread.Sleep(500); // Chờ focus
            
            // Gửi tổ hợp phím Ctrl+D
            System.Windows.Forms.SendKeys.SendWait("^d");
            System.Threading.Thread.Sleep(1000); // Chờ xử lý
            
            TestContext.Progress.WriteLine("✅ RevitHomePage: Đã gửi tổ hợp phím Ctrl+D");
            return true;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi sử dụng tổ hợp phím: {ex.Message}");
            return false;
        }
    }

    public bool ReturnToProjectSelection()
    {
        try
        {
            TestContext.Progress.WriteLine("🔄 RevitHomePage: Sử dụng Ctrl+D để về trang project selection...");
            
            // Sử dụng Ctrl+D
            var success = UseKeyboardShortcut();
            
            if (success)
            {
                TestContext.Progress.WriteLine("✅ RevitHomePage: Ctrl+D đã được thực thi thành công");
                
                // Chờ và kiểm tra xem đã về trang project selection chưa
                TestContext.Progress.WriteLine("⏳ RevitHomePage: Chờ chuyển về trang project selection...");
                var returnedToProjectSelection = UiWaits.Until(() =>
                {
                    try
                    {
                        return IsProjectSelectionVisible();
                    }
                    catch
                    {
                        return false;
                    }
                }, TimeSpan.FromSeconds(15), TestConfig.PollInterval);

                if (returnedToProjectSelection)
                {
                    TestContext.Progress.WriteLine("✅ RevitHomePage: Đã chuyển về trang project selection thành công");
                    return true;
                }
                else
                {
                    TestContext.Progress.WriteLine("⚠️ RevitHomePage: Ctrl+D đã thực thi nhưng chưa về trang project selection");
                    return false;
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi sử dụng Ctrl+D: {ex.Message}");
            return false;
        }
    }
    private bool IsProjectSelectionVisible()
    {
        try
        {
            // Tìm text "Recent" để xác nhận đang ở home page
            var recentText = _mainWindow.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Text).And(cf.ByName("Recent")));
            
            if (recentText != null && recentText.IsAvailable)
            {
                return true;
            }
            
            // Backup: tìm "Revit 2026" text
            var revitText = _mainWindow.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Text).And(cf.ByName("Revit 2026")));
                
            if (revitText != null && revitText.IsAvailable)
            {
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ Lỗi khi kiểm tra project selection: {ex.Message}");
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

            // 3. Chờ rời khỏi trang Home - SỬA LOGIC NÀY
            TestContext.Progress.WriteLine("⏳ RevitHomePage: Chờ rời khỏi trang Home...");
            
            // Chờ một chút để project load
            System.Threading.Thread.Sleep(5000);
            
            // Kiểm tra đơn giản: nếu không còn ở home thì OK
            var isStillHome = IsLoaded();
            if (!isStillHome)
            {
                TestContext.Progress.WriteLine("✅ RevitHomePage: Đã rời khỏi trang Home, project mới đang được tạo");
                return true;
            }
            else
            {
                TestContext.Progress.WriteLine("⚠️ RevitHomePage: Vẫn còn ở trang Home sau khi tạo project");
                return false;
            }
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi tạo project mới: {ex.Message}");
            return false;
        }
    }
}
