using System;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Revit.Automation.Core.Config;
using Revit.Automation.Core.Utils;
using NUnit.Framework;

namespace Revit.UiPages.Dialogs;

/// <summary>
/// Xử lý Save As dialog của Revit
/// </summary>
public class SaveAsDialog
{
    private readonly UIA3Automation _automation;
    private readonly Window _revitWindow;

    public SaveAsDialog(UIA3Automation automation, Window revitWindow)
    {
        _automation = automation ?? throw new ArgumentNullException(nameof(automation));
        _revitWindow = revitWindow ?? throw new ArgumentNullException(nameof(revitWindow));
    }

    /// <summary>
    /// Xử lý Save As dialog để lưu project
    /// </summary>
    public bool SaveProject(string projectName = "TestProject")
    {
        try
        {
            TestContext.Progress.WriteLine("💾 SaveAsDialog: Bắt đầu lưu project");

            System.Threading.Thread.Sleep(5000);
            var saveAsTriggered = TriggerSaveAs();
            if (!saveAsTriggered)
            {
                TestContext.Progress.WriteLine("❌ SaveAsDialog: Không thể mở Save As dialog");
                return false;
            }

            var saveDialog = WaitForSaveAsDialog();
            if (saveDialog == null)
            {
                TestContext.Progress.WriteLine("❌ SaveAsDialog: Save As dialog không xuất hiện");
                return false;
            }

            TestContext.Progress.WriteLine("✅ SaveAsDialog: Đã tìm thấy Save As dialog");

            var folderCreated = CreateNewFolder(saveDialog);
            if (!folderCreated)
            {
                TestContext.Progress.WriteLine("❌ SaveAsDialog: Không thể tạo folder mới");
                return false;
            }

            var fileNameSet = SetFileName(saveDialog, projectName);
            if (!fileNameSet)
            {
                TestContext.Progress.WriteLine("❌ SaveAsDialog: Không thể nhập tên file");
                return false;
            }

            var saveClicked = ClickSave(saveDialog);
            if (!saveClicked)
            {
                TestContext.Progress.WriteLine("❌ SaveAsDialog: Không thể click Save");
                return false;
            }

            TestContext.Progress.WriteLine("✅ SaveAsDialog: Project đã được lưu thành công");
            return true;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ SaveAsDialog: Lỗi khi xử lý Save As dialog: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Chờ Save As dialog xuất hiện
    /// </summary>
    private Window? WaitForSaveAsDialog()
    {
        TestContext.Progress.WriteLine("🔍 SaveAsDialog: Bắt đầu tìm Save As dialog trong Revit...");
        
        return UiWaits.Until(() =>
        {
            try
            {
                // Tìm Save As dialog CHỈ trong Revit window
                var dialogsInRevit = _revitWindow.FindAllChildren(cf => 
                    cf.ByControlType(ControlType.Window));
                
                TestContext.Progress.WriteLine($"🔍 SaveAsDialog: Tìm thấy {dialogsInRevit.Length} dialogs trong Revit:");
                
                foreach (var dialog in dialogsInRevit)
                {
                    if (dialog.IsAvailable)
                    {
                        TestContext.Progress.WriteLine($"  - '{dialog.Name}' (Type: {dialog.ControlType})");
                        
                        // Kiểm tra nếu tên chứa "Save As"
                        if (dialog.Name.Contains("Save As") || dialog.Name.Contains("Save"))
                        {
                            TestContext.Progress.WriteLine($"✅ SaveAsDialog: Tìm thấy Save As dialog: '{dialog.Name}'");
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"⚠️ SaveAsDialog: Lỗi khi tìm Save As dialog: {ex.Message}");
                return false;
            }
        }, TimeSpan.FromSeconds(15), TestConfig.PollInterval) ? 
        GetSaveAsDialog() : null;
    }

    /// <summary>
    /// Lấy Save As dialog window
    /// </summary>
    private Window? GetSaveAsDialog()
    {
        try
        {
            // Tìm Save As dialog trong Revit window
            var dialogsInRevit = _revitWindow.FindAllChildren(cf => 
                cf.ByControlType(ControlType.Window));
            
            foreach (var dialog in dialogsInRevit)
            {
                if (dialog.IsAvailable && (dialog.Name.Contains("Save As") || dialog.Name.Contains("Save")))
                {
                    TestContext.Progress.WriteLine($"✅ SaveAsDialog: Trả về Save As dialog: '{dialog.Name}'");
                    return dialog.AsWindow();
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ SaveAsDialog: Lỗi khi lấy Save As dialog: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Tạo folder mới
    /// </summary>
    private bool CreateNewFolder(Window saveDialog)
    {
        try
        {
            TestContext.Progress.WriteLine("📁 SaveAsDialog: Tạo folder mới trong thư mục hiện tại...");
            
            // Tìm button "Create New Folder"
            var createFolderButton = saveDialog.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Button).And(cf.ByName("Create New Folder (Alt+5)")));
            
            if (createFolderButton != null && createFolderButton.IsAvailable)
            {
                createFolderButton.Click();
                System.Threading.Thread.Sleep(1000);
                
                // Nhập tên folder mới
                System.Windows.Forms.SendKeys.SendWait("RevitTestData");
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                System.Threading.Thread.Sleep(1000);
                
                TestContext.Progress.WriteLine("✅ SaveAsDialog: Đã tạo folder 'RevitTestData'");
                return true;
            }
            
            TestContext.Progress.WriteLine("❌ SaveAsDialog: Không tìm thấy button 'Create New Folder'");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ SaveAsDialog: Lỗi khi tạo folder: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Set tên file
    /// </summary>
    private bool SetFileName(Window saveDialog, string fileName)
    {
        try
        {
            TestContext.Progress.WriteLine("📝 SaveAsDialog: Tìm textbox để nhập tên file...");
            
            // Tìm textbox "File name" - có thể có nhiều cách
            var fileNameBox = saveDialog.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Edit).And(cf.ByName("File name:"))) ??
                saveDialog.FindFirstDescendant(cf => 
                    cf.ByControlType(ControlType.Edit).And(cf.ByName("File name"))) ??
                saveDialog.FindFirstDescendant(cf => 
                    cf.ByControlType(ControlType.Edit));
            
            if (fileNameBox != null && fileNameBox.IsAvailable)
            {
                TestContext.Progress.WriteLine($"✅ SaveAsDialog: Tìm thấy textbox: '{fileNameBox.Name}'");
                
                fileNameBox.Focus();
                System.Threading.Thread.Sleep(500);
                
                // Xóa text hiện tại và nhập tên mới
                System.Windows.Forms.SendKeys.SendWait("^a"); // Select all
                System.Threading.Thread.Sleep(200);
                System.Windows.Forms.SendKeys.SendWait(fileName);
                System.Threading.Thread.Sleep(500);
                
                TestContext.Progress.WriteLine($"✅ SaveAsDialog: Đã nhập tên file: {fileName}");
                return true;
            }
            
            TestContext.Progress.WriteLine("❌ SaveAsDialog: Không tìm thấy textbox để nhập tên file");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ SaveAsDialog: Lỗi khi set tên file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Click Save button
    /// </summary>
    private bool ClickSave(Window saveDialog)
    {
        try
        {
            TestContext.Progress.WriteLine("💾 SaveAsDialog: Tìm button Save...");
            
            // Tìm button Save - có thể có nhiều cách
            var saveButton = saveDialog.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Button).And(cf.ByName("Save"))) ??
                saveDialog.FindFirstDescendant(cf => 
                    cf.ByControlType(ControlType.Button).And(cf.ByName("&Save"))) ??
                saveDialog.FindFirstDescendant(cf => 
                    cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("1")));
            
            if (saveButton != null && saveButton.IsAvailable)
            {
                TestContext.Progress.WriteLine($"✅ SaveAsDialog: Tìm thấy button Save: '{saveButton.Name}'");
                
                saveButton.Click();
                System.Threading.Thread.Sleep(1000); // Chờ xử lý
                
                TestContext.Progress.WriteLine("✅ SaveAsDialog: Đã click Save button");
                return true;
            }
            
            TestContext.Progress.WriteLine("❌ SaveAsDialog: Không tìm thấy button Save");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ SaveAsDialog: Lỗi khi click Save: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Kích hoạt Save As dialog
    /// </summary>
    private bool TriggerSaveAs()
    {
        try
        {
            TestContext.Progress.WriteLine("💾 SaveAsDialog: Sử dụng Ctrl+S để mở Save As...");
            
            System.Windows.Forms.SendKeys.SendWait("^s");
            System.Threading.Thread.Sleep(1000);
            
            TestContext.Progress.WriteLine("✅ SaveAsDialog: Đã gửi Ctrl+S");
            return true;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ SaveAsDialog: Lỗi khi kích hoạt Save As: {ex.Message}");
            return false;
        }
    }
}
