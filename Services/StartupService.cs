using System;
using System.IO;
using Microsoft.Win32;

namespace XboxBatteryMonitor.Services;

/// <summary>
/// Gestiona la configuración de inicio automático con Windows en el Registro (HKCU\Run).
/// Incluye auto-reparación si el ejecutable es movido a otra carpeta o unidad.
/// </summary>
public static class StartupService
{
    private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "X-Charge";
    private const string LegacyAppName = "XboxBatteryMonitor";

    public static bool IsStartupEnabled()
    {
        try
        {
            using RegistryKey? readKey = Registry.CurrentUser.OpenSubKey(RunRegistryKey, writable: false);
            if (readKey != null)
            {
                object? val = readKey.GetValue(AppName) ?? readKey.GetValue(LegacyAppName);
                return val is string;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static void SetStartup(bool enable)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, writable: true);
            if (key == null) return;

            // Limpieza de clave heredada si existiera
            key.DeleteValue(LegacyAppName, throwOnMissingValue: false);

            if (!enable)
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
                return;
            }

            string exePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "X-Charge.exe");
            key.SetValue(AppName, $"\"{exePath}\"");
        }
        catch
        {
            // Silencioso ante falta de permisos de registro
        }
    }
}
