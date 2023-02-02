using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DavidsRevitApp.ReNumGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace DavidsRevitApp.AutoFlag
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class AutoFlagCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            /*Пробуем создать экземпляр окна программы АвтоФлажка*/
            try
            {
                /*Создаем экземпляр окна*/AutoFlagMainView MView = 
                    new AutoFlagMainView(commandData, null, null);
                /*Открываем окно в диалоговом режиме*/MView.ShowDialog();

                return Result.Succeeded;
            }
            catch(Exception ex)
            {
                TaskDialog.Show("error", ex.Message);
                return Result.Failed;
            }
        }

    }
}
