using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MyFirstRevitPlugin
{
    internal class FirstApp:IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location,
                   myTabName = "Плагины для бруса";

            application.CreateRibbonTab(myTabName);
            RibbonPanel panel = application.CreateRibbonPanel(myTabName, "Стена из бруса");
            PushButtonData BtnData = new PushButtonData(nameof(NewComand), "Забивка брусом", assemblyLocation, typeof(NewComand).ToString());
            panel.AddItem(BtnData);
            return Result.Succeeded;
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
