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


using vLauncher.Helpers;

namespace vLauncher
{
    public partial class MainWindow : Window
    {
        private DateTime lastClickTime;

        public MainWindow()
        {
            InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.Manual;

            if (Properties.Settings.Default.WindowLeft != 0 ||
                Properties.Settings.Default.WindowTop != 0)
            {
                this.Left = Properties.Settings.Default.WindowLeft;
                this.Top = Properties.Settings.Default.WindowTop;
            }

            vEnsureExistPath();
            vLoadHeadlines();
            vBuildHeadlineCards();
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
            WindowPositionHelper.CenterToOwner(info, this);
            info.ShowDialog();
        }

        public void vMenueHeadlines(object sender, RoutedEventArgs e)
        {
            CloseMenu(sender, e);
            vChangeHeadlines(sender, e);
        }

        public void vMenueReset(object sender, RoutedEventArgs e)
        {
            CloseMenu(sender, e);
            vResetFiles(sender, e);
        }

        public void vCloseSoftware(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void vMenueExport(object sender, RoutedEventArgs e)
        {
            CloseMenu(sender, e);
            ExportWindow exportWindow = new ExportWindow();
            WindowPositionHelper.CenterToOwner(exportWindow, this);
            exportWindow.ShowDialog();

        }

        public void vMenueImport(object sender, RoutedEventArgs e)
        {
            CloseMenu(sender, e);
            ImportWindow importWindow = new ImportWindow();
            WindowPositionHelper.CenterToOwner(importWindow, this);
            importWindow.ShowDialog();

            vReloadData();

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

            // Ensure at least 4 entries for backward compatibility
            while (strList.Count < 4)
                strList.Add("unbenutzt");

            // Store headlines in Tag of CardStackPanel for access by other methods
            CardStackPanel.Tag = strList;
        }

        public void vChangeHeadlines(object sender, RoutedEventArgs e)
        {
            HeadlineChange headlineChange = new HeadlineChange();
            WindowPositionHelper.CenterToOwner(headlineChange, this);
            headlineChange.ShowDialog();

            if (headlineChange.DialogResult == true)
            {
                vReloadData();
            }
        }

        // Builds headline cards dynamically based on headlines.vdata
        public void vBuildHeadlineCards()
        {
            CardStackPanel.Children.Clear();

            var headlines = CardStackPanel.Tag as List<string> ?? new List<string> { "unbenutzt", "unbenutzt", "unbenutzt", "unbenutzt" };

            for (int i = 0; i < headlines.Count; i++)
            {
                int headlineIndex = i + 1;

                // Create Border (card)
                var border = new Border()
                {
                    Style = (Style)FindResource("CardStyle")
                };

                var stack = new StackPanel();

                var txt = new TextBlock()
                {
                    Text = headlines[i],
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 12),
                    Foreground = Brushes.White
                };
                txt.MouseLeftButtonDown += vDoubleClickHeadline;

                stack.Children.Add(txt);

                var wrap = new WrapPanel();

                for (int btn = 1; btn <= 5; btn++)
                {
                    string tag = (headlineIndex * 10 + btn).ToString();
                    var button = new Button()
                    {
                        Tag = tag,
                        Content = "unbenutzt",
                        Style = (Style)FindResource("DisabledButtonStyle"),
                        Margin = new Thickness(6),
                        MinWidth = 140,
                        MinHeight = 52
                    };
                    button.Click += vLoadApps;
                    button.MouseRightButtonUp += vChangeButtons;

                    // Register name so FindName can locate it later
                    try
                    {
                        this.RegisterName($"Btn{tag}", button);
                    }
                    catch { }

                    // Try to load button display/style from corresponding .vdata file
                    try
                    {
                        string savesPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves");
                        string filePath = IOPath.Combine(savesPath, tag + ".vdata");
                        if (File.Exists(filePath))
                        {
                            var content = File.ReadAllLines(filePath);
                            var raw = content.Length > 0 ? content[0] : string.Empty;
                            var display = string.IsNullOrWhiteSpace(raw) ? "unbenutzt" : raw.Trim();
                            button.Content = display;

                            var blue = TryFindResource("ModernBlueButton") as Style;
                            var disabled = TryFindResource("DisabledButtonStyle") as Style;

                            if (!string.Equals(display, "unbenutzt", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(display, "leer", StringComparison.OrdinalIgnoreCase) &&
                                blue != null)
                            {
                                button.Style = blue;
                            }
                            else if (disabled != null)
                            {
                                button.Style = disabled;
                            }
                        }
                    }
                    catch { }

                    wrap.Children.Add(button);
                }

                stack.Children.Add(wrap);
                border.Child = stack;

                CardStackPanel.Children.Add(border);
            }
        }

        public void vResetFiles(object sender, RoutedEventArgs e)
        {
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines.vdata");
            string path2 = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves");

            string[] daten = { "unbenutzt", "unbenutzt", "unbenutzt", "unbenutzt" };


            YesNoMessage yesNoMessage = new YesNoMessage("Reset1");
            WindowPositionHelper.CenterToOwner(yesNoMessage, this);
            yesNoMessage.ShowDialog();

            if (yesNoMessage.DialogResult == true)
            {
                YesNoMessage yesNoMessage2 = new YesNoMessage("Reset2");
                WindowPositionHelper.CenterToOwner(yesNoMessage2, this);
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
                    WindowPositionHelper.CenterToOwner(yesNoMessage3, this);
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

            vClearUI();

            vLoadHeadlines();
            vBuildHeadlineCards();
            vLoadButtons();

            Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void vClearUI()
        {
            // Headlines and buttons reset for dynamic UI
            foreach (var child in CardStackPanel.Children)
            {
                if (child is Border b && b.Child is StackPanel sp)
                {
                    // First child is TextBlock headline
                    if (sp.Children.Count > 0 && sp.Children[0] is TextBlock tb)
                    {
                        tb.Text = string.Empty;
                    }

                    // Second child is WrapPanel with buttons
                    if (sp.Children.Count > 1 && sp.Children[1] is WrapPanel wp)
                    {
                        foreach (var btnObj in wp.Children)
                        {
                            if (btnObj is Button btn)
                            {
                                btn.Content = "unbenutzt";
                                var disabledStyle = TryFindResource("DisabledButtonStyle") as Style;
                                if (disabledStyle != null)
                                    btn.Style = disabledStyle;
                            }
                        }
                    }
                }
            }
        }

        public void vChangeButtons(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            int iSender = int.Parse(btn.Tag.ToString());

            YesNoMessage yesNoMessage = new YesNoMessage("ButtonChange");
            WindowPositionHelper.CenterToOwner(yesNoMessage, this);
            yesNoMessage.ShowDialog();

            if (yesNoMessage.DialogResult == true)
            {
                EditorWindow buttonChange = new EditorWindow(iSender);
                WindowPositionHelper.CenterToOwner(buttonChange, this);
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

                // If not found by FindName (dynamic buttons), try to search in CardStackPanel
                if (btn == null)
                {
                    // name is like "11", "12" etc. Construct registered name
                    btn = this.FindName("Btn" + name) as Button;
                    if (btn == null)
                    {
                        // Search children
                        foreach (var card in CardStackPanel.Children)
                        {
                            if (card is Border b && b.Child is StackPanel sp && sp.Children.Count > 1 && sp.Children[1] is WrapPanel wp)
                            {
                                foreach (var child in wp.Children)
                                {
                                    if (child is Button cb && cb.Tag != null && cb.Tag.ToString() == name)
                                    {
                                        btn = cb;
                                        break;
                                    }
                                }
                            }
                            if (btn != null) break;
                        }
                    }
                }

                if (btn != null)
                {
                    // Load display and style from file (first line stores display)
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

                    // Optionally, if there are saved paths after the first line, they are kept in the file; loading of apps happens when clicking the button
                }
            }
        }

        public void vLoadApps(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            int iSender = int.Parse(btn.Tag.ToString());

            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves");

            if (!Directory.Exists(path))
            {
                OkMessage okMessage = new OkMessage("DirectoryNotFound", "");
                WindowPositionHelper.CenterToOwner(okMessage, this);
                okMessage.ShowDialog();
                return;
            }

            string filePath = IOPath.Combine(path, iSender + ".vdata");

            if (!File.Exists(filePath))
            {
                OkMessage okMessage = new OkMessage("FileNotFound", "");
                WindowPositionHelper.CenterToOwner(okMessage, this);
                okMessage.ShowDialog();
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
                catch (Exception)
                {
                    OkMessage okMessage = new OkMessage("AppNotFound", "");
                    WindowPositionHelper.CenterToOwner(okMessage, this);
                    okMessage.ShowDialog();
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

        protected override void OnClosed(EventArgs e)
        {
            Properties.Settings.Default.WindowLeft = this.Left;
            Properties.Settings.Default.WindowTop = this.Top;

            Properties.Settings.Default.Save();

            base.OnClosed(e);
        }
    }
}