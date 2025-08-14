using System;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Revit.Automation.Core.Config;
using Revit.Automation.Core.Utils;
using NUnit.Framework;

namespace Revit.UiPages.Dialogs;

public class TrialDialog
{
    private readonly Application _app;
    private readonly UIA3Automation _uia;

    public TrialDialog(Application app, UIA3Automation uia)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _uia = uia ?? throw new ArgumentNullException(nameof(uia));
    }

    public DialogResult CloseIfPresent()
    {
        try
        {
            TestContext.Progress.WriteLine("🔍 TrialDialog: Tìm trial dialog...");
            var startTime = DateTime.Now;
            
            var trialDialog = FindTrialDialog();
            
            if (trialDialog == null)
            {
                TestContext.Progress.WriteLine("✅ TrialDialog: Không tìm thấy trial dialog");
                return DialogResult.NotFound;
            }

            TestContext.Progress.WriteLine($"✅ TrialDialog: Tìm thấy trial dialog: '{trialDialog.Name}'");
            
            var result = HandleTrialDialog(trialDialog);
            
            if (result == DialogResult.Closed)
            {
                TestContext.Progress.WriteLine("⏳ TrialDialog: Kiểm tra dialog có đóng...");
                var closed = UiWaits.Until(() => !trialDialog.IsAvailable, TimeSpan.FromSeconds(3), TestConfig.PollInterval);
                if (closed)
                {
                    TestContext.Progress.WriteLine("✅ TrialDialog: Trial dialog đã đóng thành công");
                }
                else
                {
                    TestContext.Progress.WriteLine("⚠️ TrialDialog: Dialog vẫn còn nhưng có thể đang đóng, tiếp tục...");
                }
            }

            var totalTime = DateTime.Now - startTime;
            TestContext.Progress.WriteLine($"⏱️ TrialDialog: Thời gian xử lý trial dialog: {totalTime.TotalSeconds:F1}s");
            
            return result;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ TrialDialog: Lỗi khi xử lý trial dialog: {ex.Message}");
            return DialogResult.Failed;
        }
    }

    private AutomationElement? FindTrialDialog()
    {
        var desktop = _uia.GetDesktop();
        
        // Method 1: Tìm theo AutomationId (nhanh nhất và chính xác nhất)
        try
        {
            Console.WriteLine("🔍 Tìm theo AutomationId 'QApplication.WebView2BrowserDlg'...");
            var dialog = desktop.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Window).And(cf.ByAutomationId("QApplication.WebView2BrowserDlg")));
            if (dialog != null) 
            {
                Console.WriteLine("✅ Tìm thấy trial dialog theo AutomationId");
                return dialog;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Lỗi khi tìm theo AutomationId: {ex.Message}");
        }
        
        // Method 2: Tìm theo tên chính xác
        try
        {
            Console.WriteLine("🔍 Tìm theo tên 'WebView2 WebBrowser'...");
            var dialog = desktop.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Window).And(cf.ByName("WebView2 WebBrowser")));
            if (dialog != null) 
            {
                Console.WriteLine("✅ Tìm thấy trial dialog theo tên");
                return dialog;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Lỗi khi tìm theo tên: {ex.Message}");
        }

        return null;
    }

    private DialogResult HandleTrialDialog(AutomationElement dialog)
    {
        try
        {
            // CHỜ WEBVIEW2 RENDER - WebView2 cần thời gian load HTML/CSS
            TestContext.Progress.WriteLine("⏳ Chờ 5 giây để WebView2 trial dialog render hoàn toàn...");
            System.Threading.Thread.Sleep(5000); 
            TestContext.Progress.WriteLine("✅ Hoàn tất chờ WebView2 render");
            
            // Thử tìm button với retry logic
            Button? closeButton = null;
            int attempts = 0;
            int maxAttempts = 3;            
            while (closeButton == null && attempts < maxAttempts)
            {
                attempts++;
                TestContext.Progress.WriteLine($"🔄 Lần thử {attempts}/{maxAttempts} tìm close button...");
                
                closeButton = FindCloseButtonInDialog(dialog);
                
                if (closeButton == null && attempts < maxAttempts)
                {
                    TestContext.Progress.WriteLine("⏳ Chờ thêm 5 giây để WebView2 load...");
                    System.Threading.Thread.Sleep(5000);
                }
            }
            
            if (closeButton != null)
            {
                TestContext.Progress.WriteLine($"🖱️ === THỰC HIỆN CLICK BUTTON ===");
                TestContext.Progress.WriteLine($"🖱️ Click button: '{closeButton.Name}' (Class: '{closeButton.ClassName}')");
                closeButton.Invoke();
                TestContext.Progress.WriteLine($"✅ Click hoàn tất!");
                return DialogResult.Closed;
            }
            return DialogResult.Closed;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ Lỗi khi xử lý trial dialog: {ex.Message}");
            return DialogResult.Failed;
        }
    }

    private Button? FindCloseButtonInDialog(AutomationElement dialog)
    {
        // Tìm ĐÚNG trial dialog close button (Hyperlink với class 'btn-close no-outline')
        try
        {
            TestContext.Progress.WriteLine("🔍 Tìm TRIAL DIALOG close button (Hyperlink class 'btn-close no-outline')...");
            var closeButton = dialog.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Hyperlink).And(cf.ByClassName("btn-close no-outline")))?.AsButton();
            if (closeButton != null) 
            {
                TestContext.Progress.WriteLine("✅ Tìm thấy TRIAL DIALOG close button (element [19])");
                return closeButton;
            }
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ Lỗi khi tìm trial dialog close button: {ex.Message}");
        }
        return null;
    }
}

