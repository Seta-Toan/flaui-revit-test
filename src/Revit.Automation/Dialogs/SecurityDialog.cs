using System;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Revit.Automation.Config;
using Revit.Automation.Waits;
using NUnit.Framework;

namespace Revit.Automation.Dialogs;

public class SecurityDialog
{
    private readonly Application _app;
    private readonly UIA3Automation _uia;

    public SecurityDialog(Application app, UIA3Automation uia)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _uia = uia ?? throw new ArgumentNullException(nameof(uia));
    }

    public DialogResult HandleIfPresent()
    {
        try
        {
            TestContext.Progress.WriteLine("🔍 SecurityDialog: Tìm security dialog...");
            var startTime = DateTime.Now;
            
            var securityDialog = FindSecurityDialog();
            
            if (securityDialog == null)
            {
                TestContext.Progress.WriteLine("✅ SecurityDialog: Không tìm thấy security dialog");
                return DialogResult.NotFound;
            }

            TestContext.Progress.WriteLine($"✅ SecurityDialog: Tìm thấy security dialog: '{securityDialog.Name}'");
            
            var result = HandleSecurityDialog(securityDialog);
            
            if (result == DialogResult.Closed)
            {
                var closed = UiWaits.Until(() => !securityDialog.IsAvailable, TestConfig.DialogTimeout, TestConfig.PollInterval);
                if (!closed)
                {
                    TestContext.Progress.WriteLine("⚠️ SecurityDialog: Security dialog không đóng sau khi xử lý, nhưng tiếp tục...");
                }
                else
                {
                    TestContext.Progress.WriteLine("✅ SecurityDialog: Security dialog đã đóng thành công");
                }
            }

            var totalTime = DateTime.Now - startTime;
            TestContext.Progress.WriteLine($"⏱️ SecurityDialog: Thời gian xử lý security dialog: {totalTime.TotalSeconds:F1}s");
            
            return result;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ Lỗi khi xử lý security dialog: {ex.Message}");
            return DialogResult.Failed;
        }
    }

    private AutomationElement? FindSecurityDialog()
    {
        var desktop = _uia.GetDesktop();
        
        // Tìm theo tên chính xác
        try
        {
            TestContext.Progress.WriteLine("🔍 Tìm theo tên 'Security - Unsigned Add-In'...");
            var dialog = desktop.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Window).And(cf.ByName("Security - Unsigned Add-In")));
            if (dialog != null) 
            {
                TestContext.Progress.WriteLine("✅ Tìm thấy security dialog theo tên");
                return dialog;
            }
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ Lỗi khi tìm theo tên: {ex.Message}");
        }

        TestContext.Progress.WriteLine("❌ Không tìm thấy security dialog");
        return null;
    }

    private DialogResult HandleSecurityDialog(AutomationElement dialog)
    {
        try
        {
            var doNotLoadButton = FindDoNotLoadButton(dialog);
            
            if (doNotLoadButton != null)
            {
                TestContext.Progress.WriteLine($"🖱️ === THỰC HIỆN CLICK DO NOT LOAD BUTTON ===");
                TestContext.Progress.WriteLine($"🖱️ Click button: '{doNotLoadButton.Name}' (AutoId: '{doNotLoadButton.AutomationId}')");
                doNotLoadButton.Invoke();
                TestContext.Progress.WriteLine($"✅ Click DO NOT LOAD hoàn tất - AN TOÀN!");
                return DialogResult.Closed;
            }
            return DialogResult.Closed;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ Lỗi khi xử lý security dialog: {ex.Message}");
            return DialogResult.Failed;
        }
    }

    private Button? FindDoNotLoadButton(AutomationElement dialog)
    {
        //Scan tất cả buttons và tìm "Do Not Load" 
        try
        {
            TestContext.Progress.WriteLine("🔍 Scan tất cả buttons để tìm 'Do Not Load'...");
            var allButtons = dialog.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
            foreach (var btn in allButtons)
            {
                try
                {
                    var name = btn.Name ?? "";
                    var autoId = btn.AutomationId ?? "";
                    TestContext.Progress.WriteLine($"  Button: Name='{name}', AutoId='{autoId}'");
                    
                    if (name == "Do Not Load" || name.Contains("Do Not") || name.Contains("Don't Load") || name.Contains("Reject"))
                    {
                        TestContext.Progress.WriteLine($"✅ Tìm thấy DO NOT LOAD button!");
                        return btn.AsButton();
                    }
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ Lỗi khi scan buttons: {ex.Message}");
        }

        TestContext.Progress.WriteLine("❌ Không tìm thấy 'Do Not Load' button");
        return null;
    }
}
