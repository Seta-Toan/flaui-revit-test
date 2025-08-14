using NUnit.Framework;
using System;
using System.Threading;
using System.Collections.Concurrent;
using Revit.Automation.Drivers;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Revit.Automation.Config;
using Revit.Automation.Waits;
using System.Linq;
using Revit.Automation.Dialogs;

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
                        _revit = new RevitProcess();
                        _revit.StartOrAttach();
                        TestContext.Progress.WriteLine("✅ Revit started/attached. Main window ready.");
                    }
                }
            }
            return _revit;
        }
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        try
        {
            Console.WriteLine("🚀 GlobalSetup: Bắt đầu khởi động Revit...");
            
            lock (_lock)
            {
                if (_revit == null)
                {
                    _revit = new RevitProcess();
                    _revit.StartOrAttach();
                    
                    // Xử lý startup dialogs ngay sau khi khởi động
                    Console.WriteLine(" GlobalSetup: Xử lý startup dialogs...");
                    var dialogManager = new DialogManager(_revit.App!, _revit.Uia!);
                    var dialogResult = dialogManager.HandleTrialDialogs();
                    
                    if (dialogResult == DialogResult.Success)
                    {
                        Console.WriteLine("✅ GlobalSetup: Startup dialogs đã được xử lý thành công");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ GlobalSetup: Startup dialogs không được xử lý hoàn toàn: {dialogResult}");
                    }
                    
                    Console.WriteLine("✅ GlobalSetup: Revit đã sẵn sàng");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GlobalSetup: Lỗi khi khởi động Revit: {ex.Message}");
            throw;
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
}
