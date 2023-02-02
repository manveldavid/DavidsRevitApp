using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Логика взаимодействия для FlagContextView.xaml
    /// </summary>
    public partial class FlagContextView : Window
    {
        double NormOfOneMeter = 3.28083989501312;

        ExternalCommandData cmd;
        ElementId SelectedFlag;
        List<ElementId> ConstrIds;

        string[] transactionNames = new string[]
        {
            "Заполнение автофлажка",
            "Удаление старых значений автофлажка"
        };

        public FlagContextView(string FlagContext, ExternalCommandData cmd, ElementId SelectedFlag, List<ElementId> ConstrIds)
        {
            this.cmd = cmd;
            this.SelectedFlag = SelectedFlag;
            this.ConstrIds = ConstrIds;
            InitializeComponent();

            if (File.Exists(DavidsApp.IconPaths[2])) Icon = Imaging.CreateBitmapSourceFromHIcon(new Icon(DavidsApp.IconPaths[2]).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            MainTextBox.Text = FlagContext;
        }

        private void ParseFlagBtn_Click(object sender, RoutedEventArgs e)
        {
            AnnotationSymbol MainAutoFlag = cmd.Application.ActiveUIDocument.Document.GetElement(SelectedFlag) as AnnotationSymbol;
            List<Parameter> Params = MainAutoFlag.GetOrderedParameters().ToList();

            Parameter NLine = Params.First(x => x.Definition.Name.Contains("Число строк"));
            Parameter LineWidth = Params.First(x => x.Definition.Name.Contains("Ширина строк"));

            List<Parameter> ParamNames = Params.Where(o => o.Definition.Name.Contains("Текст")).OrderBy(l => l.Definition.Name).ToList();
            List<Parameter> ParamWidths = Params.Where(o => o.Definition.Name.Contains("Толщина")).OrderBy(l => l.Definition.Name).ToList();

            List<string> Names = null;
            List<string> Widths = null;

            if (MainTextBox.Text != "")
            {
                Names = MainTextBox.Text.Split('\n').ToList()
                    .Select(l => (l.LastIndexOf('-') != -1 ? l.Substring(0, l.LastIndexOf('-')) : l))
                    .Select(p => p.Trim()).ToList();
                Widths = MainTextBox.Text.Split('\n')
                    .Select(l => (l.LastIndexOf('-') != -1 ? l.Substring(l.LastIndexOf('-') + 1, l.Length - l.LastIndexOf('-') - 1) : ""))
                    .Select(p => p.Trim()).ToList();
            }

            if (Names != null)
            {
                using (Transaction transaction = new Transaction(
                    cmd.Application.ActiveUIDocument.Document))
                {
                    transaction.Start(transactionNames[1]);

                    foreach (var el in ParamNames) el.Set("");
                    foreach (var el in ParamWidths) el.Set("");

                    transaction.Commit();
                }

                using (Transaction transaction = new Transaction(
                    cmd.Application.ActiveUIDocument.Document))
                {
                    transaction.Start(transactionNames[0]);

                    int iterations = -1;
                    for (int i = 0; i < Names.Count; i++)
                    {
                        if (Names[i] != "" && iterations <= 15)
                        {
                            iterations++;
                            if (Widths[i].All(o => !Char.IsDigit(o)))
                            {
                                Names[i] += " - " + Widths[i];
                                Widths[i] = "";
                            }
                            ParamNames[iterations].Set(Names[i]);
                            if (Widths[i] != null)
                                ParamWidths[iterations].Set(Widths[i].Length > 1 ? Widths[i] : "");
                        }
                    }
                    NLine.Set(iterations + 1);
                    var MainFlagWidth = (NormOfOneMeter * Convert.ToDouble(SomeNumTextBox.Text) / 1000) *
                        (Names.Select(o => o.Length).Max() + 2 +
                         Widths.Select(o => o.Length).Max());
                    LineWidth.Set(MainFlagWidth);

                    transaction.Commit();
                }
            }
            
            this.Close();
        }

        private void ReverseFlagBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainTextBox.Text != "")
                MainTextBox.Text = String.Join("\n", MainTextBox.Text.Split('\n').Reverse());
        }
    }
}
