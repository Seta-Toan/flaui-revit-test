using System;
using System.Threading;
using FlaUI.Core.AutomationElements;

namespace Revit.Automation.Core.Utils;

public static class ElementExtensions
{
    public static void InvokeOrClick(this AutomationElement el)
    {
        try
        {
            el.Patterns?.Invoke?.Pattern?.Invoke();
        }
        catch
        {
            // fallback
            el.AsButton()?.Invoke();
        }

        // fallback cuối
        if (el.Patterns?.Invoke?.Pattern == null)
        {
            try { el.Click(); } catch { /* ignore */ }
        }
    }

    public static bool SafeInvoke(this AutomationElement el, TimeSpan settle)
    {
        try
        {
            el.InvokeOrClick();
            Thread.Sleep(settle); // hoặc chuyển sang Wait điều kiện cụ thể nếu bạn có
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool Exists(this AutomationElement? el) => el != null && el.IsAvailable;
}
