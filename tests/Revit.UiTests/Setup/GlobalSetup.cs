using NUnit.Framework;
using System;
using System.Threading;
using System.Collections.Concurrent;
using Revit.Automation.Core.Drivers;
using System.Linq;
using Revit.UiPages.Dialogs;
using System.Diagnostics;

[assembly: Apartment(ApartmentState.STA)]

namespace Revit.UiTests.Setup;

[SetUpFixture]
public sealed class GlobalSetup
{
    private static RevitProcess? _revit;
    private static readonly object _lock = new object();
    private static readonly ConcurrentDictionary<int, bool> _activeTests = new ConcurrentDictionary<int, bool>();
    private static int _testCounter = 0;

    public static RevitProcess Revit
    {
        get
        {
            if (_revit == null)
            {
                lock (_lock)
                {
                    if (_revit == null)
                    {
                        TestContext.Progress.WriteLine("🚀 === GLOBALSETUP INITIALIZING REVIT ===");
                        _revit = new RevitProcess();
                        _revit.StartOrAttach();
                        
                        // XỬ LÝ STARTUP DIALOGS BẰNG DIALOGMANAGER NGAY TẠI ĐÂY
                        TestContext.Progress.WriteLine("🔄 GlobalSetup: Xử lý startup dialogs bằng DialogManager...");
                        try
                        {
                            var dialogManager = new DialogManager(_revit.App!, _revit.Uia!);
                            var dialogResult = dialogManager.HandleTrialDialogs();
                            
                            if (dialogResult == DialogResult.Success)
                            {
                                TestContext.Progress.WriteLine("✅ GlobalSetup: DialogManager xử lý startup dialogs thành công");
                            }
                            else
                            {
                                TestContext.Progress.WriteLine($"⚠️ GlobalSetup: DialogManager xử lý startup dialogs không hoàn toàn: {dialogResult}");
                            }
                        }
                        catch (Exception dialogEx)
                        {
                            TestContext.Progress.WriteLine($"⚠️ GlobalSetup: Lỗi khi xử lý dialogs: {dialogEx.Message}");
                            TestContext.Progress.WriteLine($"⚠️ GlobalSetup: Stack trace: {dialogEx.StackTrace}");
                        }
                        
                        TestContext.Progress.WriteLine("✅ Revit started/attached. Main window ready.");
                    }
                }
            }
            return _revit;
        }
    }



    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        try
        {
            if (_revit != null)
            {
                TestContext.Progress.WriteLine("🧹 Cleaning up Revit process...");
                _revit.Dispose();
                _revit = null;
            }
            
            KillAllRevitProcesses();
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ Error during cleanup: {ex.Message}");
        }
    }

    public static int RegisterTest()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        _activeTests.TryAdd(testId, true);
        return testId;
    }

    public static void UnregisterTest(int testId)
    {
        _activeTests.TryRemove(testId, out _);
    }

    public static bool HasActiveTests => !_activeTests.IsEmpty;

    /// <summary>
    /// Kill tất cả Revit processes
    /// </summary>
    public static void KillAllRevitProcesses()
    {
        try
        {
            var revitProcesses = Process.GetProcessesByName("Revit");
            foreach (var process in revitProcesses)
            {
                TestContext.Progress.WriteLine($"🔒 GlobalSetup: Kill Revit process PID {process.Id}");
                process.Kill();
            }
            TestContext.Progress.WriteLine("✅ GlobalSetup: Đã kill tất cả Revit processes");
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"⚠️ GlobalSetup: Lỗi khi kill Revit processes: {ex.Message}");
        }
    }
}
