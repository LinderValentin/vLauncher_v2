using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using vLauncher.Helpers;

namespace vLauncher
{
    public partial class ImportWindow : Window
    {
        public ImportWindow()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "ZIP files (*.zip)|*.zip";
            if (dlg.ShowDialog() == true)
            {
                TxtZipPath.Text = dlg.FileName;
                BtnImport.IsEnabled = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            string ordner = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vLauncher", "Saves");

            string[] dateien = Directory.GetFiles(ordner, "*.vdata");

            foreach (string datei in dateien) {
                File.Delete(datei);
            }


            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var target = System.IO.Path.Combine(appData, "vLauncher", "Saves");

                if (!Directory.Exists(target)) Directory.CreateDirectory(target);

                // extract using PowerShell Expand-Archive to avoid extra references
                var psi = new ProcessStartInfo();
                psi.FileName = "powershell.exe";
                psi.Arguments = $"-NoProfile -Command \"Expand-Archive -Path '{TxtZipPath.Text}' -DestinationPath '{target}' -Force\"";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                var proc = Process.Start(psi);
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    throw new Exception("Expand-Archive failed.");
                }

                OkMessage okMessage = new OkMessage("ImportSuccess", "");
                WindowPositionHelper.CenterToOwner(okMessage, this);
                okMessage.ShowDialog();
            }
            catch (Exception ex)
            {
                OkMessage okMessage2 = new OkMessage("ImportError", ex.Message);
                WindowPositionHelper.CenterToOwner(okMessage2, this);
                okMessage2.ShowDialog();
            }
        }
    }
}
