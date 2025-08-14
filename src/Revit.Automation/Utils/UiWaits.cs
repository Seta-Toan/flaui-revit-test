using System;
using System.Threading;

namespace Revit.Automation.Core.Utils;

public static class UiWaits
{
    public static bool Until(Func<bool> cond, TimeSpan? timeout = null, TimeSpan? poll = null)
    {
        var end = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(20));
        var delay = poll ?? TimeSpan.FromMilliseconds(200);
        Exception? last = null;

        while (DateTime.UtcNow < end)
        {
            try
            {
                last = null; // reset mỗi lần cond() chạy ok
                if (cond()) return true;
            }
            catch (Exception ex)
            {
                last = ex; // ghi nhận rồi tiếp tục polling
            }
            Thread.Sleep(delay);
        }

        if (last != null)
            throw new TimeoutException("Wait timed out", last);

        return false;
    }

    public static T? UntilNotNull<T>(Func<T?> cond, TimeSpan? timeout = null, TimeSpan? poll = null) where T : class
    {
        T? result = null;
        var ok = Until(() => (result = cond()) != null, timeout, poll);
        return ok ? result : null;
    }
}
