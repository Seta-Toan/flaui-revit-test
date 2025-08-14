using NUnit.Framework;
using System;

namespace Revit.UiTests.Setup;

/// <summary>
/// Base class cho tất cả test classes để quản lý lifecycle và sử dụng GlobalSetup
/// </summary>
public abstract class TestBase
{
    protected int TestId { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
        // Đăng ký test với GlobalSetup
        TestId = GlobalSetup.RegisterTest();
        TestContext.Progress.WriteLine($"🚀 Test {TestId} bắt đầu: {TestContext.CurrentContext.Test.Name}");
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Hủy đăng ký test
        GlobalSetup.UnregisterTest(TestId);
        TestContext.Progress.WriteLine($"✅ Test {TestId} hoàn thành: {TestContext.CurrentContext.Test.Name}");
    }

    /// <summary>
    /// Lấy Revit instance từ GlobalSetup
    /// </summary>
    protected Revit.Automation.Core.Drivers.RevitProcess GetRevit()
    {
        return GlobalSetup.Revit;
    }

    /// <summary>
    /// Kiểm tra xem Revit đã sẵn sàng chưa
    /// </summary>
    protected void EnsureRevitReady()
    {
        var revit = GetRevit();
        Assert.That(revit.App, Is.Not.Null, "Revit application không được khởi tạo");
        Assert.That(revit.Uia, Is.Not.Null, "UIA automation không được khởi tạo");
        Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
    }
}
