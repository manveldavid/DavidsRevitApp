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
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using TextBox = System.Windows.Controls.TextBox;

namespace DavidsRevitApp.ReNumGrid
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public partial class ReNumGridMainView : Window
    {
        Document doc;
        ExternalCommandData commandData;
        List<Autodesk.Revit.DB.Grid> allGrids;
        string pathToBadSymbolFile = DavidsApp.DirOfAssembly+"BadSymbol.txt";
        List<List<Autodesk.Revit.DB.Grid>> VerHorOthGridList = new List<List<Autodesk.Revit.DB.Grid>>();
        bool selectAllGrids = false;
        static string[] transactionNames = new string[]
        {
            "Перенумерование осей"
        };
        public ReNumGridMainView(ExternalCommandData cd, List<Autodesk.Revit.DB.Grid> gridsFromDoc, List<Autodesk.Revit.DB.Grid> selectedGrids, bool SelectAllGrids)
        {
            InitializeComponent();

            if (File.Exists(DavidsApp.IconPaths[1])) Icon = Imaging.CreateBitmapSourceFromHIcon(new Icon(DavidsApp.IconPaths[1]).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            if (!File.Exists(pathToBadSymbolFile)) File.Create(pathToBadSymbolFile);
            commandData = cd;
            doc = cd.Application.ActiveUIDocument.Document;
            allGrids = gridsFromDoc;
            selectAllGrids = SelectAllGrids;
            var targetGridList = !SelectAllGrids ? selectedGrids : allGrids;

            VerHorOthGridList.Add(targetGridList
                .Where(x => Math.Round(x.Curve.GetEndPoint(0).X, 5) == Math.Round(x.Curve.GetEndPoint(1).X, 5))
                .OrderBy(y => Math.Round(y.Curve.GetEndPoint(0).X, 5))
                .ToList());
            VerHorOthGridList.Add(targetGridList
                .Where(x => Math.Round(x.Curve.GetEndPoint(0).Y, 5) == Math.Round(x.Curve.GetEndPoint(1).Y, 5))
                .OrderBy(y => Math.Round(y.Curve.GetEndPoint(0).Y, 5))
                .ToList());
            VerHorOthGridList.Add(targetGridList
                .Where(x =>
                Math.Round(x.Curve.GetEndPoint(0).X, 5) != Math.Round(x.Curve.GetEndPoint(1).X, 5) &&
                Math.Round(x.Curve.GetEndPoint(0).Y, 5) != Math.Round(x.Curve.GetEndPoint(1).Y, 5))
                .OrderBy(y => y.Name).ToList());


            VerGridList.Text = String.Join("\n", VerHorOthGridList[0].Select(x => x.Name));
            HorGridList.Text = String.Join("\n", VerHorOthGridList[1].Select(x => x.Name));
            OthGridList.Text = String.Join("\n", VerHorOthGridList[2].Select(x => x.Name));
            if (!SelectAllGrids)
            {
                SelectGridsBtn.Content = "Выбрать все оси";
                switch (VerHorOthGridList
                    .Select(x => x.Count).ToList()
                    .IndexOf(VerHorOthGridList.Select(x => x.Count).Max()))
                {
                    case 0:
                        VerCheckBox.IsChecked = true;
                        break;
                    case 1:
                        HorCheckBox.IsChecked = true;
                        break;
                    case 2:
                        OthCheckBox.IsChecked = true;
                        break;
                }
            }
            else SelectGridsBtn.Content = "Выбрать оси на виде";
            
            BadSymbolTextBox.Text = File.ReadAllText(pathToBadSymbolFile);
        }

        private void RenumberingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StartSymbol.Text != null && StartSymbol.Text != "")
            {
                int VerHorOthNum = 0;
                List<TextBox> VerHorOthTextBoxes = new List<System.Windows.Controls.TextBox>();
                VerHorOthTextBoxes.Add(VerGridList);
                VerHorOthTextBoxes.Add(HorGridList);
                VerHorOthTextBoxes.Add(OthGridList);
                if (VerCheckBox.IsChecked.Value) VerHorOthNum = 0;
                if (HorCheckBox.IsChecked.Value) VerHorOthNum = 1;
                if (OthCheckBox.IsChecked.Value) VerHorOthNum = 2;
                List<string> newGridNames = new List<string>();
                List<Autodesk.Revit.DB.Grid> ChousedGrids = new List<Autodesk.Revit.DB.Grid>();


                List<string> gridNamesFromUser = VerHorOthTextBoxes[VerHorOthNum].Text.Split('\n')
                    .Where(x => x != "").Select(y => y.Replace("\r", ""))
                    .Where(o => allGrids.Select(u => u.Name).Contains(o)).ToList();

                using (Transaction transaction = new Transaction(doc))
                {
                    transaction.Start(transactionNames[0]);
                    try
                    {
                        bool EndCheck = EndCheckBox.IsChecked.Value;
                        bool isDigit = StartSymbol.Text.All(x => Char.IsDigit(x));

                        string tempName = ""; int addNum = 0;
                        if (EndCheck) gridNamesFromUser.Reverse();

                        foreach (var el in gridNamesFromUser)
                            ChousedGrids.Add(allGrids.First(x => x.Name == el));

                        foreach (var el in ChousedGrids) el.Name += "*";

                        int GridMaxI = gridNamesFromUser.Count - 1;
                        for (int i = EndCheck ? GridMaxI : 0;
                            EndCheck ? i >= 0 : i < GridMaxI + 1;
                            i += EndCheck ? -1 : 1)
                        {
                            if (isDigit)
                                tempName = EndCheck ?
                                     (Convert.ToInt32(StartSymbol.Text) - (GridMaxI - i)).ToString() :
                                     (Convert.ToInt32(StartSymbol.Text) + (i)).ToString();
                            else
                            {

                                int temp = EndCheck ? i - GridMaxI - addNum : i + addNum;

                                if (BadSymbolTextBox.Text
                                    .Contains(Char.ToLower(Convert.ToChar(StartSymbol.Text.Last() + temp))))
                                    temp = EndCheck ? i - GridMaxI - (++addNum) : i + (++addNum);

                                tempName = StartSymbol.Text.Substring(0, StartSymbol.Text.Length - 1)
                                                + Convert.ToChar(StartSymbol.Text.Last() + temp);
                            }

                            if (!allGrids.Select(x => x.Name).Contains(tempName))
                                ChousedGrids[EndCheck ? GridMaxI - i : i].Name = tempName;
                            else
                            {
                                ChousedGrids[EndCheck ? GridMaxI - i : i].Name =
                                    ChousedGrids[EndCheck ? GridMaxI - i : i].Name.Substring(0, ChousedGrids[EndCheck ? GridMaxI - i : i].Name.Length - 1);
                                TaskDialog.Show("error", "Не перенумерована ось " + ChousedGrids[EndCheck ? GridMaxI - i : i].Name);
                            }
                        }
                    }
                    catch (Exception ex) { TaskDialog.Show("error", ex.Message); }
                    transaction.Commit();
                }
                this.Close();
                ReNumGridMainView MWin =
                    new ReNumGridMainView(commandData, allGrids, ChousedGrids, selectAllGrids);
                MWin.ShowDialog();
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Autodesk.Revit.DB.Grid> ChousedGrids = new List<Autodesk.Revit.DB.Grid>();
                    
            int VerHorOthNum = 0;
            List<TextBox> VerHorOthTextBoxes = new List<System.Windows.Controls.TextBox>();
            VerHorOthTextBoxes.Add(VerGridList);
            VerHorOthTextBoxes.Add(HorGridList);
            VerHorOthTextBoxes.Add(OthGridList);
            if (VerCheckBox.IsChecked.Value) VerHorOthNum = 0;
            if (HorCheckBox.IsChecked.Value) VerHorOthNum = 1;
            if (OthCheckBox.IsChecked.Value) VerHorOthNum = 2;

            List<string> gridNamesFromUser = VerHorOthTextBoxes[VerHorOthNum].Text.Split('\n')
                .Where(x => x != "").Select(y => y.Replace("\r", ""))
                .Where(o => allGrids.Select(u => u.Name).Contains(o)).ToList();

            if (PreNameTextBox.Text == "До") PreNameTextBox.Text = "";
            if (PastNameTextBox.Text == "После") PastNameTextBox.Text = "";

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start(transactionNames[0]);
                try
                {
                    foreach (var el in gridNamesFromUser)
                        ChousedGrids.Add(allGrids.First(x => x.Name == el));
                    foreach (var el in ChousedGrids) el.Name =
                            PreNameTextBox.Text + el.Name + PastNameTextBox.Text;
                }
                catch (Exception ex) { TaskDialog.Show("error", ex.Message); }
                transaction.Commit();
            }
            this.Close();
            ReNumGridMainView MWin =
                new ReNumGridMainView(commandData, allGrids, ChousedGrids, selectAllGrids);
            MWin.ShowDialog();
        }

        private void SelectGridsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();ReNumGridMainView MWin;
            if (SelectGridsBtn.Content.ToString() == "Выбрать оси на виде")
            {
                var uiSelect = commandData.Application.ActiveUIDocument.Selection;
                ISelectionFilter gridsFilter = new GridsFilterClass();
                try
                {
                    List<Reference> reference = uiSelect.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, gridsFilter, "Выберите оси").ToList();
                    List<Autodesk.Revit.DB.Grid> SelectedGrids =
                        reference.Select(x => doc.GetElement(x))
                        .Where(y => y.GetType() == typeof(Autodesk.Revit.DB.Grid))
                        .Cast<Autodesk.Revit.DB.Grid>().ToList();

                    MWin = new ReNumGridMainView(commandData, allGrids, SelectedGrids, false);
                } catch { MWin = new ReNumGridMainView(commandData, allGrids, new List<Autodesk.Revit.DB.Grid>(), true); }
            }
            else MWin = new ReNumGridMainView(commandData, allGrids, new List<Autodesk.Revit.DB.Grid>(), true);
            MWin.ShowDialog();
        }

        private void VerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VerCheckBox.IsChecked = true;
            if (VerHorOthGridList.Count > 1) if (StartCheckBox != null && VerHorOthGridList[0].Count>1) 
                StartSymbol.Text = !EndCheckBox.IsChecked.Value ?
                VerHorOthGridList[0].First().Name :
                VerHorOthGridList[0].Last().Name;
            if (HorCheckBox != null && OthCheckBox != null)
            {
                HorCheckBox.IsChecked = !VerCheckBox.IsChecked;
                OthCheckBox.IsChecked = !VerCheckBox.IsChecked;
            }
        }
        private void HorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HorCheckBox.IsChecked = true;
            if (VerHorOthGridList.Count > 1) if (StartCheckBox != null && VerHorOthGridList[1].Count > 1) 
                StartSymbol.Text = !EndCheckBox.IsChecked.Value ?
                VerHorOthGridList[1].First().Name :
                VerHorOthGridList[1].Last().Name;
            if (VerCheckBox != null && OthCheckBox != null)
            {
                VerCheckBox.IsChecked = !HorCheckBox.IsChecked;
                OthCheckBox.IsChecked = !HorCheckBox.IsChecked;
            }
        }
        private void OthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OthCheckBox.IsChecked = true;
            if(VerHorOthGridList.Count > 1) if (StartCheckBox != null && VerHorOthGridList[2].Count > 1) 
                StartSymbol.Text = !EndCheckBox.IsChecked.Value ?
                VerHorOthGridList[2].First().Name :
                VerHorOthGridList[2].Last().Name;
            if (HorCheckBox != null && VerCheckBox != null)
            {
                HorCheckBox.IsChecked = !OthCheckBox.IsChecked;
                VerCheckBox.IsChecked = !OthCheckBox.IsChecked;
            }
        }

        private void VerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            VerCheckBox.IsChecked = true;
        }
        private void HorCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HorCheckBox.IsChecked = true;
        }
        private void OthCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OthCheckBox.IsChecked = true;
        }

        private void EndCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (StartCheckBox != null) StartCheckBox.IsChecked = !EndCheckBox.IsChecked;

            if (VerCheckBox != null && HorCheckBox != null && OthCheckBox != null)
            {
                bool[] CheckArr = new bool[] {VerCheckBox.IsChecked.Value,
                                          HorCheckBox.IsChecked.Value,
                                          OthCheckBox.IsChecked.Value};

                switch (CheckArr.ToList().IndexOf(CheckArr.Where(x => x == true).First()))
                {
                    case 0:
                        VerCheckBox_Checked(sender, e);
                        break;
                    case 1:
                        HorCheckBox_Checked(sender, e);
                        break;
                    case 2:
                        OthCheckBox_Checked(sender, e);
                        break;
                }
            }
        }

        private void StartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (EndCheckBox != null) EndCheckBox.IsChecked = !StartCheckBox.IsChecked;

            if (VerCheckBox != null && HorCheckBox != null && OthCheckBox != null)
            {
                bool[] CheckArr = new bool[] {VerCheckBox.IsChecked.Value,
                                          HorCheckBox.IsChecked.Value,
                                          OthCheckBox.IsChecked.Value};

                switch (CheckArr.ToList().IndexOf(CheckArr.Where(x => x == true).First()))
                {
                    case 0:
                        VerCheckBox_Checked(sender, e);
                        break;
                    case 1:
                        HorCheckBox_Checked(sender, e);
                        break;
                    case 2:
                        OthCheckBox_Checked(sender, e);
                        break;
                }
            }
        }

        private void BadSymbolTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            File.WriteAllText(pathToBadSymbolFile, BadSymbolTextBox.Text);
        }
    }

    public class GridsFilterClass : ISelectionFilter
    {
        BuiltInCategory selectCategory = BuiltInCategory.OST_Grids;
        public bool AllowElement(Element elem) => elem.Category != null ?
            elem.Category.Id == new ElementId(selectCategory) : false;

        public bool AllowReference(Reference reference, XYZ position) => false;
    }
}
