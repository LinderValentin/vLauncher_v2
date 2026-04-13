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
    public partial class MainWindow : Window
    {
        private DateTime lastClickTime;

        public MainWindow()
        {
            InitializeComponent();
            vEnsureExistPath();
            vLoadHeadlines();
            vLoadButtons();
        }

        public void vDoubleClickHeadline(object sender, MouseButtonEventArgs e)
        {
            DateTime now = DateTime.Now;

            // Double Click Erkennung
            if ((now - lastClickTime).TotalMilliseconds <= 300)
            {
                vChangeHeadlines();
            }

            lastClickTime = now;
        }

        public void vLoadHeadlines()
        {
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines.vdata");

            List<string> strList = new List<string>(File.ReadAllLines(path));

            Headline1.Text = strList[0];
            Headline2.Text = strList[1];
            Headline3.Text = strList[2];
            Headline4.Text = strList[3];
        }

        public void vChangeHeadlines()
        {
            HeadlineChange headlineChange = new HeadlineChange();
            headlineChange.ShowDialog();

            if (headlineChange.DialogResult == true)
            {
                vReloadData();
            }
        }

        public void vResetFiles(object sender, RoutedEventArgs e)
        {
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines.vdata");
            string path2 = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves");

            string[] daten = { "unbenutzt", "unbenutzt", "unbenutzt", "unbenutzt" };


            MessageBoxResult result = MessageBox.Show(
                "Wollen Sie wirklich alle Daten zurücksetzen? Alle Buttons werden auf 'unbenutzt' gesetzt und alle Apps gelöscht. | Abfrage 1",
                "vLauncher - Daten zurücksetzen | Abfrage 1",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MessageBoxResult result2 = MessageBox.Show(
                    "Wollen Sie wirklich alle Daten zurücksetzen? Alle Buttons werden auf 'unbenutzt' gesetzt und alle Apps gelöscht. | Abfrage 2",
                    "vLauncher - Daten zurücksetzen | Abfrage 2",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result2 == MessageBoxResult.Yes)
                {

                    File.WriteAllLines(path, daten);

                    string[] files = Directory.GetFiles(path2, "*.vdata");

                    foreach (string file in files)
                    {
                        if (IOPath.GetFileName(file) != "headlines.vdata")
                        {
                            File.Delete(file);
                        }
                    }

                    MessageBoxResult result3 = MessageBox.Show(
                        "Die App muss neu gestartet werden, damit die Änderungen wirksam werden. Möchten Sie die App jetzt neu starten?",
                        "Neustart erforderlich",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result3 == MessageBoxResult.Yes)
                    {
                        Application.Current.Shutdown();
                        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    }
                }
            }
        }

        public void vReloadData()
        {
            vLoadHeadlines();
            vLoadButtons();
        }

        public void vChangeButtons(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            int iSender = int.Parse(btn.Tag.ToString());

            MessageBoxResult result = MessageBox.Show(
                "Wollen Sie den Button ändern?",
                "Button ändern",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                EditorWindow buttonChange = new EditorWindow(iSender);
                buttonChange.ShowDialog();

                if (buttonChange.DialogResult == true)
                {
                    vReloadData();
                }
            }
        }

        public void vLoadButtons()
        {
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves");

            if (!Directory.Exists(path)) return;

            string[] files = Directory.GetFiles(path, "*.vdata");

            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(file);

                if (name == "headlines")
                    continue;

                string[] content = File.ReadAllLines(file);

                Button btn = (Button)FindName("Btn" + name);

                if (btn != null)
                {
                    var raw = content.Length > 0 ? content[0] : string.Empty;
                    var display = string.IsNullOrWhiteSpace(raw) ? "unbenutzt" : raw.Trim();
                    btn.Content = display;

                    var blue = TryFindResource("ModernBlueButton") as Style;
                    var disabled = TryFindResource("DisabledButtonStyle") as Style;

                    if (!string.Equals(display, "unbenutzt", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(display, "leer", StringComparison.OrdinalIgnoreCase) &&
                        blue != null)
                    {
                        btn.Style = blue;
                    }
                    else if (disabled != null)
                    {
                        btn.Style = disabled;
                    }
                }
            }
        }

        public void vLoadApps(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            int iSender = int.Parse(btn.Tag.ToString());

            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves" );

            if (!Directory.Exists(path))
            {
                MessageBox.Show("Saves-Ordner nicht gefunden.");
                return;
            }

            string filePath = IOPath.Combine(path, iSender + ".vdata");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Keine Datei gefunden.");
                return;
            }

            List<string> strAppsList = File.ReadAllLines(filePath).ToList();

            if (strAppsList.Count > 0)
                strAppsList.RemoveAt(0);

            foreach (string file in strAppsList)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(file)
                    {
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void vEnsureExistPath()
        {
            string[] strData = { "unbenutzt", "unbenutzt", "unbenutzt", "unbenutzt" };

            string path = IOPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "vLauncher",
                "Saves"
            );

            // Ordner erstellen (falls nicht vorhanden)
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Datei prüfen + erstellen
            string filePath = IOPath.Combine(path, "headlines.vdata");

            if (!File.Exists(filePath))
            {
                File.WriteAllLines(filePath, strData);
            }
        }

        //for custom window top bar (not default - windows one)
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //only one windows allowed


        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}