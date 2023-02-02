using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace DavidsRevitApp.WallSweeps
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WallSweepsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            /*Документ проекта*/Document doc = commandData.Application.ActiveUIDocument.Document;

            /*Берем все экземпляры выступающих профилей WallSweep из проекта*/List<Element> allProfiles = 
                new FilteredElementCollector(doc)
                                        .OfCategory(BuiltInCategory.OST_ProfileFamilies)
                                        .WhereElementIsElementType()
                                        .Cast<Element>().ToList();
            /*Берем все экземпляры стен Wall из проекта*/List<Element> allWalls = 
                new FilteredElementCollector(doc)
                                        .OfCategory(BuiltInCategory.OST_Walls)
                                        .WhereElementIsElementType().ToList();
            /*Берем все Материалы из проекта*/List<Element> allMaterials = 
                new FilteredElementCollector(doc)
                                        .OfCategory(BuiltInCategory.OST_Materials)
                                        .Cast<Element>().ToList();

            /*Создаем экземпляр окна*/WallSweepsMainView MView = 
                new WallSweepsMainView(allWalls, allProfiles, allMaterials, commandData);
            /*Открываем окно в диалоговом режиме*/MView.ShowDialog();
            
            return Result.Succeeded;
        }
    }
}
