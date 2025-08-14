using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace TestUnsignedAddin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Simple test command - chỉ hiển thị message box
            TaskDialog.Show("Test Add-in", "Đây là test add-in để trigger security dialog!");
            return Result.Succeeded;
        }
    }

    public class TestApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Tạo ribbon button đơn giản
            try
            {
                // Tạo ribbon panel
                RibbonPanel panel = application.CreateRibbonPanel("Test Security");
                
                // Tạo button
                PushButtonData buttonData = new PushButtonData(
                    "TestSecurityButton",
                    "Test Security",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    "TestUnsignedAddin.TestCommand"
                );
                
                panel.AddItem(buttonData);
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "Lỗi khi khởi tạo add-in: " + ex.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
