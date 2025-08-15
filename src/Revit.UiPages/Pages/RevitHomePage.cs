using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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

    /// <summary>
    /// Mở project đầu tiên có sẵn (không cần chỉ định tên cụ thể)
    /// </summary>
    public bool OpenFirstAvailableProject()
    {
        try
        {
            TestContext.Progress.WriteLine("🔍 RevitHomePage: Tìm project đầu tiên có sẵn...");

            // Tìm tất cả project cards
            var projectFound = UiWaits.Until(() =>
            {
                var allElements = _mainWindow.FindAllDescendants();
                var availableProjects = new List<AutomationElement>();

                foreach (var element in allElements)
                {
                    try
                    {
                        if (element.IsAvailable)
                        {
                            var name = element.Name ?? "";
                            var controlType = element.ControlType;
                            if (IsValidProjectCard(name, controlType))
                            {
                                availableProjects.Add(element);
                                TestContext.Progress.WriteLine($"🔍 RevitHomePage: Tìm thấy project candidate: '{name}' (Type: {controlType})");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TestContext.Progress.WriteLine($"⚠️ RevitHomePage: Lỗi khi xử lý element: {ex.Message}");
                    }
                }

                TestContext.Progress.WriteLine($"📊 RevitHomePage: Tìm thấy {availableProjects.Count} project candidates");

                if (availableProjects.Count > 0)
                {
                    // Chọn project đầu tiên
                    var firstProject = availableProjects[0];
                    var projectName = firstProject.Name ?? "Unknown Project";
                    
                    TestContext.Progress.WriteLine($"✅ RevitHomePage: Chọn project đầu tiên: '{projectName}'");
                    
                    try
                    {
                        firstProject.Click();
                        TestContext.Progress.WriteLine($"✅ RevitHomePage: Đã click trực tiếp vào project '{projectName}'");
                        return true;
                    }
                    catch (Exception directClickEx)
                    {
                        TestContext.Progress.WriteLine($"⚠️ RevitHomePage: Lỗi khi click trực tiếp: {directClickEx.Message}");
                    }
                }

                return false;
            }, TimeSpan.FromSeconds(15), TestConfig.PollInterval);

            if (projectFound)
            {
                // Chờ trang project load
                TestContext.Progress.WriteLine("⏳ RevitHomePage: Chờ 50 giây để trang project load...");
                System.Threading.Thread.Sleep(50000); 
                TestContext.Progress.WriteLine("✅ RevitHomePage: Hoàn tất chờ project load");
                
                return true;
            }

            TestContext.Progress.WriteLine("❌ RevitHomePage: Không tìm thấy project nào có sẵn");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi mở project: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Kiểm tra xem element có phải là project card hợp lệ không
    /// </summary>
    private bool IsValidProjectCard(string name, ControlType controlType)
    {
        // Loại bỏ các element không phải project
        if (string.IsNullOrEmpty(name) || 
            name.Contains("Autodesk") || 
            name.Contains("Cloud") ||
            name.Contains("Recent") ||
            name.Contains("Sort by") ||
            name.Contains("Search"))
        {
            return false;
        }

        // Chỉ chấp nhận các control type phù hợp với project cards
        if (controlType != ControlType.Group && 
            controlType != ControlType.Custom && 
            controlType != ControlType.Pane &&
            controlType != ControlType.ListItem)
        {
            return false;
        }

        // Kiểm tra tên có chứa từ khóa project không
        var lowerName = name.ToLower();
        return lowerName.Contains("sample") || 
               lowerName.Contains("project") || 
               lowerName.Contains("template") ||
               lowerName.Contains("architectural") ||
               lowerName.Contains("structural") ||
               lowerName.Contains("hvac") ||
               lowerName.Contains("plumbing") ||
               lowerName.Contains("electrical") ||
               lowerName.Contains("family");
    }

    // /// <summary>
    // /// Mở project theo tên cụ thể (giữ lại để tương thích ngược)
    // /// </summary>
    // public bool OpenExistingProject(string projectName)
    // {
    //     try
    //     {
    //         TestContext.Progress.WriteLine($"🔍 RevitHomePage: Tìm và mở project '{projectName}'...");

    //         // Tìm project card theo tên
    //         var projectFound = UiWaits.Until(() =>
    //         {
    //             var allElements = _mainWindow.FindAllDescendants();
    //             foreach (var element in allElements)
    //             {
    //                 try
    //                 {
    //                     var name = element.Name ?? "";
    //                     if (name.Contains(projectName) && element.IsAvailable)
    //                     {
    //                         TestContext.Progress.WriteLine($"✅ RevitHomePage: Tìm thấy project '{name}'");
    //                         element.AsButton()?.Click();
    //                         return true;
    //                     }
    //                 }
    //                 catch { }
    //             }
    //             return false;
    //         }, TimeSpan.FromSeconds(15), TestConfig.PollInterval);

    //         if (projectFound)
    //         {
    //             TestContext.Progress.WriteLine($"✅ RevitHomePage: Đã click vào project '{projectName}'");
                
    //             // Chờ trang project load
    //             TestContext.Progress.WriteLine("⏳ RevitHomePage: Chờ 50 giây để trang project load...");
    //             System.Threading.Thread.Sleep(50000); 
    //             TestContext.Progress.WriteLine("✅ RevitHomePage: Hoàn tất chờ project load");
                
    //             return true;
    //         }

    //         TestContext.Progress.WriteLine($"❌ RevitHomePage: Không tìm thấy project '{projectName}'");
    //         return false;
    //     }
    //     catch (Exception ex)
    //     {
    //         TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi mở project: {ex.Message}");
    //         return false;
    //     }
    // }

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

    /// <summary>
    /// Lấy danh sách tất cả projects có sẵn
    /// </summary>
    public List<string> GetAvailableProjects()
    {
        var availableProjects = new List<string>();
        
        try
        {
            TestContext.Progress.WriteLine("🔍 RevitHomePage: Quét tất cả projects có sẵn...");
            
            var allElements = _mainWindow.FindAllDescendants();
            
            foreach (var element in allElements)
            {
                try
                {
                    if (element.IsAvailable)
                    {
                        var name = element.Name ?? "";
                        var controlType = element.ControlType;
                        
                        // Sử dụng logic mới để kiểm tra project card
                        if (IsValidProjectCard(name, controlType))
                        {
                            availableProjects.Add(name);
                            TestContext.Progress.WriteLine($"🔍 RevitHomePage: Tìm thấy project: '{name}' (Type: {controlType})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Progress.WriteLine($"⚠️ RevitHomePage: Lỗi khi xử lý element: {ex.Message}");
                }
            }
            
            TestContext.Progress.WriteLine($"📊 RevitHomePage: Tổng cộng tìm thấy {availableProjects.Count} projects");
            
            // Loại bỏ duplicates
            var uniqueProjects = availableProjects.Distinct().ToList();
            if (uniqueProjects.Count != availableProjects.Count)
            {
                TestContext.Progress.WriteLine($"📊 RevitHomePage: Sau khi loại bỏ duplicates: {uniqueProjects.Count} projects");
            }
            
            return uniqueProjects;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitHomePage: Lỗi khi quét projects: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Kiểm tra xem có đang ở trang project selection không
    /// </summary>
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
}
