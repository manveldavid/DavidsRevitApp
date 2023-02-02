using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DavidsRevitApp.WallSweeps
{
    /// <summary>
    /// Логика взаимодействия для HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        int MAXWIDTH = 800, MAXHEIGHT = 500;
        public HelpWindow(string pathToContext)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            HelperMainTextBox.MaxHeight= MAXHEIGHT;
            HelperMainTextBox.MaxWidth= MAXWIDTH;
            try
            {
                HelperMainTextBox.Text = File.ReadAllText(pathToContext);
            }
            catch(Exception ex) { 
                TaskDialog.Show("error!", "File: " + pathToContext + " does not exist!" + 
                    "\n\nDetails: " + ex.Message); 
                this.Close(); }
        }
    }
}
