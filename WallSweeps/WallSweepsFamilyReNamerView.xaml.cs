using Autodesk.Revit.DB;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DavidsRevitApp.WallSweeps
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class WallSweepsFamilyReNamerView : Window
    {
        public string familyName = null;
        public bool isFine = false;
        public WallSweepsFamilyReNamerView(FamilySymbol tempProfile)
        {
            InitializeComponent();

            if (File.Exists(DavidsApp.IconPaths[0])) Icon = Imaging.CreateBitmapSourceFromHIcon(new Icon(DavidsApp.IconPaths[0]).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            this.SizeToContent = SizeToContent.Width;

            familyName = tempProfile.Family.Name;
            FamilyNameBox.Text = familyName;
        }

        private void ReNamerBTN_Click(object sender, RoutedEventArgs e)
        {
            isFine = true;
            familyName = FamilyNameBox.Text;
            this.Close();
        }
    }
}
