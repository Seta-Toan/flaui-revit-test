using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions; 
using FlaUI.UIA3;
using Revit.Automation.Config;
using Revit.Automation.Waits;
using Revit.Automation.Dialogs;
using NUnit.Framework;

namespace Revit.Automation.Drivers;

public sealed class RevitProcess : IDisposable
{
    public Application? App { get; private set; }
    public UIA3Automation? Uia { get; private set; }
    private bool _isDisposed = false;

    public void StartOrAttach()
    {
        try
        {
            TestContext.Progress.WriteLine("🚀 RevitProcess.StartOrAttach: Bắt đầu khởi động hoặc attach Revit...");
            
            // 1) Attach nếu đã chạy, ngược lại Launch
            var existing = Process.GetProcessesByName("Revit").FirstOrDefault();
            if (existing != null)
            {
                TestContext.Progress.WriteLine($"✅ RevitProcess: Attach vào Revit process hiện có (PID: {existing.Id})");
                App = Application.Attach(existing);
            }
            else
            {
                TestContext.Progress.WriteLine("🆕 RevitProcess: Khởi động Revit mới...");
                App = Application.Launch(TestConfig.RevitExe);
                
                TestContext.Progress.WriteLine("⏳ RevitProcess: Chờ 22 giây để Revit khởi động hoàn toàn...");
                System.Threading.Thread.Sleep(22000); 
                TestContext.Progress.WriteLine("✅ RevitProcess: Hoàn tất chờ khởi động");
            }

            // 2) Khởi tạo UIA
            Uia = new UIA3Automation();
            TestContext.Progress.WriteLine("✅ RevitProcess: UIA đã được khởi tạo");

            // 3) Tìm main window
            TestContext.Progress.WriteLine("🔍 RevitProcess: Tìm main window...");
            Window? mainWindow = null;
            
            // Lấy qua WindowFactory của FlaUI
            var windowFound = UiWaits.Until(() =>
            {
                try
                {
                    //  Lấy handle của process
                    var process = Process.GetProcessesByName("Revit").FirstOrDefault();
                    if (process != null && process.MainWindowHandle != IntPtr.Zero)
                    {
                        mainWindow = Uia!.FromHandle(process.MainWindowHandle).AsWindow();
                        if (mainWindow != null && mainWindow.IsAvailable)
                        {
                            TestContext.Progress.WriteLine($"✅ RevitProcess: Tìm thấy window qua process handle: '{mainWindow.Name}'");
                            return true;
                        }
                    }
                    
                    return false;
                }
                catch (Exception ex)
                {
                    TestContext.Progress.WriteLine($"⚠️ RevitProcess: Lỗi khi tìm main window: {ex.Message}");
                    return false;
                }
            }, TestConfig.StartTimeout, TestConfig.PollInterval);

            TestContext.Progress.WriteLine("✅ RevitProcess: Revit đã khởi động thành công và sẵn sàng");
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ RevitProcess: Lỗi khi khởi động/attach Revit: {ex.Message}");
            throw;
        }
    }

    private Process? FindExistingRevitProcess()
    {
        try
        {
            var processes = Process.GetProcessesByName("Revit");
            
            var validProcesses = processes.Where(p => 
                p.MainWindowHandle != IntPtr.Zero && 
                !string.IsNullOrEmpty(p.MainWindowTitle) &&
                p.ProcessName == "Revit" &&
                !p.HasExited).ToList();

            if (validProcesses.Count == 0)
            {
                Console.WriteLine("ℹ️ Không tìm thấy Revit process hợp lệ");
                return null;
            }

            if (validProcesses.Count == 1)
            {
                var process = validProcesses.First();
                Console.WriteLine($"✅ Tìm thấy 1 Revit process hợp lệ (PID: {process.Id})");
                return process;
            }

            // Nếu có nhiều process, chọn process có main window title rõ ràng nhất
            var bestProcess = validProcesses
                .OrderByDescending(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .ThenByDescending(p => p.MainWindowTitle.Length)
                .First();

            Console.WriteLine($"⚠️ Tìm thấy {validProcesses.Count} Revit processes, chọn PID: {bestProcess.Id}");
            return bestProcess;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Lỗi khi tìm Revit process: {ex.Message}");
            return null;
        }
    }


    private bool IsProcessAlreadyAttached(Process process)
    {
        try
        {
            // Kiểm tra xem process có thể được attach không
            return process.HasExited || process.MainWindowHandle == IntPtr.Zero;
        }
        catch
        {
            return true; // Nếu có lỗi, coi như đã attach
        }
    }

    public bool TryReconnectUia()
    {
        try
        {
            Uia?.Dispose();
        }
        catch { /* ignore */ }

        try
        {
            Uia = new UIA3Automation();
            return true;
        }
        catch
        {
            Uia = null;
            return false;
        }
    }

    public bool IsMainWindowReady()
    {
        if (App == null || Uia == null) return false;
        try
        {
            // Thử lấy main window từ App trước
            var mw = App.GetMainWindow(Uia);
            if (mw != null && mw.IsAvailable)
            {
                return true;
            }
            
            // Fallback: tìm từ desktop
            var desktop = Uia.GetDesktop();
            var revitWindow = desktop.FindFirstDescendant(cf => 
                cf.ByControlType(ControlType.Window).And(
                    cf.ByName("Revit")
                      .Or(cf.ByName("Autodesk Revit"))
                      .Or(cf.ByClassName("Afx:00400000:8"))
                ));
            
            return revitWindow != null && revitWindow.IsAvailable;
        }
        catch
        {
            return false;
        }
    }

    public Window MainWindow
    {
        get
        {
            if (App == null || Uia == null) throw new InvalidOperationException("Revit chưa được StartOrAttach.");
            return App.GetMainWindow(Uia);
        }
    }

    public bool WaitStableAfterReady(TimeSpan grace, TimeSpan poll)
    {
        var end = DateTime.UtcNow + grace;
        string? lastTitle = null;
        while (DateTime.UtcNow < end)
        {
            TryReconnectUia();
            if (!IsMainWindowReady()) return false;

            var t = MainWindow.Title ?? string.Empty;
            if (!string.IsNullOrEmpty(t) && t == lastTitle) return true; 
            lastTitle = t;

            Thread.Sleep(poll);
        }
        return true; 
    }

    public T WithUia<T>(Func<T> fn)
    {
        for (int i = 0; i < 2; i++)
        {
            try { return fn(); }
            catch
            {
                if (!TryReconnectUia()) throw;
            }
        }
        return fn(); 
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        try 
        { 
            Uia?.Dispose(); 
            
            // Chỉ đóng Revit nếu không có test nào khác đang chạy
            if (ShouldCloseRevit())
            {
                Console.WriteLine(" Đóng Revit process vì không còn test nào đang chạy");
                App?.Close();
            }
            else
            {
                Console.WriteLine("ℹ️ Giữ Revit process để tiết kiệm thời gian cho test tiếp theo");
            }
        } 
        catch (Exception ex) 
        { 
            Console.WriteLine($"⚠️ Lỗi khi dispose: {ex.Message}");
        }
        finally
        {
            _isDisposed = true;
        }
    }

    private bool ShouldCloseRevit()
    {
        try
        {
            // Kiểm tra xem có test nào khác đang chạy không
            // Sử dụng reflection để truy cập GlobalSetup
            var globalSetupType = Type.GetType("Revit.UiTests.Setup.GlobalSetup");
            if (globalSetupType != null)
            {
                var hasActiveTestsProperty = globalSetupType.GetProperty("HasActiveTests");
                if (hasActiveTestsProperty != null)
                {
                    var hasActiveTests = (bool)hasActiveTestsProperty.GetValue(null)!;
                    return !hasActiveTests;
                }
            }
            
            // Fallback: luôn đóng Revit nếu không thể kiểm tra
            return true;
        }
        catch
        {
            return true;
        }
    }
}
