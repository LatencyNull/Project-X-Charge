using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace XboxBatteryMonitor
{
    public partial class App : Application
    {
        private const string EventName = "XChargeShowAppEvent_8a7d6f5e";
        private const string MutexName = "XChargeAppMutex_8a7d6f5e";

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        private Mutex? _mutex;
        private EventWaitHandle? _showAppEvent;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                SetCurrentProcessExplicitAppUserModelID("XCharge.XboxBatteryMonitor");
            }
            catch { }

            FieldInfo? field = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(null, false);
            }

            _mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
            if (!createdNew)
            {
                try
                {
                    using EventWaitHandle showEvent = EventWaitHandle.OpenExisting(EventName);
                    showEvent.Set();
                }
                catch { }

                Application.Current.Shutdown();
                return;
            }

            try
            {
                _showAppEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);
                ThreadPool.RegisterWaitForSingleObject(_showAppEvent, (state, timedOut) =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (MainWindow is MainWindow mw)
                        {
                            mw.ShowWindow();
                        }
                    });
                }, null, -1, false);
            }
            catch { }

            base.OnStartup(e);

            this.DispatcherUnhandledException += (s, args) =>
            {
                try
                {
                    string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "X-Charge", "error.log");
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                    File.AppendAllText(logPath, $"{DateTime.Now}: {args.Exception}{Environment.NewLine}");
                }
                catch { }
                args.Handled = true;
                Shutdown();
            };

            MainWindow mainWindow = new MainWindow();
            MainWindow = mainWindow;

            bool shouldStartHidden = AppSettings.Current.StartInTray || AppSettings.Current.HideUntilLowBattery;
            if (shouldStartHidden)
            {
                // Inicializa el HWND y registros Win32 de forma 100% silenciosa sin mostrar la ventana
                new WindowInteropHelper(mainWindow).EnsureHandle();
                return;
            }

            mainWindow.Show();
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            try
            {
                if (MainWindow is MainWindow mw)
                {
                    mw.SaveCurrentState();
                }
            }
            catch { }
            base.OnSessionEnding(e);
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
            _showAppEvent?.Dispose();
            base.OnExit(e);
        }
    }
}
