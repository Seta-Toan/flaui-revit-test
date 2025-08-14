using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Revit.Automation.Config;

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

    public static string RevitExe
    {
        get
        {
            var envVar = Environment.GetEnvironmentVariable("REVIT_EXE");
            var configPath = Cfg["Revit:ExePath"];
            var defaultPath = @"D:\Revit\Autodesk\Revit 2026\Revit.exe";

            var chosen = !string.IsNullOrWhiteSpace(envVar) ? envVar
                       : (!string.IsNullOrWhiteSpace(configPath) && configPath != "%REVIT_EXE%") ? configPath
                       : defaultPath;

            if (!File.Exists(chosen))
            {
                throw new FileNotFoundException(
                    $"Revit.exe không tồn tại tại đường dẫn: '{chosen}'. " +
                    "Vui lòng đặt biến môi trường REVIT_EXE hoặc cấu hình Revit:ExePath trong appsettings.Test.json.");
            }

            return chosen;
        }
    }

    public static TimeSpan StartTimeout =>
        TimeSpan.FromSeconds(int.TryParse(Cfg["Revit:StartTimeoutSec"], out var s) ? s : 60);

    public static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(30); 
    public static TimeSpan PollInterval => TimeSpan.FromMilliseconds(200); 
    public static TimeSpan DialogTimeout => TimeSpan.FromSeconds(15);
    public static string RevitPath => GetRevitPath();
    public static string RevitArguments => GetRevitArguments();
    
    private static string GetRevitPath()
    {
        return @"D:\Revit\Autodesk\Revit 2026\Revit.exe";
    }
    
    private static string GetRevitArguments()
    {
        return "/language ENU /nosplash";
    }

    public static bool CiMode =>
        bool.TryParse(Cfg["CiMode"], out var b) && b;
}
