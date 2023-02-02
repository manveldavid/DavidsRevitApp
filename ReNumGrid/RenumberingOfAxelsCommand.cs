using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DavidsRevitApp.ReNumGrid
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class RenumberingOfAxelsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            /*Пробуем создать экземпляр окна программы Перенумерации осей и взять все оси из документа*/
            try
            { 
                /*Берем экземпляры осей из проекта и сортируем их по имени*/
                List<Grid> GridsInDoc = 
                    new FilteredElementCollector(commandData.Application.ActiveUIDocument.Document)
                        .OfCategory(BuiltInCategory.OST_Grids)
                        .WhereElementIsNotElementType()
                        .Cast<Grid>().OrderBy(x => x.Name).ToList();

                /*Создаем экземпляр окна*/ReNumGridMainView MWin = 
                    new ReNumGridMainView(commandData, GridsInDoc, new List<Grid>(), true);
                /*Открываем окно в диалоговом режиме*/MWin.ShowDialog();
                
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
