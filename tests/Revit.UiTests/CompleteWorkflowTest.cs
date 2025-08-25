using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Revit.UiTests.Setup;
using Revit.UiPages.Pages;
using Revit.Automation.Services;
using Revit.Automation.Utils;

namespace Revit.UiTests;

[TestFixture]
[Category("CompleteWorkflow")]
public class CompleteWorkflowTest
{
    private JiraXrayService? _xrayService;
    private string? _testExecutionKey;
    private string? _cachedToken;
    private const string TEST_SUITE_NAME = "CompleteWorkflow";

    [OneTimeSetUp]
    public async Task Setup()
    {
        var clientId = ConfigurationHelper.GetJiraClientId();
        var clientSecret = ConfigurationHelper.GetJiraClientSecret();
        _testExecutionKey = TestMappingHelper.GetTestExecutionKey(TEST_SUITE_NAME);
        
        _xrayService = new JiraXrayService(clientId, clientSecret);
        
        // Xác thực 1 lần duy nhất khi bắt đầu test suite
        _cachedToken = await _xrayService.GetTokenAsync();
        TestContext.Progress.WriteLine("🔐 Đã xác thực Xray và cache token cho toàn bộ test suite");
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        _xrayService?.Dispose();
    }

    [Test, Order(1)]
    [Timeout(120000)]
    [Retry(1)]
    public async Task E2E_Revit_Startup_To_Project_Selection()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Khởi động Revit và tới trang chọn project...");

        try
        {
            var revit = GlobalSetup.Revit;
            Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
            var mainWindow = revit.MainWindow;
            Assert.That(mainWindow, Is.Not.Null, "Không tìm thấy main window");

            var homePage = new RevitHomePage(mainWindow, revit.Uia!);
            
            var onProjectSelection = homePage.IsLoaded();
            Assert.That(onProjectSelection, Is.True, "Home page chưa load xong");

            var duration = DateTime.Now - startTime;
            TestContext.Progress.WriteLine($"✅ E2E hoàn tất trong {duration.TotalSeconds:F1}s");

            // Sử dụng token đã cache
            var testKey = TestMappingHelper.GetTestKey(TEST_SUITE_NAME, nameof(E2E_Revit_Startup_To_Project_Selection));
            await PushTestResultToJira(testKey, "PASSED");
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ Test thất bại: {ex.Message}");
            var testKey = TestMappingHelper.GetTestKey(TEST_SUITE_NAME, nameof(E2E_Revit_Startup_To_Project_Selection));
            await PushTestResultToJira(testKey, "FAILED");
            throw;
        }
    }

    [Test, Order(2)]
    [Timeout(70000)] 
    [Retry(1)]
    public async Task E2E_Click_New_Project()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Click New Project...");

        try
        {
            var revit = GlobalSetup.Revit;
            Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
            var mainWindow = revit.MainWindow;
            Assert.That(mainWindow, Is.Not.Null, "Không tìm thấy main window");

            var homePage = new RevitHomePage(mainWindow, revit.Uia!);
            Assert.That(homePage.IsLoaded(), Is.True, "Home page chưa load xong");
            
            var newProjectClicked = homePage.ClickNewProject();
            Assert.That(newProjectClicked, Is.True, "Không thể click New Project button");

            TestContext.Progress.WriteLine($"✅ E2E New Project hoàn tất trong {(DateTime.Now - startTime).TotalSeconds:F1}s");
                
            var testKey = TestMappingHelper.GetTestKey(TEST_SUITE_NAME, nameof(E2E_Click_New_Project));
            await PushTestResultToJira(testKey, "PASSED");
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ Test thất bại: {ex.Message}");
            var testKey = TestMappingHelper.GetTestKey(TEST_SUITE_NAME, nameof(E2E_Click_New_Project));
            await PushTestResultToJira(testKey, "FAILED");
            throw;
        }
    }

    [Test, Order(3)]
    [Timeout(120000)]
    [Retry(1)]
    public async Task E2E_Create_New_Project_And_Save_To_TestData()
    {
        var startTime = DateTime.Now;
        TestContext.Progress.WriteLine("🚀 Bắt đầu E2E: Tạo project mới và lưu vào test data...");

        try
        {
            var revit = GlobalSetup.Revit;
            Assert.That(revit.IsMainWindowReady(), Is.True, "Revit main window chưa sẵn sàng");
            var mainWindow = revit.MainWindow;
            Assert.That(mainWindow, Is.Not.Null, "Không tìm thấy main window");

            var homePage = new RevitHomePage(mainWindow, revit.Uia!);
            Assert.That(homePage.IsLoaded(), Is.True, "Home page chưa load xong");
            
            var projectCreated = homePage.CreateNewProject("TestProject_Automated");
            Assert.That(projectCreated, Is.True, "Không thể tạo project mới");

            TestContext.Progress.WriteLine("✅ E2E Create New Project hoàn tất - Save As dialog đã được hiển thị");
            TestContext.Progress.WriteLine($"✅ E2E Create New Project hoàn tất trong {(DateTime.Now - startTime).TotalSeconds:F1}s");
            
            var testKey = TestMappingHelper.GetTestKey(TEST_SUITE_NAME, nameof(E2E_Create_New_Project_And_Save_To_TestData));
            await PushTestResultToJira(testKey, "PASSED");
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ Test thất bại: {ex.Message}");
            var testKey = TestMappingHelper.GetTestKey(TEST_SUITE_NAME, nameof(E2E_Create_New_Project_And_Save_To_TestData));
            await PushTestResultToJira(testKey, "FAILED");
            throw;
        }
    }

    private async Task PushTestResultToJira(string testKey, string status)
    {
        try
        {
            TestContext.Progress.WriteLine($" Đẩy kết quả test {testKey} với status {status} lên Jira...");
            
            // Sử dụng token đã cache, không cần gọi GetTokenAsync() nữa
            bool success = await _xrayService!.PushResultAsync(_testExecutionKey!, testKey, status, _cachedToken!);
            
            if (success)
            {
                TestContext.Progress.WriteLine($"✅ Đã đẩy kết quả test {testKey} lên Jira thành công");
            }
            else
            {
                TestContext.Progress.WriteLine($"⚠️ Không thể đẩy kết quả test {testKey} lên Jira");
            }
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"❌ Lỗi khi đẩy kết quả test lên Jira: {ex.Message}");
        }
    }
}
