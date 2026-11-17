using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace XboxBatteryMonitor.Services;

/// <summary>
/// Gestiona la reproducción nativa y ligera de sonidos del sistema (winmm.dll)
/// para eventos de conexión USB y alarmas de batería baja, sin dependencias COM.
/// </summary>
public static class SoundService
{
    private const uint SND_ASYNC = 0x0001;
    private const uint SND_NODEFAULT = 0x0002;
    private const uint SND_ALIAS = 0x00010000;
    private const uint SND_APPLICATION = 0x00200000;
    private const uint SoundFlags = SND_ASYNC | SND_NODEFAULT | SND_ALIAS | SND_APPLICATION;

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool PlaySound(string pszSound, nint hmod, uint fdwSound);

    public static void PlayConnectSound()
    {
        if (!AppSettings.Current.SoundUsbEnabled) return;
        try
        {
            PlaySound("DeviceConnect", IntPtr.Zero, SoundFlags);
        }
        catch { }
    }

    public static void PlayDisconnectSound()
    {
        if (!AppSettings.Current.SoundUsbEnabled) return;
        try
        {
            PlaySound("DeviceDisconnect", IntPtr.Zero, SoundFlags);
        }
        catch { }
    }

    public static async Task PlayLowBatteryAlertAsync(CancellationToken cancellationToken = default)
    {
        if (!AppSettings.Current.SoundBatteryEnabled) return;

        try
        {
            for (int round = 0; round < 2; round++)
            {
                if (cancellationToken.IsCancellationRequested || !AppSettings.Current.SoundBatteryEnabled)
                    return;

                PlaySound("SystemExclamation", IntPtr.Zero, SoundFlags);

                await Task.Delay(1500, cancellationToken);
            }
        }
        catch
        {
            // Silencioso si el audio del sistema está ocupado o desactivado
        }
    }

    public static void StopAlert()
    {
        try
        {
            // SND_PURGE detiene cualquier sonido asíncrono en curso en winmm
            PlaySound(string.Empty, IntPtr.Zero, 0x0040); // 0x0040 = SND_PURGE
        }
        catch { }
    }

    public static void Dispose()
    {
        StopAlert();
    }
}
