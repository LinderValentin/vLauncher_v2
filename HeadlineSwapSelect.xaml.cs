using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using vLauncher.Helpers;
using System.IO;
using IOPath = System.IO.Path;

namespace vLauncher
{
    /// <summary>
    /// Interaction logic for HeadlineSwapSelect.xaml
    /// </summary>
    public partial class HeadlineSwapSelect : Window
    {
        public int FirstHeadlineIndex { get; set; } = -1;
        public int SecondHeadlineIndex { get; set; } = -1;
        public List<string> AllHeadlines { get; set; } = new List<string>();
        private HeadlineChange _parentWindow;

        public HeadlineSwapSelect(int selectedIndex, List<string> headlines, HeadlineChange parentWindow)
        {
            InitializeComponent();
            FirstHeadlineIndex = selectedIndex;
            AllHeadlines = headlines;
            _parentWindow = parentWindow;

            vLoadHeadlines();
        }

        private void vLoadHeadlines()
        {
            TxtSelectedHeadline.Text = $"Ausgewählte Überschrift: {AllHeadlines[FirstHeadlineIndex]}";

            HeadlineListBox.Items.Clear();

            for (int i = 0; i < AllHeadlines.Count; i++)
            {
                if (i != FirstHeadlineIndex)
                {
                    HeadlineListBox.Items.Add(new HeadlineItem 
                    { 
                        Index = i, 
                        Name = AllHeadlines[i] 
                    });
                }
            }
        }

        public void BtnSwap_Click(object sender, RoutedEventArgs e)
        {
            if (HeadlineListBox.SelectedItem == null)
            {
                MessageBox.Show("Bitte wählen Sie eine Überschrift zum Tauschen aus.", "Auswahl erforderlich", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selected = HeadlineListBox.SelectedItem as HeadlineItem;
            SecondHeadlineIndex = selected.Index;

            try
            {
                // Perform swap directly
                PerformSwap();

                // Notify parent to refresh UI
                _parentWindow.RefreshHeadlinesUI();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Tauschen: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PerformSwap()
        {
            // firstIdx and secondIdx are 0-based
            int firstHeadlineNum = FirstHeadlineIndex + 1;
            int secondHeadlineNum = SecondHeadlineIndex + 1;

            // Swap headline texts in parent
            var firstTb = _parentWindow.FindName($"TxtHeadline{firstHeadlineNum}") as TextBox;
            var secondTb = _parentWindow.FindName($"TxtHeadline{secondHeadlineNum}") as TextBox;

            if (firstTb == null || secondTb == null)
            {
                throw new Exception("Headline nicht gefunden.");
            }

            string temp = firstTb.Text;
            firstTb.Text = secondTb.Text;
            secondTb.Text = temp;

            // Swap all associated .vdata files
            try
            {
                string savesPath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves");

                for (int b = 1; b <= 5; b++)
                {
                    string firstTag = (firstHeadlineNum * 10 + b).ToString();
                    string secondTag = (secondHeadlineNum * 10 + b).ToString();

                    string firstFile = IOPath.Combine(savesPath, firstTag + ".vdata");
                    string secondFile = IOPath.Combine(savesPath, secondTag + ".vdata");

                    // Create temporary file path
                    string tempFile = IOPath.Combine(savesPath, "swap_temp_" + firstTag + ".vdata");

                    // Swap by using temp file
                    bool firstExists = File.Exists(firstFile);
                    bool secondExists = File.Exists(secondFile);

                    if (firstExists)
                    {
                        if (File.Exists(tempFile))
                            File.Delete(tempFile);
                        File.Move(firstFile, tempFile);
                    }

                    if (secondExists)
                    {
                        File.Move(secondFile, firstFile);
                    }

                    if (firstExists)
                    {
                        File.Move(tempFile, secondFile);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Tauschen der Dateien: " + ex.Message);
            }
        }

        public void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

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

    public class HeadlineItem
    {
        public int Index { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"Headline {Index + 1}: {Name}";
        }
    }
}

