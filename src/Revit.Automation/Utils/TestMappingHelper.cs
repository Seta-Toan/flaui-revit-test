using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Revit.Automation.Utils
{
    public static class TestMappingHelper
    {
        private static IConfiguration _configuration;

        static TestMappingHelper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        public static string GetTestExecutionKey(string testSuiteName)
        {
            return _configuration[$"TestMappings:{testSuiteName}:TestExecutionKey"]!;
        }

        public static string GetTestKey(string testSuiteName, string testMethodName)
        {
            return _configuration[$"TestMappings:{testSuiteName}:Tests:{testMethodName}"]!;
        }

        public static Dictionary<string, string> GetAllTestMappings(string testSuiteName)
        {
            var mappings = new Dictionary<string, string>();
            var section = _configuration.GetSection($"TestMappings:{testSuiteName}:Tests");
            
            foreach (var child in section.GetChildren())
            {
                mappings[child.Key] = child.Value!;
            }
            
            return mappings;
        }
    }
}
