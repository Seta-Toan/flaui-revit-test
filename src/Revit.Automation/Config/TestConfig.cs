using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Revit.Automation.Core.Config;

public static class TestConfig
{
    private static readonly IConfigurationRoot Cfg;
    static TestConfig()
    {
        Cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }
    public static TimeSpan StartTimeout =>
        TimeSpan.FromSeconds(int.TryParse(Cfg["Revit:StartTimeoutSec"], out var s) ? s : 60);

    public static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(30); 
    public static TimeSpan PollInterval => TimeSpan.FromMilliseconds(200); 
    public static TimeSpan DialogTimeout => TimeSpan.FromSeconds(8);
    public static string RevitPath => GetRevitPath();
    public static string RevitExe
    {
        get
        {
            try
            {
                // 1. Kiểm tra biến môi trường
                var envVar = Environment.GetEnvironmentVariable("REVIT_EXE");
                if (!string.IsNullOrWhiteSpace(envVar) && File.Exists(envVar))
                {
                    return envVar;
                }

                // 2. Kiểm tra cấu hình từ appsettings
                var configPath = Cfg["Revit:ExePath"];
                if (!string.IsNullOrWhiteSpace(configPath) && configPath != "%REVIT_EXE%" && File.Exists(configPath))
                {
                    return configPath!;
                }

                // 3. Tìm Revit 2026 trong các thư mục phổ biến
                var revitPath = FindRevit2026Path();
                if (!string.IsNullOrEmpty(revitPath))
                {
                    return revitPath;
                }

                // 4. Nếu không tìm thấy, throw exception với thông tin hướng dẫn
                throw new FileNotFoundException(
                    "Không thể tìm thấy Revit 2026. Vui lòng:\n" +
                    "1. Đặt biến môi trường REVIT_EXE\n" +
                    "2. Cấu hình Revit:ExePath trong appsettings.Test.json\n" +
                    "3. Cài đặt Revit 2026 vào thư mục mặc định\n" +
                    "4. Hoặc chỉ định đường dẫn chính xác"
                );
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException(
                    $"Lỗi khi tìm Revit 2026: {ex.Message}", ex);
            }
        }
    }
    
    private static string GetRevitPath()
    {
        return RevitExe;
    }

    public static bool CiMode =>
        bool.TryParse(Cfg["CiMode"], out var b) && b;
    public static class UiAliases
    {

        public static readonly string[] NewButtonNames = new[]
        {
            "New ...",            
            "Nouveau ...",        
            "Neu ...",            
            "Nuevo ...",          
            "Nuovo ...",          
            "Новый ...",          
            "新建 ...",            
            "新規 ...",            
            "새로 만들기 ...",      
            "ใหม่ ..."             
        };
    }
    private static string FindRevit2026Path()
    {
        try
        {
            // Các đường dẫn phổ biến cho Revit 2026
            var commonPaths = new List<string>();

            // 1. Từ thư mục hiện tại (nếu chạy từ thư mục cài đặt)
            var currentDir = Directory.GetCurrentDirectory();
            var currentRevitPath = Path.Combine(currentDir, "Revit.exe");
            if (File.Exists(currentRevitPath))
            {
                commonPaths.Add(currentRevitPath);
            }

            // 2. Từ thư mục cha (nếu chạy từ subfolder)
            var parentDir = Directory.GetParent(currentDir)?.FullName;
            if (!string.IsNullOrEmpty(parentDir))
            {
                var parentRevitPath = Path.Combine(parentDir, "Revit.exe");
                if (File.Exists(parentRevitPath))
                {
                    commonPaths.Add(parentRevitPath);
                }
            }

            // 3. Từ các ổ đĩa có sẵn
            var drives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Fixed)
                .Select(d => d.Name);

            foreach (var drive in drives)
            {
                var customPaths = new[]
                {
                    Path.Combine(drive, "Revit", "Autodesk", "Revit 2026", "Revit.exe"),
                    Path.Combine(drive, "Revit", "Autodesk", "Revit 2026", "Revit.exe"),
                    Path.Combine(drive, "Revit", "Revit 2026", "Revit.exe"),
                    Path.Combine(drive, "Revit", "Revit.exe"),
                    Path.Combine(drive, "Autodesk", "Revit 2026", "Revit.exe"),
                    Path.Combine(drive, "Autodesk", "Revit", "Revit.exe")
                };

                commonPaths.AddRange(customPaths);
            }

            // 4. Từ Program Files
            var programFilesPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            foreach (var programFiles in programFilesPaths)
            {
                if (string.IsNullOrEmpty(programFiles) || !Directory.Exists(programFiles))
                    continue;

                var autodeskPath = Path.Combine(programFiles, "Autodesk");
                if (Directory.Exists(autodeskPath))
                {
                    var revit2026Path = Path.Combine(autodeskPath, "Revit 2026", "Revit.exe");
                    if (File.Exists(revit2026Path))
                    {
                        commonPaths.Add(revit2026Path);
                    }

                    var revitPath = Path.Combine(autodeskPath, "Revit", "Revit.exe");
                    if (File.Exists(revitPath))
                    {
                        commonPaths.Add(revitPath);
                    }
                }
            }

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                {
                    if (IsRevit2026(path))
                    {
                        return path;
                    }
                }
            }

            return null!;
        }
        catch
        {
            return null!;
        }
    }

    private static bool IsRevit2026(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            var fileName = Path.GetFileName(filePath);
            if (fileName != "Revit.exe")
                return false;

            var directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath != null && directoryPath.Contains("2026"))
                return true;

            var parentDir = Directory.GetParent(directoryPath)?.Name;
            if (parentDir != null && parentDir.Contains("2026"))
                return true;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
