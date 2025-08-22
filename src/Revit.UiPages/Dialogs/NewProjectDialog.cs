using System;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Revit.Automation.Core.Config;
using Revit.Automation.Core.Utils;
using NUnit.Framework;
using System.IO;

namespace Revit.UiPages.Dialogs;

public class NewProjectDialog
{
    private readonly UIA3Automation _automation;

    public NewProjectDialog(UIA3Automation automation)
    {
        _automation = automation ?? throw new ArgumentNullException(nameof(automation));
    }

    /// <summary>
    /// Xử lý New Project dialog và tạo project mới
    /// </summary>
    public bool CreateNewProject(string projectName = "TestProject", string templatePath = null!)
    {
        try
        {
            TestContext.Progress.WriteLine("🔍 NewProjectDialog: Tìm New Project dialog...");

            var dialog = WaitForNewProjectDialog();
            if (dialog == null)
            {
                TestContext.Progress.WriteLine("❌ NewProjectDialog: Không tìm thấy New Project dialog");
                return false;
            }

            TestContext.Progress.WriteLine("✅ NewProjectDialog: Đã tìm thấy New Project dialog");

            var okButton = dialog.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Button).And(cf.ByName("OK")));
            
            if (okButton != null && okButton.IsAvailable)
            {
                okButton.Click();
                TestContext.Progress.WriteLine("✅ NewProjectDialog: Đã click OK button");
            }
            else
            {
                TestContext.Progress.WriteLine("❌ NewProjectDialog: Không tìm thấy OK button");
                return false;
            }

            TestContext.Progress.WriteLine("⏳ NewProjectDialog: Chờ project load hoàn toàn...");
            System.Threading.Thread.Sleep(15000);

            TestContext.Progress.WriteLine("💾 NewProjectDialog: Focus vào Revit window...");
            var revitWindow = FindRevitWindow();
            if (revitWindow != null)
            {
                revitWindow.Focus();
                System.Threading.Thread.Sleep(2000);
            }

            TestContext.Progress.WriteLine("💾 NewProjectDialog: Sử dụng SaveAsDialog để lưu project...");
            var saveAsDialog = new SaveAsDialog(_automation, revitWindow!);
            var projectSaved = saveAsDialog.SaveProject(projectName);
            
            if (!projectSaved)
            {
                TestContext.Progress.WriteLine("⚠️ NewProjectDialog: Không thể lưu project, nhưng project đã được tạo");
                return false;
            }

            TestContext.Progress.WriteLine("✅ NewProjectDialog: Project mới đã được tạo và lưu thành công");
            return true;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ NewProjectDialog: Lỗi khi tạo project mới: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Chờ New Project dialog xuất hiện
    /// </summary>
    private Window? WaitForNewProjectDialog()
    {
        TestContext.Progress.WriteLine("🔍 NewProjectDialog: Bắt đầu tìm dialog trong Revit...");
        
        // Sử dụng UiWaits.Until để chờ dialog xuất hiện
        var dialogFound = UiWaits.Until(() =>
        {
            try
            {
                // Tìm Revit main window trước
                var revitWindow = _automation.GetDesktop().FindFirstChild(cf => 
                    cf.ByControlType(ControlType.Window).And(cf.ByName("Autodesk Revit 2026.2 - UNREGISTERED VERSION - [Home]")));
                
                if (revitWindow == null)
                {
                    TestContext.Progress.WriteLine("⚠️ NewProjectDialog: Không tìm thấy Revit main window");
                    return false;
                }

                // Tìm tất cả dialogs CHỈ trong Revit window
                var dialogsInRevit = revitWindow.FindAllChildren(cf => 
                    cf.ByControlType(ControlType.Window));
                
                TestContext.Progress.WriteLine($"🔍 NewProjectDialog: Tìm thấy {dialogsInRevit.Length} dialogs trong Revit:");
                
                foreach (var dialog in dialogsInRevit)
                {
                    if (dialog.IsAvailable)
                    {
                        TestContext.Progress.WriteLine($"  - '{dialog.Name}' (Type: {dialog.ControlType})");
                        
                        // Kiểm tra nếu tên chứa "New Project" hoặc "New"
                        if (dialog.Name.Contains("New Project") || dialog.Name.Contains("New"))
                        {
                            TestContext.Progress.WriteLine($"✅ NewProjectDialog: Tìm thấy dialog phù hợp: '{dialog.Name}'");
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"⚠️ NewProjectDialog: Lỗi khi tìm dialog: {ex.Message}");
                return false;
            }
        }, TimeSpan.FromSeconds(15), TestConfig.PollInterval);

        // Sau khi tìm thấy, lấy Window object
        if (dialogFound)
        {
            var revitWindow = _automation.GetDesktop().FindFirstChild(cf => 
                cf.ByControlType(ControlType.Window).And(cf.ByName("Autodesk Revit 2026.2 - UNREGISTERED VERSION - [Home]")));
            
            var dialogsInRevit = revitWindow!.FindAllChildren(cf => 
                cf.ByControlType(ControlType.Window));
            
            foreach (var dialog in dialogsInRevit)
            {
                if (dialog.IsAvailable && (dialog.Name.Contains("New Project") || dialog.Name.Contains("New")))
                {
                    TestContext.Progress.WriteLine($"✅ NewProjectDialog: Trả về dialog: '{dialog.Name}'");
                    return dialog.AsWindow();
                }
            }
        }

        TestContext.Progress.WriteLine("❌ NewProjectDialog: Không tìm thấy dialog phù hợp trong Revit");
        return null;
    }

    /// <summary>
    /// Click OK button
    /// </summary>
    private bool ClickOK(Window dialog)
    {
        try
        {
            var okButton = dialog.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Button).And(cf.ByName("OK")));
            
            if (okButton != null && okButton.IsAvailable)
            {
                okButton.Click();
                TestContext.Progress.WriteLine("✅ NewProjectDialog: Đã click OK button");
                return true;
            }

            TestContext.Progress.WriteLine("❌ NewProjectDialog: Không tìm thấy OK button");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ NewProjectDialog: Lỗi khi click OK: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Tìm Revit window hiện tại
    /// </summary>
    private Window? FindRevitWindow()
    {
        try
        {
            var allWindows = _automation.GetDesktop().FindAllChildren(cf => 
                cf.ByControlType(ControlType.Window));
            
            foreach (var window in allWindows)
            {
                if (window.IsAvailable && window.Name.Contains("Autodesk Revit 2026.2 - UNREGISTERED VERSION"))
                {
                    return window.AsWindow();
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ NewProjectDialog: Lỗi khi tìm Revit window: {ex.Message}");
            return null;
        }
    }
}
