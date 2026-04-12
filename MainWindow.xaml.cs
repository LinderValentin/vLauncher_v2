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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    /*
            Process.Start(new ProcessStartInfo(@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe") { UseShellExecute = true }); //VS
            Process.Start(new ProcessStartInfo(@"C:\Users\linde\AppData\Local\Programs\Microsoft VS Code\Code.exe") { UseShellExecute = true }); //VS Code
            Process.Start(new ProcessStartInfo("https://chatgpt.com/") { UseShellExecute = true }); //Chatgpt
            Process.Start(new ProcessStartInfo(@"C:\Users\linde\Desktop\Coding") { UseShellExecute = true }); //Coding Ordner
            Process.Start(new ProcessStartInfo("msteams:") { UseShellExecute = true });//Teams
    */


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            vLoadHeadlines();
            vLoadButtons();
        }

        public void vLoadHeadlines()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves", "headlines.vdata");

            List<string> strList = new List<string>(File.ReadAllLines(path));

            Headline1.Text = strList[0];
            Headline2.Text = strList[1];
            Headline3.Text = strList[2];
            Headline4.Text = strList[3];
        }

        public void vChangeHeadlines(object sender, RoutedEventArgs e)
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
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves", "headlines.vdata");
            string path2 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            string[] daten = { "unbenutzt", "unbenutzt", "unbenutzt", "unbenutzt" };


            File.WriteAllLines(path, daten);

            string[] files = Directory.GetFiles(path2, "*.vdata");

            foreach (string file in files)
            {
                if (IOPath.GetFileName(file) != "headlines.vdata")
                {
                    File.Delete(file);
                }
            }

            MessageBoxResult result = MessageBox.Show(
                "Die App muss neu gestartet werden, damit die Änderungen wirksam werden. Möchten Sie die App jetzt neu starten?",
                "Neustart erforderlich",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBoxResult result2 = MessageBox.Show(
                    "Die App muss neu gestartet werden, damit die Änderungen wirksam werden. Möchten Sie die App jetzt neu starten? (2. Überprüfung)",
                    "Neustart erforderlich (2. Überprüfung)",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result2 == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                }


            }

            
        }

        public void vReloadData()
        {
            MainWindow neu = new MainWindow();
            neu.Show();

            this.Close();
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
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            if (!Directory.Exists(path)) return;

            string[] files = Directory.GetFiles(path, "*.vdata");

            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(file);

                if (name == "headlines")
                    continue;

                string[] content = File.ReadAllLines(file);

                // find button by the same id as the filename (Btn11 -> file "11.vdata")
                Button btn = (Button)FindName("Btn" + name);

                if (btn != null)
                {
                // first line is the name; if empty use "unbenutzt"
                    var raw = content.Length > 0 ? content[0] : string.Empty;
                    var display = string.IsNullOrWhiteSpace(raw) ? "unbenutzt" : raw.Trim();
                    btn.Content = display;

                    // if the name is set (not the default 'unbenutzt' or 'leer'), give it the blue style
                    var blue = TryFindResource("ModernBlueButton") as Style;
                    var disabled = TryFindResource("DisabledButtonStyle") as Style;
                    if (!string.Equals(display, "unbenutzt", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(display, "leer", StringComparison.OrdinalIgnoreCase) && blue != null)
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

            int iSender;
            if (!int.TryParse(btn.Tag?.ToString(), out iSender))
            {
                MessageBox.Show("Ungültiger Button-Tag.");
                return;
            }

            string path = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Saves-Ordner nicht gefunden.");
                return;
            }

            string filePath = IOPath.Combine(path, iSender + ".vdata");

            if (!File.Exists(filePath))
            {
                MessageBox.Show(
                    $"Für diesen Button (ID: {iSender}) wurde keine Datei gefunden. Bitte ändern/bearbeiten Sie den Button.",
                    "Keine Daten vorhanden",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Datei '{filePath}' nicht gefunden.");
                return;
            }

            List<string> strAppsList = File.ReadAllLines(filePath).ToList();

            // Optional: erste Zeile entfernen (z.B. Header)
            if (strAppsList.Count > 0)
                strAppsList.RemoveAt(0);

            foreach (string file in strAppsList)
            {
                string strType = strNoticeType(file);

                try
                {
                    switch (strType)
                    {
                        case "url":
                        case "file":
                        case "directory":
                        case "executable":
                            Process.Start(new ProcessStartInfo(file)
                            {
                                UseShellExecute = true
                            });
                            break;

                        case "special":
                            Process.Start(new ProcessStartInfo(file)
                            {
                                UseShellExecute = true
                            });
                            //MessageBox.Show($"Spezialpfad '{file}' wird noch nicht unterstützt.");
                            break;

                        default:
                            MessageBox.Show($"Ungültiger oder unbekannter Eintrag:\n{file}",
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Starten von:\n{file}\n\n{ex.Message}",
                        "Startfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public string strNoticeType(string strData)
        {
            if (string.IsNullOrWhiteSpace(strData))
                return "unbekannt";

            strData = strData.Trim();

            if (strData.StartsWith("http://") || strData.StartsWith("https://"))
            {
                return "url";
            }
            else if (strData.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return "executable";
            }
            else if (File.Exists(strData))
            {
                return "file";
            }
            else if (Directory.Exists(strData))
            {
                return "directory";
            }
            else if (strData.EndsWith(":", StringComparison.OrdinalIgnoreCase))
            {
                return "special";
            }

            return "unbekannt";
        }
    }
}