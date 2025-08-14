using Microsoft.Extensions.Configuration;

namespace Revit.Automation.Config;

public static class TestConfig
{
    private static IConfigurationRoot? _cfg;
    static TestConfig()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables();
        _cfg = builder.Build();
    }

    public static string RevitExe =>
        Environment.GetEnvironmentVariable("REVIT_EXE")
        ?? _cfg?["Revit:ExePath"]?.Replace("%REVIT_EXE%", string.Empty) 
        ?? @"C:\Program Files\Autodesk\Revit 2026\Revit.exe";

    public static TimeSpan StartTimeout =>
        TimeSpan.FromSeconds(int.TryParse(_cfg?["Revit:StartTimeoutSec"], out var s) ? s : 60);

    public static TimeSpan DefaultTimeout =>
        TimeSpan.FromSeconds(int.TryParse(_cfg?["Revit:DefaultTimeoutSec"], out var s) ? s : 20);

    public static TimeSpan PollInterval =>
        TimeSpan.FromMilliseconds(int.TryParse(_cfg?["Revit:UiPollMs"], out var ms) ? ms : 200);
}
