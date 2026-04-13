using System;
using System.Threading;
using System.Windows;

namespace vLauncher
{
    public partial class App : Application
    {
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "vLauncher_SingleInstance";

            bool createdNew;
            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("vLauncher läuft bereits!");
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}