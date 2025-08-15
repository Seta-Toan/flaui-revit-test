# 🏗️ **FlaUI Revit Testing Project**

## 📋 **Mục Lục**
1. [Tổng Quan](#-tổng-quan)
2. [Kiến Trúc Project](#-kiến-trúc-project)
3. [Cài Đặt & Thiết Lập](#-cài-đặt--thiết-lập)
4. [Hướng Dẫn Tạo Project](#-hướng-dẫn-tạo-project)
5. [Refactoring & Cải Tiến](#-refactoring--cải-tiến)
6. [Cách Tạo Solution Tổng](#-cách-tạo-solution-tổng)
7. [Chạy Tests](#-chạy-tests)
8. [Troubleshooting](#-troubleshooting)
9. [Best Practices](#-best-practices)

---

## 🎯 **Tổng Quan**

Project này sử dụng **FlaUI framework** để tự động hóa testing cho **Autodesk Revit**. FlaUI là một wrapper cho Windows UI Automation API, cho phép tương tác với các ứng dụng Windows một cách tự động.

### **Tính Năng Chính:**
- ✅ **Khởi động/Attach Revit** tự động
- ✅ **Xử lý startup dialogs** (Trial, Security warnings)
- ✅ **Navigation UI** (Home page, Project selection)
- ✅ **E2E testing** cho workflow Revit
- ✅ **Page Object Model** architecture
- ✅ **Global setup** cho tất cả tests

---

## 🏛️ **Kiến Trúc Project**

```
FlaUITestWithClassLibraryNETStandard/
├── src/
│   ├── Revit.Automation/           # Core automation logic
│   │   ├── Core/
│   │   │   ├── Config/            # Configuration management
│   │   │   ├── Drivers/           # Revit process management
│   │   │   └── Utils/             # Utility classes
│   │   └── Revit.Automation.csproj
│   │
│   ├── Revit.UiPages/             # UI page objects
│   │   ├── Dialogs/               # Dialog handling
│   │   ├── Pages/                 # Page objects
│   │   └── Revit.UiPages.csproj
│   │
│   └── Revit.TestData/            # Test data & resources
│
├── tests/
│   └── Revit.UiTests/             # Test projects
│       ├── Setup/                  # Global setup & configuration
│       ├── CompleteWorkflowTest.cs # Main E2E tests
│       └── Revit.UiTests.csproj
│
├── RevitTests.sln                  # Main solution file
└── README.md                       # This file
```

### **Các Project Chính:**

#### **1. Revit.Automation (Core)**
- **Config/TestConfig.cs**: Quản lý timeout, đường dẫn
- **Drivers/RevitProcess.cs**: Khởi động/attach Revit process
- **Utils/UiWaits.cs**: Utility methods cho waiting
- **Utils/ElementsExtensions.cs**: Extension methods cho FlaUI elements

#### **2. Revit.UiPages**
- **Dialogs/DialogManager.cs**: Quản lý tất cả dialogs
- **Dialogs/TrialDialog.cs**: Xử lý trial dialog
- **Dialogs/SecurityDialog.cs**: Xử lý security warnings
- **Pages/RevitHomePage.cs**: Page object cho home page

#### **3. Revit.UiTests**
- **Setup/GlobalSetup.cs**: Global setup cho tất cả tests
- **CompleteWorkflowTest.cs**: E2E test scenarios

---

## 🚀 **Cài Đặt & Thiết Lập**

### **Yêu Cầu Hệ Thống:**
- Windows 10/11
- .NET Framework 4.8
- .NET 8.0 SDK
- Autodesk Revit 2026
- Visual Studio 2022 hoặc VS Code

### **Cài Đặt Dependencies:**

#### **1. NuGet Packages:**
```xml
<!-- Revit.Automation.csproj -->
<PackageReference Include="FlaUI.Core" Version="4.0.0" />
<PackageReference Include="FlaUI.UIA3" Version="4.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="8.0.0" />

<!-- Revit.UiTests.csproj -->
<PackageReference Include="NUnit" Version="4.0.0" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
```

#### **2. Environment Variables:**
```bash
# Set Revit executable path
REVIT_EXE="D:\Revit\Autodesk\Revit 2026\Revit.exe"

# Set test configuration
CI_MODE=false
```

#### **3. Configuration File (appsettings.Test.json):**
```json
{
  "Revit": {
    "ExePath": "D:\\Revit\\Autodesk\\Revit 2026\\Revit.exe",
    "StartTimeoutSec": 60
  },
  "DefaultTimeoutSeconds": 30,
  "PollIntervalMilliseconds": 200,
  "DialogTimeoutSeconds": 8
}
```

---

## 🛠️ **Hướng Dẫn Tạo Project**

### **Bước 1: Tạo Solution Structure**

```bash
# Tạo thư mục gốc
mkdir FlaUITestWithClassLibraryNETStandard
cd FlaUITestWithClassLibraryNETStandard

# Tạo solution file
dotnet new sln -n RevitTests

# Tạo các project
dotnet new classlib -n Revit.Automation -o src/Revit.Automation
dotnet new classlib -n Revit.UiPages -o src/Revit.UiPages
dotnet new classlib -n Revit.TestData -o src/Revit.TestData
dotnet new xunit -n Revit.UiTests -o tests/Revit.UiTests

# Thêm projects vào solution
dotnet sln add src/Revit.Automation/Revit.Automation.csproj
dotnet sln add src/Revit.UiPages/Revit.UiPages.csproj
dotnet sln add src/Revit.TestData/Revit.TestData.csproj
dotnet sln add tests/Revit.UiTests/Revit.UiTests.csproj
```

### **Bước 2: Cấu Hình Project References**

```bash
# Revit.UiPages references Revit.Automation
dotnet add src/Revit.UiPages/Revit.UiPages.csproj reference src/Revit.Automation/Revit.Automation.csproj

# Revit.UiTests references cả hai
dotnet add tests/Revit.UiTests/Revit.UiTests.csproj reference src/Revit.Automation/Revit.Automation.csproj
dotnet add tests/Revit.UiTests/Revit.UiTests.csproj reference src/Revit.UiPages/Revit.UiPages.csproj
```

### **Bước 3: Cấu Hình Target Frameworks**

#### **Revit.Automation.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net48</TargetFrameworks>
    <UseWindowsForms>true</UseWindowsForms>
    <EnableWindowsTargeting>true</EnableWindowsTargeting>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

#### **Revit.UiPages.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net48</TargetFrameworks>
    <UseWindowsForms>true</UseWindowsForms>
    <EnableWindowsTargeting>true</EnableWindowsTargeting>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\Revit.Automation\Revit.Automation.csproj" />
  </ItemGroup>
</Project>
```

#### **Revit.UiTests.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net48</TargetFrameworks>
    <UseWindowsForms>true</UseWindowsForms>
    <EnableWindowsTargeting>true</EnableWindowsTargeting>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\..\src\Revit.Automation\Revit.Automation.csproj" />
    <ProjectReference Include="..\..\src\Revit.UiPages\Revit.UiPages.csproj" />
  </ItemGroup>
</Project>
```

---

## 🔄 **Refactoring & Cải Tiến**

### **Quá Trình Refactoring:**

#### **1. Kiến Trúc Ban Đầu (Monolithic):**
```
FlaUIRevitTest/
├── ClassLibrary1.csproj          # Tất cả logic trong 1 project
├── Setup/
├── Tests/
└── Utilities/
```

#### **2. Kiến Trúc Mới (Layered):**
```
FlaUITestWithClassLibraryNETStandard/
├── src/Revit.Automation/         # Core automation
├── src/Revit.UiPages/            # UI interactions
├── src/Revit.TestData/           # Test data
└── tests/Revit.UiTests/          # Test execution
```

### **Lợi Ích Của Refactoring:**

- ✅ **Separation of Concerns**: Mỗi project có trách nhiệm riêng
- ✅ **Reusability**: Core logic có thể dùng cho nhiều test projects
- ✅ **Maintainability**: Dễ bảo trì và mở rộng
- ✅ **Testability**: Có thể test từng layer riêng biệt
- ✅ **Scalability**: Dễ thêm features mới

### **Các Bước Refactoring:**

#### **Bước 1: Tách Core Logic**
```bash
# Tạo Revit.Automation project
dotnet new classlib -n Revit.Automation -o src/Revit.Automation

# Di chuyển các file core
mv Setup/RevitTestSetup.cs src/Revit.Automation/
mv Utilities/AutomationHelper.cs src/Revit.Automation/
```

#### **Bước 2: Tách UI Logic**
```bash
# Tạo Revit.UiPages project
dotnet new classlib -n Revit.UiPages -o src/Revit.UiPages

# Di chuyển dialog handling
mv Setup/DialogHandlers.cs src/Revit.UiPages/Dialogs/
```

#### **Bước 3: Cập Nhật Namespaces**
```csharp
// Trước
namespace FlaUIRevitTest;

// Sau
namespace Revit.Automation.Core.Drivers;
namespace Revit.UiPages.Dialogs;
namespace Revit.UiTests.Setup;
```

#### **Bước 4: Giải Quyết Circular Dependencies**
```csharp
// Vấn đề: Revit.UiPages reference Revit.Automation
// Giải pháp: Sử dụng dependency injection hoặc events
public class DialogManager
{
    private readonly Application _app;
    private readonly UIA3Automation _uia;
    
    public DialogManager(Application app, UIA3Automation uia)
    {
        _app = app;
        _uia = uia;
    }
}
```

---

## 🎯 **Cách Tạo Solution Tổng**

### **Phương Pháp 1: Sử Dụng dotnet CLI**

```bash
# Tạo solution mới
dotnet new sln -n RevitTests

# Tạo các projects
dotnet new classlib -n Revit.Automation -o src/Revit.Automation
dotnet new classlib -n Revit.UiPages -o src/Revit.UiPages
dotnet new classlib -n Revit.TestData -o src/Revit.TestData
dotnet new xunit -n Revit.UiTests -o tests/Revit.UiTests

# Thêm vào solution
dotnet sln add src/Revit.Automation/Revit.Automation.csproj
dotnet sln add src/Revit.UiPages/Revit.UiPages.csproj
dotnet sln add src/Revit.TestData/Revit.TestData.csproj
dotnet sln add tests/Revit.UiTests/Revit.UiTests.csproj

# Build solution
dotnet build
```

### **Phương Pháp 2: Sử Dụng Visual Studio**

1. **File → New → Project**
2. **Chọn "Blank Solution"**
3. **Đặt tên "RevitTests"**
4. **Right-click solution → Add → New Project**
5. **Tạo từng project theo thứ tự:**
   - Revit.Automation (Class Library)
   - Revit.UiPages (Class Library)
   - Revit.TestData (Class Library)
   - Revit.UiTests (xUnit Test Project)

### **Phương Pháp 3: Sử Dụng VS Code**

1. **Tạo thư mục gốc**
2. **Mở terminal trong VS Code**
3. **Chạy các lệnh dotnet CLI**
4. **Sử dụng C# extension để quản lý**

### **Cấu Trúc Solution File (.sln):**

```ini
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1

Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Revit.Automation", "src\Revit.Automation\Revit.Automation.csproj", "{GUID1}"
EndProject

Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Revit.UiPages", "src\Revit.UiPages\Revit.UiPages.csproj", "{GUID2}"
EndProject

Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Revit.TestData", "src\Revit.TestData\Revit.TestData.csproj", "{GUID3}"
EndProject

Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Revit.UiTests", "tests\Revit.UiTests\Revit.UiTests.csproj", "{GUID4}"
EndProject

Global
    GlobalSection(SolutionConfigurationPlatforms) = preSolution
        Debug|Any CPU = Debug|Any CPU
        Release|Any CPU = Release|Any CPU
    EndGlobalSection
    
    GlobalSection(ProjectConfigurationPlatforms) = postSolution
        {GUID1}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        {GUID1}.Debug|Any CPU.Build.0 = Debug|Any CPU
        # ... các project khác
    EndGlobalSection
EndGlobal
```

---

## 🧪 **Chạy Tests**

### **Chạy Tất Cả Tests:**
```bash
dotnet test
```

### **Chạy Test Cụ Thể:**
```bash
# Chạy test theo tên
dotnet test --filter "Name=E2E_Revit_Startup_To_Project_Selection" --logger "console;verbosity=detailed"

# Chạy test theo category
dotnet test --filter "Category=CompleteWorkflow" --logger "console;verbosity=detailed"

# Chạy test với log chi tiết
dotnet test --logger "console;verbosity=detailed"
```

### **Chạy Test Với Configuration:**
```bash
# Chạy với configuration cụ thể
dotnet test --configuration Release

# Chạy với framework cụ thể
dotnet test --framework net48

# Chạy với output cụ thể
dotnet test --results-directory TestResults
```

### **Chạy Test Trong Visual Studio:**
1. **Mở Test Explorer** (Test → Test Explorer)
2. **Build solution**
3. **Chọn test cần chạy**
4. **Click "Run" hoặc "Debug"**

---

## 🔧 **Troubleshooting**

### **Lỗi Thường Gặp:**

#### **1. Circular Dependency:**
```
Error: Project 'Revit.UiPages' has a circular dependency on 'Revit.Automation'
```
**Giải pháp:**
- Kiểm tra project references
- Sử dụng dependency injection
- Tách interface và implementation

#### **2. Build Errors:**
```
Error: The type or namespace name 'X' could not be found
```
**Giải pháp:**
- Kiểm tra using statements
- Kiểm tra project references
- Clean và rebuild solution

#### **3. Test Failures:**
```
Error: Test exceeded Timeout value
```
**Giải pháp:**
- Tăng timeout trong TestConfig
- Kiểm tra Revit process
- Xử lý dialogs trước khi test

#### **4. FlaUI Errors:**
```
Error: Element not found
```
**Giải pháp:**
- Kiểm tra element identification
- Sử dụng UiWaits.Until
- Thêm retry logic

### **Debug Tips:**

#### **1. Enable Detailed Logging:**
```csharp
TestContext.Progress.WriteLine("🔍 Debug: Element details...");
```

#### **2. Use UiWaits:**
```csharp
var element = UiWaits.Until(() => 
    mainWindow.FindFirstDescendant(cf => cf.ByName("Button")),
    TimeSpan.FromSeconds(10)
);
```

#### **3. Check Element Properties:**
```csharp
var button = mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
if (button != null)
{
    TestContext.Progress.WriteLine($"Name: {button.Name}");
    TestContext.Progress.WriteLine($"AutomationId: {button.AutomationId}");
    TestContext.Progress.WriteLine($"ClassName: {button.ClassName}");
}
```

---

## 📚 **Best Practices**

### **1. Architecture:**
- ✅ **Separation of Concerns**: Mỗi project có trách nhiệm riêng
- ✅ **Dependency Injection**: Tránh circular dependencies
- ✅ **Interface Segregation**: Sử dụng interfaces cho loose coupling
- ✅ **Single Responsibility**: Mỗi class chỉ làm một việc

### **2. UI Automation:**
- ✅ **Page Object Model**: Tách UI logic khỏi test logic
- ✅ **Explicit Waits**: Sử dụng UiWaits thay vì Thread.Sleep
- ✅ **Element Identification**: Sử dụng AutomationId khi có thể
- ✅ **Error Handling**: Xử lý lỗi gracefully

### **3. Testing:**
- ✅ **Global Setup**: Sử dụng [SetUpFixture] cho setup chung
- ✅ **Test Isolation**: Mỗi test độc lập với nhau
- ✅ **Timeout Management**: Sử dụng [Timeout] attribute
- ✅ **Retry Logic**: Sử dụng [Retry] cho flaky tests

### **4. Configuration:**
- ✅ **Environment Variables**: Sử dụng cho sensitive data
- ✅ **Configuration Files**: Sử dụng appsettings.json
- ✅ **Default Values**: Cung cấp fallback values
- ✅ **Validation**: Kiểm tra configuration khi startup

### **5. Logging:**
- ✅ **Structured Logging**: Sử dụng TestContext.Progress.WriteLine
- ✅ **Log Levels**: Phân biệt debug, info, warning, error
- ✅ **Context Information**: Log đầy đủ context
- ✅ **Performance Metrics**: Log thời gian thực thi

---

## 🚀 **Mở Rộng Project**

### **Thêm Features Mới:**

#### **1. Thêm Dialog Type:**
```csharp
public class NewDialog : IDialog
{
    public DialogResult HandleIfPresent()
    {
        // Implementation
    }
}
```

#### **2. Thêm Page Object:**
```csharp
public class NewPage : IPage
{
    public bool IsLoaded()
    {
        // Implementation
    }
}
```

#### **3. Thêm Test Scenario:**
```csharp
[Test]
[Timeout(60000)]
public void New_Test_Scenario()
{
    // Test implementation
}
```

### **Performance Optimization:**

#### **1. Parallel Execution:**
```csharp
[assembly: Parallelizable(ParallelScope.Fixtures)]
```

#### **2. Test Categories:**
```csharp
[Test]
[Category("Fast")]
[Category("UI")]
public void Fast_UI_Test()
{
    // Implementation
}
```

#### **3. Conditional Execution:**
```csharp
[Test]
[Category("Slow")]
[Ignore("Only run in CI")]
public void Slow_Test()
{
    // Implementation
}
```

---

## 📞 **Hỗ Trợ & Liên Hệ**

### **Documentation:**
- [FlaUI Documentation](https://github.com/FlaUI/FlaUI)
- [NUnit Documentation](https://docs.nunit.org/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)

### **Issues & Bugs:**
- Tạo issue trên GitHub repository
- Mô tả chi tiết vấn đề
- Cung cấp log và stack trace

### **Contributing:**
1. Fork repository
2. Tạo feature branch
3. Commit changes
4. Push to branch
5. Tạo Pull Request

---

## 📝 **Changelog**

### **Version 1.0.0 (Current)**
- ✅ Initial project setup
- ✅ FlaUI integration
- ✅ Dialog handling (Trial, Security)
- ✅ Page Object Model
- ✅ Global setup configuration
- ✅ E2E test scenarios

### **Planned Features**
- 🔄 Parallel test execution
- 🔄 CI/CD integration
- 🔄 Performance monitoring
- 🔄 Advanced reporting
- 🔄 Cross-browser support

---

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 **Acknowledgments**

- **FlaUI Team**: For the excellent UI automation framework
- **NUnit Team**: For the robust testing framework
- **Microsoft**: For .NET platform and tools
- **Autodesk**: For Revit API and documentation

---

**🎉 Happy Testing! 🎉**
