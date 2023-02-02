using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DavidsRevitApp.AutoFlag
{
    /// <summary>
    /// Логика взаимодействия для ArrowDirectionView.xaml
    /// </summary>
    public partial class ArrowDirectionView : Window
    {
        ElementId SelectedFlagId;
        Document doc;
        static string ImgPath = DavidsApp.DirOfAssembly + "ArrowDirection.JPG";
        double NormOfOneMeter = 3.28083989501312;

        static string[] transactionNames = new string[]
        {
            "Выбор направления автофлажка"
        };

        Parameter LeftRight;
        Parameter Degree;
        Parameter ArrowLength;
        double OldArrowLength;

        public ArrowDirectionView(ElementId SelectedFlagId, Document doc)
        {
            InitializeComponent();

            this.SelectedFlagId = SelectedFlagId;
            this.doc = doc;

            var MainFlag = doc.GetElement(SelectedFlagId) as AnnotationSymbol;

            if (File.Exists(DavidsApp.IconPaths[2])) 
                Icon = Imaging.CreateBitmapSourceFromHIcon(new Icon(DavidsApp.IconPaths[2]).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            if (File.Exists(ImgPath))
                ArrowedImg.Source = Imaging.CreateBitmapSourceFromHBitmap(new Bitmap(ImgPath).GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            LeftRight = MainFlag.GetOrderedParameters().First(o => o.Definition.Name.Contains("Правый"));
            Degree = MainFlag.GetOrderedParameters().First(o => o.Definition.Name.Contains("Положение выноски"));
            ArrowLength = MainFlag.GetOrderedParameters().First(o => o.Definition.Name.Contains("Длина стрелки"));

            OldArrowLength = Math.Round(ArrowLength.AsDouble() * 1000 / NormOfOneMeter);
            ArrowLengthTextBox.Text = OldArrowLength.ToString();

            /*Задаем старое направление стрелки*/{
                List<RadioButton> RB = new RadioButton[] 
                {
                    DownLeftRB,
                    LeftUpRB,
                    UpLeftRB,
                    LeftDownRB,

                    DownRightRB,
                    RightUpRB,
                    UpRightRB,
                    RightDownRB
                }.ToList();

                int RbIndex = LeftRight.AsInteger() == 1 ? 4 : 0;
                int DirArrow = Degree.AsInteger() == 90 ? 3 : Degree.AsInteger();

                if (DirArrow < 4) 
                    RB[RbIndex + DirArrow].IsChecked = true;
            }

        }

        private void SetDirectionBtn_Click(object sender, RoutedEventArgs e)
        {
            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start(transactionNames[0]);

                Degree.Set(DownLeftRB.IsChecked.Value ? 0 :
                           LeftDownRB.IsChecked.Value ? 90 :
                           LeftUpRB.IsChecked.Value ? 1 :
                           UpLeftRB.IsChecked.Value ? 2 :
                           DownRightRB.IsChecked.Value ? 0 :
                           RightDownRB.IsChecked.Value ? 90 :
                           RightUpRB.IsChecked.Value ? 1 :
                           UpRightRB.IsChecked.Value ? 2 : 1);

                LeftRight.Set(DownLeftRB.IsChecked.Value ? 0 :
                              LeftDownRB.IsChecked.Value ? 0 :
                              LeftUpRB.IsChecked.Value ? 0 :
                              UpLeftRB.IsChecked.Value ? 0 :
                              DownRightRB.IsChecked.Value ? 1 :
                              RightDownRB.IsChecked.Value ? 1 :
                              RightUpRB.IsChecked.Value ? 1 :
                              UpRightRB.IsChecked.Value ? 1 : 0);

                try   { ArrowLength.Set(NormOfOneMeter * Convert.ToDouble(ArrowLengthTextBox.Text.Replace(".", ","))/1000); }
                catch { ArrowLength.Set(NormOfOneMeter * OldArrowLength / 1000); }

                transaction.Commit();
            }
            this.Close();
        }
    }
}
