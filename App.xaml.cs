using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace XboxBatteryMonitor
{
    public partial class App : Application
    {
        private const nint HWND_BROADCAST = 0xFFFF;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        private Mutex? _mutex;

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

            _mutex = new Mutex(initiallyOwned: true, "XChargeAppMutex_8a7d6f5e", out var createdNew);
            if (!createdNew)
            {
                // Notificar a la instancia existente que se muestre en pantalla y ganar foco
                uint showMsg = RegisterWindowMessage("XCHARGE_SHOW_APP");
                PostMessage(HWND_BROADCAST, showMsg, IntPtr.Zero, IntPtr.Zero);
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
            base.OnExit(e);
        }
    }
}
