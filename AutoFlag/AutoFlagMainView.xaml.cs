using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace DavidsRevitApp.AutoFlag
{
    /// <summary>
    /// Логика взаимодействия для AutoFlagMainView.xaml
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public partial class AutoFlagMainView : Window
    {

        ExternalCommandData commandData;
        int ImageWidth = 10, ImageHeight = 10;
        UIApplication mainapp;
        UIDocument uiDoc;
        Document doc;
        Selection uiSelect;
        Family AutoFlagFamily;
        ElementId SelectedAutoFlagId;
        List<ElementId> ConstructionIds;
        double NormOfOneMeter = 3.28083989501312;
        string FlagContent = "";
        string autoFlagFileName = DavidsApp.DirOfAssembly + "AutoFlagFamily.rfa";
        string autoFlagFamilyName = "AutoFlagFamily";
        string[] transactionNames = new string[]
        {
            "Загрузка семейства автофлажка",
            "Создание экземпляра автофлажка"
        };

        public AutoFlagMainView(ExternalCommandData commandData, ElementId flagId, List<ElementId> ConstrIds)
        {
            SelectedAutoFlagId = flagId;
            ConstructionIds = ConstrIds == null ? new List<ElementId>() : ConstrIds;
            this.commandData = commandData;
            InitializeComponent();

            if (File.Exists(DavidsApp.IconPaths[2])) Icon = Imaging.CreateBitmapSourceFromHIcon(new Icon(DavidsApp.IconPaths[2]).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            mainapp = this.commandData.Application;
            uiDoc = mainapp.ActiveUIDocument;
            doc = uiDoc.Document;
            uiSelect = uiDoc.Selection;

            SelectAutoFlagImage.Source = SetColor(false, ImageWidth, ImageHeight);

            if (!(SelectedAutoFlagId == null)) AutoFlagIsSelected();
            if (ConstructionIds != null) if (ConstructionIds.Count > 0)
                    WatchConstructionIdsBtn.IsEnabled = true;
        }

        private void CreateFlagBtn_Click(object sender, RoutedEventArgs e)
        {
            FilteredElementCollector flc = new FilteredElementCollector(doc);
            /*Загрузка семейства в проект, если семейство не загружено*/
            {
                if (!flc.WhereElementIsNotElementType()
                    .OfClass(typeof(Family)).Cast<Family>()
                    .Select(o => o.Name).Contains(autoFlagFamilyName))
                    using (Transaction transaction = new Transaction(doc))
                    {
                        transaction.Start(transactionNames[0]);
                        if (File.Exists(autoFlagFileName))
                            doc.LoadFamily(autoFlagFileName, out AutoFlagFamily);
                        transaction.Commit();
                    }
                else
                    AutoFlagFamily = flc.WhereElementIsNotElementType()
                                    .OfClass(typeof(Family)).Cast<Family>()
                                    .First(o => o.Name == autoFlagFamilyName);
            }
            /*Установка флажка в зависимости от вида*/
            {
                if ((doc.ActiveView.ViewType != ViewType.ThreeD))
                {
                    FamilySymbol fs = doc.GetElement(AutoFlagFamily.GetFamilySymbolIds().First()) as FamilySymbol;
                    AutoFlagMainView MView;
                    this.Close();

                    XYZ PickedPoint;
                    try { PickedPoint = doc.ActiveView.ViewType != ViewType.DraftingView ?
                            uiSelect.PickObject(ObjectType.Element).GlobalPoint : uiSelect.PickPoint(); }
                    catch { PickedPoint = null; }

                    if (PickedPoint != null)
                        using (Transaction transaction = new Transaction(doc))
                        {
                            transaction.Start(transactionNames[1]);

                            if (!fs.IsActive) fs.Activate();
                            SelectedAutoFlagId = doc.Create.NewFamilyInstance(
                                PickedPoint, fs, doc.ActiveView).Id;

                            transaction.Commit();
                        }

                    MView = new AutoFlagMainView(commandData, SelectedAutoFlagId, ConstructionIds);
                    MView.ShowDialog();
                }
                else { TaskDialog.Show("AutoFlag", "Выберите 2D вид"); this.Close(); }
            }
        }

        private void SelectFlagBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            try
            {
                SelectedAutoFlagId = uiSelect.PickObject(ObjectType.Element).ElementId;
            } catch { SelectedAutoFlagId = null; }
            if (SelectedAutoFlagId != null) if ((doc.GetElement(SelectedAutoFlagId) as AnnotationSymbol).AnnotationSymbolType.FamilyName != autoFlagFamilyName)
                    SelectedAutoFlagId = null;
            AutoFlagMainView MView = new AutoFlagMainView(commandData, SelectedAutoFlagId, ConstructionIds);
            MView.ShowDialog();
        }

        private void ParseFlagBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (ConstructionIds == null) ConstructionIds = new List<ElementId>();
            ConstructionIds.Clear();

            if (ConstructionCheckBox.IsChecked.Value == true)
            {
                List<Reference> reference = null;
                try
                {
                    reference = uiSelect
                        .PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, "Выберите конструкции").ToList();
                }
                catch { reference = null; }

                if (reference != null)
                    ConstructionIds = reference.Select(p => p.ElementId).ToList();
                if (ConstructionIds.Count == 0)
                    TaskDialog.Show("error", "Не выбрана конструкция для флажка");
            }
            SetFlagContext();
            
            FlagContextView ParseView =
                new FlagContextView(FlagContent, commandData, SelectedAutoFlagId, ConstructionIds);
            ParseView.ShowDialog();
            AutoFlagMainView MView = new AutoFlagMainView(commandData, SelectedAutoFlagId, ConstructionIds);
            MView.ShowDialog();
        }

        private void WatchConstructionIdsBtn_Click(object sender, RoutedEventArgs e)
        {
            var NameAndIds = ConstructionIds.Select(o => doc.GetElement(o));
            string tempNameAndIds = "";
            foreach (var el in NameAndIds) tempNameAndIds+=el.Name +" - "+ el.Id+"\n";
            TaskDialog.Show("Id of Element", tempNameAndIds);
            this.Activate();
        }

        void AutoFlagIsSelected()
        {
            SelectAutoFlagImage.Source = SetColor(true, ImageWidth, ImageHeight);
            SelectAutoFlagLable.Content = "Автофлажок выбран!";
            ParseFlagBtn.IsEnabled = true;
            DirectionFlagBtn.IsEnabled = true;
        }

        private void DirectionFlagBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            ArrowDirectionView DirectionView = new ArrowDirectionView(SelectedAutoFlagId, doc);
            DirectionView.ShowDialog();
            AutoFlagMainView MView = new AutoFlagMainView(commandData, SelectedAutoFlagId, ConstructionIds);
            MView.ShowDialog();
        }

        void SetFlagContext()
        {
            if (ConstructionIds.Count > 0)
            {
                string NameOfLayer = "", WidthOfLayer = "";

                if (ConstructionIds.Select(o => doc.GetElement(o).GetType().Name).Any(p => p == nameof(FilledRegion)))
                    foreach (var el in ConstructionIds.Select(o => doc.GetElement(o)).OfType<FilledRegion>())
                    {
                        NameOfLayer = doc.GetElement(el.GetTypeId()).Name;
                        WidthOfLayer = "";
                        FlagContent += NameOfLayer + "\t-" + WidthOfLayer + "\n";
                    }

                else
                {
                    List<HostObjAttributes> Attributes = null;

                    if (ConstructionIds.Select(o => doc.GetElement(o).Category.Id).Any(p => p == new ElementId(BuiltInCategory.OST_LegendComponents)))
                    {
                        ConstructionIds = ConstructionIds.Select(o => doc.GetElement(o)).Where(p => p.Category.Id == new ElementId(BuiltInCategory.OST_LegendComponents)).Select(k => k.Id).ToList();
                        Attributes = ConstructionIds.Select(o => doc.GetElement(doc.GetElement(o).GetOrderedParameters().FirstOrDefault(p => p.Definition.Name == "Тип компонента").AsElementId())).OfType<HostObjAttributes>().ToList();
                    }
                    else
                        Attributes = ConstructionIds.Select(o => doc.GetElement(o)).Cast<Element>().Select(p => doc.GetElement(p.GetTypeId())).OfType<HostObjAttributes>().ToList();

                    
                    if (Attributes != null) foreach (var el in Attributes)
                        {
                            var CompStruct = el.GetCompoundStructure();
                            if (CompStruct != null)
                                foreach (var layerInFloor in CompStruct.GetLayers())
                                {
                                    NameOfLayer = layerInFloor.MaterialId.IntegerValue == -1 ? "?" : doc.GetElement(layerInFloor.MaterialId).Name;
                                    WidthOfLayer = Math.Round(1000 * layerInFloor.Width / NormOfOneMeter) > 5 ?
                                        Math.Round(1000 * layerInFloor.Width / NormOfOneMeter).ToString() : "";
                                    FlagContent += NameOfLayer + "\t-" + WidthOfLayer + "\n";
                                }
                            FlagContent += "\n";
                        }
                }
                WatchConstructionIdsBtn.IsEnabled = true;
            }
        }

        ImageSource SetColor(bool col, int width, int height)
        {
            Bitmap tempBitmap = new Bitmap(width,height);
            Color tempColor = col?Color.Green:Color.DarkRed;
            
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++) 
                    tempBitmap.SetPixel(i, j, tempColor);
            }
            return Imaging.CreateBitmapSourceFromHBitmap(tempBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

    }
}
