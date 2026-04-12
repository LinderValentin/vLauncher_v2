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
    public partial class EditorWindow : Window
    {
        private List<string> appNames = new List<string>();
        private List<string> appFiles = new List<string>();
        private int iIndexButton;

        public EditorWindow(int i)
        {
            InitializeComponent();
            iIndexButton = i;
            vLoadFileContent();
        }

        public void vAbbrechen(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
               "Wollen Sie wirklich dieses Fenster schließen? Alle nicht gespeicherten Änderungen gehen verloren!",
               "Warnung",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        public void vSpeichern(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Saves",
                iIndexButton + ".vdata"
            );

            string strButtonName = TxtButtonName.Text;

            if (strButtonName != "")
            {
                string[] zeilen = TxtApps.Text
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                List<string> strAllLines = new List<string>();

                strAllLines.Add(strButtonName);
                strAllLines.AddRange(zeilen);

                File.WriteAllLines(path, strAllLines);

                this.DialogResult = true;
                this.Close();
            }
            else if (strButtonName == "")
            {
                MessageBoxResult result = MessageBox.Show(
                    "Der Buttonname darf nicht leer sein, wenn Sie trotzdem fortfahren wird die Buttoneinstellung gelöscht \n Möchten Sie das Bearbeitungsfenster verlassen?",
                    "Warnung",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(path);
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        public void vLoadFileContent()
        {
            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Saves",
                iIndexButton + ".vdata"
            );

            if (File.Exists(path))
            {
                string[] zeilen = File.ReadAllLines(path);

                if (zeilen.Length > 0)
                {
                    TxtButtonName.Text = zeilen[0];
                    TxtApps.Text = string.Join(Environment.NewLine, zeilen.Skip(1));
                }
            }
        }

        private void TxtApps_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}