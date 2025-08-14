# FlaUI Revit Test - Phiên bản cải tiến

Dự án kiểm thử tự động cho Autodesk Revit sử dụng FlaUI framework với kiến trúc phân tầng và xử lý dialog thông minh.

## 🚀 Cải tiến chính so với phiên bản cũ

### 1. **Kiến trúc phân tầng**
- **`src/Revit.Automation`**: Core automation logic, drivers, và utilities
- **`src/Revit.UiPages`**: Page Object Model cho các dialog và UI elements
- **`tests/Revit.UiTests`**: Test cases được tổ chức theo chức năng

### 2. **Xử lý dialog thông minh**
- **`DialogManager`**: Quản lý tất cả dialog trong quá trình khởi động
- **`TrialDialog`**: Xử lý trial dialog với multiple fallback strategies
- **`SecurityDialog`**: Xử lý security dialog tự động
- Tích hợp xử lý dialog vào quá trình khởi động Revit

### 3. **Cải thiện timing và synchronization**
- Thay thế `Thread.Sleep` bằng `UiWaits.Until` thông minh
- Timeout configurable thông qua `appsettings.Test.json`
- Retry mechanism cho UIA connection

### 4. **Logging và debugging tốt hơn**
- Console output chi tiết với emoji và timing
- Error handling robust với fallback strategies
- Progress tracking cho từng bước xử lý

## 🏗️ Cấu trúc dự án

```
FlaUITestWithClassLibraryNETStandard/
├── src/
│   ├── Revit.Automation/           # Core automation
│   │   ├── Config/                 # Configuration management
│   │   ├── Drivers/                # Revit process management
│   │   └── Waits/                  # Smart waiting utilities
│   └── Revit.UiPages/              # UI page objects
│       └── Dialogs/                # Dialog handlers
│           ├── DialogManager.cs     # Main dialog orchestrator
│           ├── TrialDialog.cs       # Trial dialog handler
│           └── SecurityDialog.cs    # Security dialog handler
├── tests/
│   └── Revit.UiTests/              # Test cases
│       ├── 00_Setup/               # Global setup
│       ├── 01_Startup/             # Startup tests
│       └── 02_Dialogs/             # Dialog handling tests
└── appsettings.Test.json           # Test configuration
```

## 🔧 Cài đặt và chạy

### 1. **Cấu hình Revit path**
```json
{
  "Revit": {
    "ExePath": "%REVIT_EXE%",    // Hoặc đường dẫn trực tiếp
    "StartTimeoutSec": 180,       // Timeout khởi động
    "UiPollMs": 200,              // Polling interval
    "DefaultTimeoutSec": 45        // Default timeout
  }
}
```

### 2. **Build và chạy tests**
```bash
# Build solution
dotnet build

# Chạy tất cả tests
dotnet test

# Chạy test cụ thể
dotnet test --filter "Category=Dialogs"
```

## 📋 Test Cases

### **Startup Tests**
- `Revit_MainWindow_ShouldBeVisible_AndEnabled`: Kiểm tra Revit khởi động thành công
- `Revit_Should_Remain_Stable_For_A_Short_Grace_Period`: Kiểm tra tính ổn định

### **Dialog Tests**
- `Startup_Dialogs_Should_Be_Handled_Successfully`: Kiểm tra xử lý startup dialogs
- `Trial_Dialog_Should_Be_Handled_If_Present`: Kiểm tra xử lý trial dialog
- `Security_Dialog_Should_Be_Handled_If_Present`: Kiểm tra xử lý security dialog
- `Dialog_Manager_Should_Handle_Runtime_Dialogs`: Kiểm tra xử lý runtime dialogs

## 🔍 Cách hoạt động

### **1. Quá trình khởi động**
```
🚀 Khởi động Revit (Attach hoặc Launch)
🔄 Xử lý startup dialogs:
  ├── BƯỚC 1: Xử lý trial dialog
  └── BƯỚC 2: Xử lý security dialog
⏳ Chờ Revit ổn định
✅ Revit sẵn sàng
```

### **2. Xử lý dialog**
- **Trial Dialog**: Tìm theo AutomationId, tên, hoặc fallback
- **Security Dialog**: Tìm theo AutomationId hoặc tên, click Yes/Allow
- **Fallback**: Sử dụng ESC nếu không tìm thấy nút

### **3. Error handling**
- Retry mechanism cho UIA connection
- Multiple fallback strategies cho dialog handling
- Detailed logging cho debugging

## 🚨 Lưu ý quan trọng

### **Environment Variables**
```bash
# Đặt đường dẫn Revit
set REVIT_EXE="D:\Revit\Autodesk\Revit 2026\Revit.exe"
```

### **Dependencies**
- .NET Framework 4.8
- FlaUI.Core 5.0.0
- FlaUI.UIA3 5.0.0
- NUnit 3.14.0

### **Performance**
- Startup time: ~30-60 giây (tùy máy)
- Dialog handling: ~5-15 giây
- Total setup time: ~35-75 giây

## 🔄 So sánh với phiên bản cũ

| Tính năng | Phiên bản cũ | Phiên bản mới |
|-----------|--------------|---------------|
| **Cấu trúc** | Monolithic | Phân tầng |
| **Dialog handling** | Manual, Thread.Sleep | Automatic, Smart waits |
| **Error handling** | Basic | Robust với fallback |
| **Logging** | Minimal | Detailed với timing |
| **Configuration** | Hardcoded | JSON configurable |
| **Maintainability** | Low | High |
| **Performance** | Slow (Thread.Sleep) | Fast (Smart waits) |

## 🚀 Roadmap

### **Phase 1** ✅ (Hoàn thành)
- [x] Kiến trúc phân tầng
- [x] Dialog handling tự động
- [x] Smart waiting thay thế Thread.Sleep

### **Phase 2** 🔄 (Đang phát triển)
- [ ] Project creation automation
- [ ] Template selection
- [ ] Advanced UI workflows

### **Phase 3** 📋 (Kế hoạch)
- [ ] CI/CD integration
- [ ] Parallel test execution
- [ ] Performance benchmarking

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch
3. Commit changes với message rõ ràng
4. Push và tạo Pull Request

## 📞 Hỗ trợ

Nếu gặp vấn đề:
1. Kiểm tra logs trong console output
2. Verify Revit path configuration
3. Kiểm tra UIA permissions
4. Tạo issue với log chi tiết

---

**Phiên bản**: 2.0.0  
**Trạng thái**: Stable  
**Cập nhật cuối**: 2024
