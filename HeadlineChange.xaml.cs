using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace vLauncher
{
    /// <summary>
    /// Interaktionslogik für HeadlineChange.xaml
    /// </summary>
    public partial class HeadlineChange : Window
    {
        // ✅ NEU: zentraler AppData Pfad
        private static readonly string BasePath =
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "vLauncher",
                "Saves"
            );

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
            string path = System.IO.Path.Combine(BasePath, "headlines.vdata");

            if (File.Exists(path))
            {
                List<string> strList = new List<string>(File.ReadAllLines(path));

                if (strList.Count >= 4)
                {
                    TxtHeadline1.Text = strList[0];
                    TxtHeadline2.Text = strList[1];
                    TxtHeadline3.Text = strList[2];
                    TxtHeadline4.Text = strList[3];
                }
            }
        }

        public void vChangeHeadlinesDatei(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(BasePath);

            string path = System.IO.Path.Combine(BasePath, "headlines.vdata");

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