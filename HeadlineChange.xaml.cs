using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IOPath = System.IO.Path;
using System.Windows.Input;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Window;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace vLauncher
{
    public partial class HeadlineChange : Window
    {
        public HeadlineChange()
        {
            InitializeComponent();
            vLoadHeadlines();
        }

        public void vBtnAbbrechen(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void vLoadHeadlines()
        {
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines.vdata");

            List<string> strList = new List<string>(File.ReadAllLines(path));

            TxtHeadline1.Text = strList[0];
            TxtHeadline2.Text = strList[1];
            TxtHeadline3.Text = strList[2];
            TxtHeadline4.Text = strList[3];
        }

        public void vChangeHeadlinesDatei(object sender, RoutedEventArgs e)
        {
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines.vdata");

            string[] daten =
            {
                TxtHeadline1.Text,
                TxtHeadline2.Text,
                TxtHeadline3.Text,
                TxtHeadline4.Text
            };

            File.WriteAllLines(path, daten);

            this.DialogResult = true;
            this.Close();
        }

        private void TxtHeadline1_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}