using System;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace XboxBatteryMonitor
{
    public partial class App : Application
    {
        private Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            FieldInfo? field = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(null, false);
            }

            _mutex = new Mutex(initiallyOwned: true, "XChargeAppMutex_8a7d6f5e", out var createdNew);
            if (!createdNew)
            {
                MessageBox.Show("X-Charge ya se encuentra en ejecución. Revisa la bandeja del sistema (al lado del reloj).", "X-Charge", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(args.Exception.ToString(), "Error - X-Charge", MessageBoxButton.OK, MessageBoxImage.Hand);
                args.Handled = true;
            };

            MainWindow mainWindow = new MainWindow();
            if (!AppSettings.Current.StartInTray)
            {
                mainWindow.Show();
                return;
            }
            mainWindow.Opacity = 0.0;
            mainWindow.Show();
            mainWindow.Hide();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _mutex?.ReleaseMutex();
            }
            catch
            {
            }
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
