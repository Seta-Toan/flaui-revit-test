using FlaUI.Core.AutomationElements;

namespace Revit.Automation.Extensions;

public static class ElementExtensions
{
    public static void InvokeOrClick(this AutomationElement el)
    {
        (el.AsButton() as IInvokeProvider)? .Invoke();
        // fallback:
        if (el.Patterns?.Invoke?.Pattern == null) el.Click();
    }

    public static bool Exists(this AutomationElement? el) => el != null && el.IsAvailable;
}
