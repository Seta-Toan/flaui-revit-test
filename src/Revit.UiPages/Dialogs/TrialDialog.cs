using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Revit.Automation.Waits;
using Revit.Automation.Config;

namespace Revit.UiPages.Dialogs;

public class TrialDialog
{
    private readonly Application _app;
    private readonly UIA3Automation _uia;
    public TrialDialog(Application app, UIA3Automation uia) { _app = app; _uia = uia; }

    public DialogResult CloseIfPresent()
    {
        // Aliases tiêu đề dialog có thể khác nhau theo phiên bản/locale
        string[] titles = { "WebView2 WebBrowser", "Trial", "Evaluation", "QApplication.WebView2BrowserDlg" };

        var dlg = _app.GetAllTopLevelWindows(_uia).FirstOrDefault(w => titles.Contains(w.Title));
        if (dlg == null) return DialogResult.NotFound;

        try
        {
            var closeBtn = dlg.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
                                                          .And(cf.ByName("Close")))
                              ?.AsButton();
            if (closeBtn == null) return DialogResult.Failed;

            closeBtn.Invoke();
            var closed = UiWait.Until(() => !dlg.IsAvailable, TestConfig.DefaultTimeout, TestConfig.PollInterval);
            return closed ? DialogResult.Closed : DialogResult.Failed;
        }
        catch { return DialogResult.Failed; }
    }
}
