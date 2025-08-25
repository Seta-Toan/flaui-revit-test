using Microsoft.Extensions.Configuration;
using System.IO;
using NUnit.Framework;
using DotNetEnv;

namespace Revit.Automation.Utils
{
    public static class ConfigurationHelper
    {
        private static IConfiguration _configuration;

        static ConfigurationHelper()
        {
            // Tìm file .env từ thư mục gốc của solution
            var currentDir = Directory.GetCurrentDirectory();
            var solutionDir = FindSolutionDirectory(currentDir);
            var envPath = Path.Combine(solutionDir, ".env");
            
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                TestContext.Progress.WriteLine($"✅ Đã load file .env từ: {envPath}");
            }
            else
            {
                TestContext.Progress.WriteLine($"⚠️ Không tìm thấy file .env tại: {envPath}");
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false)
                .AddEnvironmentVariables()
                .AddEnvironmentVariables(prefix: "XRAY_");

            _configuration = builder.Build();
        }

        private static string FindSolutionDirectory(string startPath)
        {
            var current = startPath;
            while (!string.IsNullOrEmpty(current))
            {
                if (File.Exists(Path.Combine(current, "RevitTests.sln")))
                {
                    return current;
                }
                current = Path.GetDirectoryName(current);
            }
            return startPath;
        }

        public static string GetJiraClientId() => 
            _configuration["JiraXray:ClientId"]?.Replace("%XRAY_CLIENT_ID%", _configuration["XRAY_CLIENT_ID"]) 
            ?? _configuration["XRAY_CLIENT_ID"]!;

        public static string GetJiraClientSecret() => 
            _configuration["JiraXray:ClientSecret"]?.Replace("%XRAY_CLIENT_SECRET%", _configuration["XRAY_CLIENT_SECRET"]) 
            ?? _configuration["XRAY_CLIENT_SECRET"]!;

        public static string GetJiraTestExecutionKey() => _configuration["JiraXray:TestExecutionKey"]!;
        public static string GetJiraProjectKey() => _configuration["JiraXray:ProjectKey"]!;
    }
}
