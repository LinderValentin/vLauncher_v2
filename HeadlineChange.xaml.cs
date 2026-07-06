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
    public partial class HeadlineChange : Window
    {
        private TextBox _lastFocusedHeadline = null;

        private readonly string _debugLogPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines_debug.log");

        public HeadlineChange()
        {
            InitializeComponent();
            // attach focus handlers for default textboxes
            TxtHeadline1.GotFocus += Headline_GotFocus;
            TxtHeadline2.GotFocus += Headline_GotFocus;
            TxtHeadline3.GotFocus += Headline_GotFocus;
            TxtHeadline4.GotFocus += Headline_GotFocus;

            vLoadHeadlines();
            vBuildDynamicUI();
        }

        private void Headline_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                _lastFocusedHeadline = sender as TextBox;
            }
            catch { _lastFocusedHeadline = null; }
        }

        private void LogState(string tag, TextBox focused)
        {
            try
            {
                var lines = new List<string>();
                lines.Add($"[{DateTime.Now:O}] {tag}");
                lines.Add($"Focused: {(focused != null ? focused.Name + "='" + focused.Text + "'" : "<none>")}");
                lines.Add("PanelChildren:");
                int i = 0;
                foreach (var child in HeadlinesPanel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        lines.Add($" {i++}: TextBlock '{tb.Text}'");
                    }
                    else if (child is TextBox t)
                    {
                        lines.Add($" {i++}: TextBox {t.Name} '{t.Text}'");
                    }
                    else if (child is Button b)
                    {
                        lines.Add($" {i++}: Button '{b.Content}'");
                    }
                    else
                    {
                        lines.Add($" {i++}: {child?.GetType().Name}");
                    }
                }
                lines.Add("----");

                File.AppendAllLines(_debugLogPath, lines);
            }
            catch { }
        }

        // Build dynamic UI controls for managing headlines: add + delete
        private void vBuildDynamicUI()
        {
            // Clear any existing dynamic controls beyond the four default ones
            // Remove any previously registered dynamic controls
            var toRemove = new List<UIElement>();
            foreach (var child in HeadlinesPanel.Children)
            {
                if (child is TextBox tb && tb.Name.StartsWith("TxtHeadline") && tb.Name != "TxtHeadline1" && tb.Name != "TxtHeadline2" && tb.Name != "TxtHeadline3" && tb.Name != "TxtHeadline4")
                {
                    toRemove.Add(tb);
                }
                if (child is TextBlock t && t.Text.StartsWith("Headline ") && t.Text != "Headline 1" && t.Text != "Headline 2" && t.Text != "Headline 3" && t.Text != "Headline 4")
                {
                    toRemove.Add(t);
                }
            }
            foreach (var r in toRemove) HeadlinesPanel.Children.Remove(r);

            // Add controls for existing headlines beyond 4 (read from file)
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines.vdata");
            var list = File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string>();
            while (list.Count < 4) list.Add("unbenutzt");

            for (int i = 4; i < list.Count; i++)
            {
                var lbl = new TextBlock() { Text = $"Headline {i + 1}", Foreground = Brushes.White, Margin = new Thickness(0,10,0,0) };
                var tb = new TextBox() { Name = $"TxtHeadline{i + 1}", Text = list[i] };
                try { this.RegisterName(tb.Name, tb); } catch { }
                tb.GotFocus += Headline_GotFocus;
                HeadlinesPanel.Children.Add(lbl);
                HeadlinesPanel.Children.Add(tb);
            }

            // Add buttons for adding/removing headlines
            var addBtn = new Button() { Content = "+ Überschrift hinzufügen", Style = (Style)FindResource("ModernBlueButton") };
            addBtn.Click += (s, e) =>
            {
                int newIndex = 1;
                // find highest existing index
                foreach (var child in HeadlinesPanel.Children)
                {
                    if (child is TextBox tb && tb.Name.StartsWith("TxtHeadline"))
                    {
                        if (int.TryParse(tb.Name.Replace("TxtHeadline", ""), out int idx))
                            newIndex = Math.Max(newIndex, idx + 1);
                    }
                }
                var lbl = new TextBlock() { Text = $"Headline {newIndex}", Foreground = Brushes.White, Margin = new Thickness(0,10,0,0) };
                var tbx = new TextBox() { Name = $"TxtHeadline{newIndex}", Text = "unbenutzt" };
                try { this.RegisterName(tbx.Name, tbx); } catch { }
                tbx.GotFocus -= Headline_GotFocus; // Ensure handler is removed first
                tbx.GotFocus += Headline_GotFocus; // Then add the handler
                HeadlinesPanel.Children.Add(lbl);
                HeadlinesPanel.Children.Add(tbx);
                // focus and scroll into view
                try
                {
                    tbx.Focus();
                    tbx.BringIntoView();
                }
                catch { }
            };

            var delBtn = new Button() { Content = "- Überschrift löschen", Style = (Style)FindResource("ModernBlueButton") };
            delBtn.Click += (s, e) =>
            {
                // Log current state before deletion
                LogState("BeforeDelete", _lastFocusedHeadline);

                var target = _lastFocusedHeadline;
                if (target == null || !target.Name.StartsWith("TxtHeadline"))
                {
                    LogState("DeleteCancelled_NoFocusedHeadline", null);
                    return;
                }

                // Mark headline for deletion by setting text to "--gelöscht--"
                // Actual deletion happens in vChangeHeadlinesDatei
                try
                {
                    target.Text = "--gelöscht--";
                    LogState("MarkedForDeletion", target);
                }
                catch { }

                _lastFocusedHeadline = null;
            };

            var swapBtn = new Button() { Content = "⇄ Tauschen", Style = (Style)FindResource("ModernBlueButton") };
            swapBtn.Click += (s, e) =>
            {
                var target = _lastFocusedHeadline;
                if (target == null || !target.Name.StartsWith("TxtHeadline"))
                {
                    MessageBox.Show("Bitte wählen Sie eine Überschrift aus.", "Auswahl erforderlich", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Extract headline index
                string headlineIndexStr = target.Name.Replace("TxtHeadline", "");
                if (!int.TryParse(headlineIndexStr, out int headlineIndex))
                {
                    return;
                }

                // Collect all headlines
                var allHeadlines = new List<string>();
                int index = 1;
                while (true)
                {
                    var tb = this.FindName($"TxtHeadline{index}") as TextBox;
                    if (tb == null) break;
                    allHeadlines.Add(tb.Text);
                    index++;
                }

                // Show selection dialog - pass this window reference
                HeadlineSwapSelect swapDialog = new HeadlineSwapSelect(headlineIndex - 1, allHeadlines, this);
                WindowPositionHelper.CenterToOwner(swapDialog, this);
                swapDialog.ShowDialog();
            };

            // Remove any existing add/del buttons first
            // Remove existing add/del buttons from AddDelPanel
            for (int i = AddDelPanel.Children.Count - 1; i >= 0; i--)
            {
                if (AddDelPanel.Children[i] is Button b && (b.Content.ToString().Contains("Überschrift") || b.Content.ToString().Contains("Tauschen")))
                    AddDelPanel.Children.RemoveAt(i);
            }

            // Add add/del buttons into AddDelPanel
            AddDelPanel.Children.Add(addBtn);
            AddDelPanel.Children.Add(delBtn);
            AddDelPanel.Children.Add(swapBtn);
        }

        public void vBtnAbbrechen(object sender, RoutedEventArgs e)
        {
            OkMessage okMessage = new OkMessage("ButtonClose", "");
            WindowPositionHelper.CenterToOwner(okMessage, this);
            okMessage.ShowDialog();
            this.Close();
        }

        public void vLoadHeadlines()
        {
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines.vdata");

            List<string> strList = new List<string>(File.ReadAllLines(path));

            // Ensure at least 4 entries for compatibility
            while (strList.Count < 4)
                strList.Add("unbenutzt");

            // Populate existing textboxes
            TxtHeadline1.Text = strList[0];
            TxtHeadline2.Text = strList[1];
            TxtHeadline3.Text = strList[2];
            TxtHeadline4.Text = strList[3];

            // ensure handlers attached in case vLoadHeadlines called before constructor finished
            TxtHeadline1.GotFocus -= Headline_GotFocus;
            TxtHeadline2.GotFocus -= Headline_GotFocus;
            TxtHeadline3.GotFocus -= Headline_GotFocus;
            TxtHeadline4.GotFocus -= Headline_GotFocus;
            TxtHeadline1.GotFocus += Headline_GotFocus;
            TxtHeadline2.GotFocus += Headline_GotFocus;
            TxtHeadline3.GotFocus += Headline_GotFocus;
            TxtHeadline4.GotFocus += Headline_GotFocus;

            // dynamic extra headline controls are created in vBuildDynamicUI
        }

        public void vChangeHeadlinesDatei(object sender, RoutedEventArgs e)
        {
            string path = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves", "headlines.vdata");

            // Collect all TextBoxes and their values
            var allHeadlines = new List<string>();
            var headlineIndices = new List<int>(); // corresponding indices (1-based)
            int index = 1;
            while (true)
            {
                var tb = this.FindName($"TxtHeadline{index}") as TextBox;
                if (tb == null) break;
                allHeadlines.Add(tb.Text);
                headlineIndices.Add(index);
                index++;
            }

            // Filter: remove entries marked with "--gelöscht--" and note their indices
            var deletedIndices = new List<int>();
            var finalHeadlines = new List<string>();
            for (int i = 0; i < allHeadlines.Count; i++)
            {
                if (allHeadlines[i] == "--gelöscht--")
                {
                    deletedIndices.Add(headlineIndices[i]);
                }
                else
                {
                    finalHeadlines.Add(allHeadlines[i]);
                }
            }

            // If all deleted, restore to default 4
            if (finalHeadlines.Count == 0)
            {
                finalHeadlines = new List<string> { "unbenutzt", "unbenutzt", "unbenutzt", "unbenutzt" };
            }

            // Delete associated .vdata files for deleted headline indices
            try
            {
                string savesPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves");
                foreach (int delIdx in deletedIndices)
                {
                    for (int b = 1; b <= 5; b++)
                    {
                        string tag = (delIdx * 10 + b).ToString();
                        string file = IOPath.Combine(savesPath, tag + ".vdata");
                        if (File.Exists(file))
                        {
                            try { File.Delete(file); } catch { }
                        }
                    }
                }
            }
            catch { }

            // Save filtered headlines to file
            File.WriteAllLines(path, finalHeadlines);

            this.DialogResult = true;
            this.Close();
        }

        private void SwapHeadlines(int firstIdx, int secondIdx)
        {
            // This method is no longer used - swap is done directly in HeadlineSwapSelect
            // Kept for compatibility if needed
        }

        public void RefreshHeadlinesUI()
        {
            // Rebuild the dynamic UI to show updated headlines
            vBuildDynamicUI();
        }

        private void TxtHeadline1_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        //for custom titlebar (not windows default)
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}