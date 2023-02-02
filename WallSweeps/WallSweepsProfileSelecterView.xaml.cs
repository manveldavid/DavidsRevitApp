using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для UserControl2.xaml
    /// </summary>
    [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
    public partial class WallSweepsProfileSelecterView : Window
    {
        public List<Element> Walls = new List<Element>();
        public List<Element> Profiles = new List<Element>();
        public List<Element> Materials = new List<Element>();
        const double oneMeterDist = 3.28083989501312;
        public bool isFirstProfile = false;
        Document docIn = null;
        ExternalCommandData cmd = null;
        public WallSweepInfo wsi = new WallSweepInfo(true, WallSweepType.Sweep);
        public double lastDist = 0, firstDist = 0, someDist = 0, wallHeight = 0;

        static string[] transactionNames = new string[]
        {
            "LoadFamily",
            "ReNameFamily",
            "CopyFamily",
            "DeleteFamily"
        };

        public WallSweepsProfileSelecterView(List<Element> allProfiles, List<Element> allMaterials, double lastDist, double firstDist, WallSweepInfo wsi, ExternalCommandData cmd)
        {
            InitializeComponent();

            if (File.Exists(DavidsApp.IconPaths[0]))
                Icon = Imaging.CreateBitmapSourceFromHIcon(new Icon(DavidsApp.IconPaths[0]).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            Profiles = allProfiles;
            Materials = allMaterials;

            docIn = cmd.Application.ActiveUIDocument.Document;
            this.cmd = cmd;

            if (allProfiles != null && allMaterials != null)
            {
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
        }

        private void LoadBTN_Click(object sender, RoutedEventArgs e)
        {
            string fName = null;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "rfa files (*.rfa)|*.rfa|All files (*.*)|*.*";
                ofd.RestoreDirectory = true;
                fName = ofd.FileName; 
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) fName = ofd.FileName;
            }
            if (fName!=null)
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
            try
            {
                isFirstProfile = true;
                var SelectEl = AllProfileView.SelectedItems.Cast<KeyValuePair<string, Element>>().FirstOrDefault().Value;
                FamilySymbol SelectedProfile = SelectEl as FamilySymbol;
                Material SelectedMaterial = MaterialBox.SelectedItem as Material;

                ElementId endProfile = ElementId.InvalidElementId;
                int endId = 123123;
                if (AllProfileView.SelectedIndex != -1) endProfile = SelectedProfile.Id;
                wsi.Distance = Convert.ToDouble(DistText.Text) * oneMeterDist / 1000;
                wsi.ProfileId = endProfile;
                wsi.MaterialId = SelectedMaterial.Id;
                wsi.Id = endId;
                firstDist = Convert.ToDouble(WallHeightText.Text) + Convert.ToDouble(DistText.Text);
                this.Close();
            }
            catch (Exception ex) { TaskDialog.Show("error", ex.Message); }
        }

        private void MaterialCategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MaterialBox.ItemsSource = Materials.Cast<Material>()
                .Where(o => o.MaterialClass == MaterialClassBox.SelectedItem.ToString());
            MaterialBox.SelectedIndex = 0;
        }
    }
}
