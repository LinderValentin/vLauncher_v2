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

        //Menue Open by clicking on the image in the botton right corner
        private void MenuToggle_Click(object sender, EventArgs e)
        {
            MenuPopup.IsOpen = !MenuPopup.IsOpen;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MenuPopup.IsOpen)
                MenuPopup.IsOpen = false;
        }

        public void vOpenInfos(object sender, RoutedEventArgs e)
        {
            CloseMenu(sender, e);

            Info info = new Info();
            info.ShowDialog();
        }

        public void vMenueHeadlines(object sender, RoutedEventArgs e)
        {
            CloseMenu(sender, e);
            vChangeHeadlines(sender, e);
        }

        public void vMenueReset (object sender, RoutedEventArgs e)
        {
            CloseMenu(sender, e);
            vResetFiles(sender, e);
        }

        public void vCloseSoftware(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }




        private void CloseMenu(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
        }

        //----------------------------------------------------------------------------------------------------

        //Double Click to Change Headlines
        public void vDoubleClickHeadline(object sender, MouseButtonEventArgs e)
        {
            DateTime now = DateTime.Now;

            // Double Click Erkennung
            if ((now - lastClickTime).TotalMilliseconds <= 300)
            {
                vChangeHeadlines(sender, e);
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
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines.vdata");
            string path2 = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves");

            string[] daten = { "unbenutzt", "unbenutzt", "unbenutzt", "unbenutzt" };


            YesNoMessage yesNoMessage = new YesNoMessage("Reset1");
            yesNoMessage.ShowDialog();

            if (yesNoMessage.DialogResult == true)
            {
                YesNoMessage yesNoMessage2 = new YesNoMessage("Reset2");
                yesNoMessage2.ShowDialog();

                if (yesNoMessage2.DialogResult == true)
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

                    YesNoMessage yesNoMessage3 = new YesNoMessage("Restart");
                    yesNoMessage3.ShowDialog();

                    if (yesNoMessage3.DialogResult == true)
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

            YesNoMessage yesNoMessage = new YesNoMessage("ButtonChange");
            yesNoMessage.ShowDialog();

            if (yesNoMessage.DialogResult == true)
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
                OkMessage okMessage = new OkMessage("DirectoryNotFound");
                return;
            }

            string filePath = IOPath.Combine(path, iSender + ".vdata");

            if (!File.Exists(filePath))
            {
                OkMessage okMessage = new OkMessage("FileNotFound");
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