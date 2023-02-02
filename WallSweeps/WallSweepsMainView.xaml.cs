using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DavidsRevitApp.WallSweeps
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public partial class WallSweepsMainView : Window
    {
        List<Element> Walls = new List<Element>();
        List<Element> Profiles = new List<Element>();
        List<Element> Materials = new List<Element>();
        Document docIn = null;
        ExternalCommandData cmd = null;
        const double oneMeterDist = 3.28083989501312;
        bool isFirstProfile = false;
        string HelperContentPath = "DavidsRevitApp\\HelperContext.txt";

        WallSweepInfo wsi = new WallSweepInfo(true, WallSweepType.Sweep);
        double lastDist = 0, firstDist = 0, someDist = 0, wallHeight = 0;

        static string[] transactionNames = new string[]
        {
            "LoadFamily",
            "ReNameFamily",
            "CopyFamily",
            "DeleteFamily",
            "ProfiledWall"
        };

        public WallSweepsMainView(List<Element> allWalls, List<Element> allProfiles, List<Element> allMaterials, ExternalCommandData commandData)
        {
            InitializeComponent();

            if (File.Exists(DavidsApp.IconPaths[0]))
                Icon = Imaging.CreateBitmapSourceFromHIcon(new Icon(DavidsApp.IconPaths[0]).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            cmd = commandData;
            docIn = commandData.Application.ActiveUIDocument.Document;
            Walls = allWalls;
            Profiles = allProfiles;
            Materials = allMaterials;

            if (allWalls != null && allProfiles != null)
            {

                AllWallsView.ItemsSource = Walls;
                AllWallsView.DisplayMemberPath = "Name";
                AllWallsView.SelectedIndex = Walls.Count - 1;

                Dictionary<string, Element> profiles = new Dictionary<string, Element>();
                foreach (var el in Profiles.OfType<ElementType>()) profiles.Add(el.FamilyName + " : " + el.Name, el);

                AllProfileView.ItemsSource = profiles;
                AllProfileView.DisplayMemberPath = "Key";
                AllProfileView.SelectedIndex = AllProfileView.Items.Count - 1;

                MaterialClassBox.ItemsSource = Materials.Cast<Material>()
                    .GroupBy(o => o.MaterialClass).Select(x => x.First().MaterialClass);
                MaterialClassBox.SelectedIndex = 0;

                MaterialBox.ItemsSource = Materials;
                MaterialBox.DisplayMemberPath = "Name";
                MaterialBox.SelectedIndex = 0;
            }

            HelpBTN.Focus();
        }

        private void LoadBTN_Click(object sender, RoutedEventArgs e)
        {
            string fName = null;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "rfa files (*.rfa)|*.rfa|All files (*.*)|*.*";
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) fName = ofd.FileName;
            }
            if (fName != null)
            {
                using (Transaction transaction = new Transaction(docIn))
                {
                    transaction.Start(transactionNames[0]);
                    try
                    {
                        docIn.LoadFamily(fName);
                        Profiles = new FilteredElementCollector(docIn)
                                    .OfCategory(BuiltInCategory.OST_ProfileFamilies)
                                    .WhereElementIsElementType()
                                    .Cast<Element>().ToList();
                        Dictionary<string, Element> profiles = new Dictionary<string, Element>();
                        foreach (var el in Profiles.OfType<ElementType>()) profiles.Add(el.FamilyName + " : " + el.Name, el);

                        AllProfileView.ItemsSource = profiles;
                    }
                    catch (Exception ex) { TaskDialog.Show("error", ex.Message); }
                    transaction.Commit();
                }
            }
        }

        private void ReNamerBTN_Click(object sender, RoutedEventArgs e)
        {
            if (AllProfileView.SelectedIndex != -1)
            {
                using (Transaction transaction = new Transaction(docIn))
                {
                    transaction.Start(transactionNames[1]);
                    var SelectEl = AllProfileView.SelectedItems.Cast<KeyValuePair<string, Element>>().FirstOrDefault().Value;
                    FamilySymbol tempProfile = SelectEl as FamilySymbol;
                    WallSweepsFamilyReNamerView reNamer = new WallSweepsFamilyReNamerView(tempProfile);
                    reNamer.ShowDialog();
                    if (reNamer.isFine)
                    {
                        tempProfile.Family.Name = reNamer.familyName;
                        Profiles = new FilteredElementCollector(docIn)
                                            .OfCategory(BuiltInCategory.OST_ProfileFamilies)
                                            .WhereElementIsElementType()
                                            .Cast<Element>().ToList();

                        Dictionary<string, Element> profiles = new Dictionary<string, Element>();
                        foreach (var el in Profiles.OfType<ElementType>()) profiles.Add(el.FamilyName + " : " + el.Name, el);

                        AllProfileView.ItemsSource = profiles;
                    }
                    transaction.Commit();
                }
            }
        }

        private void EditProfileBTN_Click(object sender, RoutedEventArgs e)
        {
            if (AllProfileView.SelectedIndex != -1)
            {

                var SelectEl = AllProfileView.SelectedItems.Cast<KeyValuePair<string, Element>>().FirstOrDefault().Value;
                FamilySymbol tempProfile = SelectEl as FamilySymbol;
                string path = System.IO.Path.GetTempPath();
                string name = tempProfile.Family.Name;
                string fName = name + ".rfa";
                string fPath = path + fName;
                Document familyDoc = docIn.EditFamily(tempProfile.Family);

                if (File.Exists(fPath)) File.Delete(fPath);
                familyDoc.SaveAs(fPath);
                cmd.Application.OpenAndActivateDocument(fPath);
                this.Close();
            }
        }

        private void CopyProfileBTN_Click(object sender, RoutedEventArgs e)
        {
            if (AllProfileView.SelectedIndex != -1)
            {
                var SelectEl = AllProfileView.SelectedItems.Cast<KeyValuePair<string, Element>>().FirstOrDefault().Value;
                FamilySymbol tempProfile = SelectEl as FamilySymbol;
                string path = System.IO.Path.GetTempPath();
                string name = tempProfile.Family.Name + "_Copy";
                string fName = name + ".rfa";
                string fPath = path + fName;
                Document familyDoc = docIn.EditFamily(tempProfile.Family);
                familyDoc.SaveAs(fPath);
                using (Transaction transaction = new Transaction(docIn))
                {
                    transaction.Start(transactionNames[2]);

                    docIn.LoadFamily(fPath);
                    Profiles = new FilteredElementCollector(docIn)
                                                .OfCategory(BuiltInCategory.OST_ProfileFamilies)
                                                .WhereElementIsElementType()
                                                .Cast<Element>().ToList();

                    Dictionary<string, Element> profiles = new Dictionary<string, Element>();
                    foreach (var el in Profiles.OfType<ElementType>()) profiles.Add(el.FamilyName + " : " + el.Name, el);

                    AllProfileView.ItemsSource = profiles;
                    transaction.Commit();
                }
            }
        }

        private void DeleteBTN_Click(object sender, RoutedEventArgs e)
        {
            if (AllProfileView.SelectedIndex != -1)
            {
                var SelectEl = AllProfileView.SelectedItems.Cast<KeyValuePair<string, Element>>().FirstOrDefault().Value;
                FamilySymbol tempProfile = SelectEl as FamilySymbol;
                if (tempProfile != null)
                {
                    using (Transaction transaction = new Transaction(docIn))
                    {
                        transaction.Start(transactionNames[3]);

                        docIn.Delete(tempProfile.Family.Id);
                        Profiles = new FilteredElementCollector(docIn)
                                                    .OfCategory(BuiltInCategory.OST_ProfileFamilies)
                                                    .WhereElementIsElementType()
                                                    .Cast<Element>().ToList();

                        Dictionary<string, Element> profiles = new Dictionary<string, Element>();
                        foreach (var el in Profiles.OfType<ElementType>()) profiles.Add(el.FamilyName + " : " + el.Name, el);

                        AllProfileView.ItemsSource = profiles;
                        transaction.Commit();
                    }
                }
            }
        }

        private void FunctionBTN_Click(object sender, RoutedEventArgs e)
        {
            WallType SelectedWallType = null;
            FamilySymbol SelectedProfile = null;
            Material SelectedMaterial = null;
            CompoundStructure p = null;

            try
            {
                SelectedWallType = AllWallsView.SelectedItem as WallType;
                var SelectEl = AllProfileView.SelectedItems.Cast<KeyValuePair<string, Element>>().FirstOrDefault().Value;
                SelectedProfile = SelectEl as FamilySymbol;
                SelectedMaterial = MaterialBox.SelectedItem as Material;

                p = SelectedWallType.GetCompoundStructure();
                p.ClearWallSweeps(WallSweepType.Sweep);

                ElementId endProfile = ElementId.InvalidElementId;
                ElementId endMaterialId = SelectedMaterial.Id;
                int endId = 123124;
                if (AllProfileView.SelectedIndex != -1) endProfile = SelectedProfile.Id;


                double endWallOffset = Convert.ToDouble(WallOffsetBox.Text) * oneMeterDist / 1000;
                wsi.WallOffset = endWallOffset;
                if (isFirstProfile) p.AddWallSweep(wsi);
                lastDist = 0; someDist = Convert.ToDouble(DistText.Text) / 1000;
                wallHeight = Convert.ToDouble(WallHeightText.Text);
                firstDist = Convert.ToDouble(StartDistBox.Text);
                if (DistText.Text != "0") while (lastDist < wallHeight / 1000)
                    {
                        wsi = new WallSweepInfo(true, WallSweepType.Sweep)
                        {
                            Id = endId,
                            MaterialId = endMaterialId,
                            Distance = lastDist * oneMeterDist,
                            ProfileId = endProfile,
                            WallOffset = endWallOffset
                        };
                        if (firstDist != 0)
                        {
                            wsi.Distance = firstDist * oneMeterDist / 1000;
                            lastDist = firstDist / 1000;
                            firstDist = 0;
                        }
                        endId += 1;
                        p.AddWallSweep(wsi);
                        lastDist += someDist;
                    }
            }
            catch (Exception exDist) { TaskDialog.Show("error", exDist.Message); }

            using (Transaction transaction = new Transaction(docIn))
            {
                transaction.Start(transactionNames[4]);

                try { SelectedWallType.SetCompoundStructure(p); }
                catch (Exception ex) { TaskDialog.Show("error", ex.Message); }

                transaction.Commit();
            }
            this.Close();
        }

        private void MaterialCategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MaterialBox.ItemsSource = Materials.Cast<Material>()
                .Where(o => o.MaterialClass == MaterialClassBox.SelectedItem.ToString());
            MaterialBox.SelectedIndex = 0;
        }

        private void AddFirstProfileBTN_Click(object sender, RoutedEventArgs e)
        {
            WallSweepsProfileSelecterView viewer = new WallSweepsProfileSelecterView(Profiles, Materials, lastDist, firstDist, wsi, cmd);
            viewer.ShowDialog();
            if (viewer.isFirstProfile)
            {
                isFirstProfile = viewer.isFirstProfile;
                wsi = viewer.wsi;
                StartDistBox.Text = viewer.firstDist.ToString();
            }
        }

        private void HelpBTN_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helper = new HelpWindow(HelperContentPath);
            try
            {
                helper.Show();
            }
            catch { }
        }
    }
}
