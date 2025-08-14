using System;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Revit.Automation.Config;
using Revit.Automation.Waits;
using NUnit.Framework;

namespace Revit.Automation.Dialogs;

public class DialogManager
{
    private readonly TrialDialog _trialDialog;
    private readonly SecurityDialog _securityDialog;

    public DialogManager(Application app, UIA3Automation uia)
    {
        _trialDialog = new TrialDialog(app, uia);
        _securityDialog = new SecurityDialog(app, uia);
    }

    // Method chính để xử lý startup dialogs
    public DialogResult HandleTrialDialogs()
    {
        try
        {
            TestContext.Progress.WriteLine("🚀 DialogManager: Bắt đầu xử lý startup dialogs...");
            var startTime = DateTime.Now;
            
            // Xử lý Trial Dialog 
            TestContext.Progress.WriteLine("🔍 DialogManager: Bước 1 - Xử lý Trial Dialog...");
            var trialResult = _trialDialog.CloseIfPresent();
            if (trialResult == DialogResult.Failed)
            {
                TestContext.Progress.WriteLine("❌ DialogManager: Không thể xử lý trial dialog");
                return DialogResult.Failed;
            }
            
            if (trialResult == DialogResult.Closed)
            {
                TestContext.Progress.WriteLine("⏳ DialogManager: Chờ trial dialog đóng...");
                System.Threading.Thread.Sleep(500); 
            }
            
            // Xử lý Security Dialog 
            TestContext.Progress.WriteLine("🔍 DialogManager: Bước 2 - Xử lý Security Dialog...");
            var securityResult = _securityDialog.HandleIfPresent();
            if (securityResult == DialogResult.Failed)
            {
                TestContext.Progress.WriteLine("❌ DialogManager: Không thể xử lý security dialog");
                return DialogResult.Failed;
            }
            
            var totalTime = DateTime.Now - startTime;
            TestContext.Progress.WriteLine($"✅ DialogManager: Hoàn thành xử lý startup dialogs trong {totalTime.TotalSeconds:F1}s");
            
            return DialogResult.Success;
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ DialogManager: Lỗi khi xử lý startup dialogs: {ex.Message}");
            return DialogResult.Failed;
        }
    }

    public bool HasActiveDialogs()
    {
        try
        {
            var desktop = _trialDialog.GetType().GetField("_uia", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_trialDialog) as UIA3Automation;
            if (desktop == null) return false;
            
            var allWindows = desktop.GetDesktop().FindAllDescendants(cf => cf.ByControlType(ControlType.Window));
            return allWindows.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public DialogResult HandleRuntimeDialogs()
    {
        return HandleTrialDialogs();
    }
}
