using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Shapes;
using vLauncher.Helpers;

namespace vLauncher
{
    public partial class ExportWindow : Window
    {
        public ExportWindow()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "ZIP files (*.zip)|*.zip";
            dlg.DefaultExt = "zip";
            dlg.FileName = "vLauncher_saves.zip";
            if (dlg.ShowDialog() == true)
            {
                TxtZipPath.Text = dlg.FileName;
                BtnExport.IsEnabled = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var saves = System.IO.Path.Combine(appData, "vLauncher", "Saves");

                if (!Directory.Exists(saves))
                {
                    MessageBox.Show("Saves-Ordner nicht gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    OkMessage okMessage = new OkMessage("ExportSourceDirectoryNotFound", "");
                    WindowPositionHelper.CenterToOwner(okMessage, this);
                    okMessage.ShowDialog();
                    return;
                }

                // fall back to using PowerShell Compress-Archive to create the ZIP (available on modern Windows)
                if (File.Exists(TxtZipPath.Text)) File.Delete(TxtZipPath.Text);
                var psi = new ProcessStartInfo();
                psi.FileName = "powershell.exe";
                psi.Arguments = $"-NoProfile -Command \"Compress-Archive -Path '{saves}\\*' -DestinationPath '{TxtZipPath.Text}' -Force\"";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                var proc = Process.Start(psi);
                proc.WaitForExit();
                if (proc.ExitCode != 0 || !File.Exists(TxtZipPath.Text))
                {
                    throw new Exception("Compress-Archive failed or zip not created.");
                }

                OkMessage okMessage2 = new OkMessage("ExportSuccess", "");
                WindowPositionHelper.CenterToOwner(okMessage2, this);
                okMessage2.ShowDialog();
            }
            catch (Exception ex)
            {
                OkMessage okMessage3 = new OkMessage("ExportError", ex.Message);
                WindowPositionHelper.CenterToOwner(okMessage3, this);
                okMessage3.ShowDialog();
            }
        }
    }
}
