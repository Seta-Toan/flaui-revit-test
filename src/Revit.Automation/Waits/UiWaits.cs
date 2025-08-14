namespace Revit.Automation.Waits;

public static class UiWait
{
    public static bool Until(Func<bool> cond, TimeSpan? timeout = null, TimeSpan? poll = null)
    {
        var end = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(20));
        var delay = poll ?? TimeSpan.FromMilliseconds(200);
        Exception? last = null;

        while (DateTime.UtcNow < end)
        {
            try { if (cond()) return true; last = null; }
            catch (Exception ex) { last = ex; }
            Thread.Sleep(delay);
        }
        if (last != null) throw last;
        return false;
    }
}
