using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DavidsRevitApp.ReNumGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Grid = Autodesk.Revit.DB.Grid;

namespace DavidsRevitApp.RoomFinishing
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomFinishingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            var uiSelect = commandData.Application.ActiveUIDocument.Selection;
            ISelectionFilter roomTagPickFilter = new RoomTagFilterClass();
            try
            {
                List<Reference> reference = uiSelect.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, roomTagPickFilter , "Выберите оси").ToList();
                
                List<RoomTag> SelectedRooms =
                    reference.Select(x => doc.GetElement(x))
                    .Where(y => y.GetType() == typeof(RoomTag))
                    .Cast<RoomTag>().ToList();
                
                var roomNums = SelectedRooms.Select(x => x.Room.Number).ToList(); 
                TaskDialog.Show("hello", string.Join(", ", roomNums));
                
                var bounds = SelectedRooms
                    .Select(x => x.Room.GetBoundarySegments(new SpatialElementBoundaryOptions())
                    .Select(y => y.Select(k => doc.GetElement(k.ElementId)).ToList()).ToList()).ToList();
                
                string paramName = "Комментарии";
                using(Transaction transaction = new Transaction(doc))
                {
                    transaction.Start("Start");
                    foreach(var el in roomNums)
                    {
                        List<Element> elementsOfRoom = bounds[roomNums.IndexOf(el)].First().ToList();
                        var Params = elementsOfRoom.Where(f => f != null)
                            .Select(e => e.GetOrderedParameters().ToList()).ToList();
                        List<Parameter> CurParams = Params.Where(p => p.Any(o => o.Definition.Name == paramName))
                            .Select(p => p.Where(k => k.Definition.Name == paramName).First()).ToList();
                        CurParams.ForEach(o => o.Set(el));
                    }
                    transaction.Commit();
                }
            }
            catch(Exception ex) { TaskDialog.Show("error", ex.Message); }

            return Result.Succeeded;
        }

        public class RoomTagFilterClass : ISelectionFilter
        {
            BuiltInCategory selectCategory = BuiltInCategory.OST_RoomTags;
            public bool AllowElement(Element elem) => elem.Category != null ?
                elem.Category.Id == new ElementId(selectCategory) : false;

            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
