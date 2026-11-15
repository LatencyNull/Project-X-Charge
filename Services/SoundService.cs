using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace XboxBatteryMonitor.Services;

/// <summary>
/// Gestiona la reproducción de sonidos del sistema para eventos de conexión USB y alarmas de batería baja.
/// </summary>
public static class SoundService
{
    private const uint SND_ASYNC = 0x0001;
    private const uint SND_NODEFAULT = 0x0002;
    private const uint SND_ALIAS = 0x00010000;
    private const uint SND_APPLICATION = 0x00200000;
    private const uint SoundFlags = SND_ASYNC | SND_NODEFAULT | SND_ALIAS | SND_APPLICATION;

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool PlaySound(string pszSound, nint hmod, uint fdwSound);

    private static MediaPlayer? _alertPlayer;

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
            string winMedia = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media", "Windows Notify System Generic.wav");
            if (!File.Exists(winMedia))
            {
                winMedia = @"C:\Windows\Media\Windows Notify System Generic.wav";
            }

            if (!File.Exists(winMedia)) return;

            _alertPlayer ??= new MediaPlayer();
            _alertPlayer.Open(new Uri(winMedia, UriKind.Absolute));

            for (int round = 0; round < 3; round++)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (cancellationToken.IsCancellationRequested || !AppSettings.Current.SoundBatteryEnabled)
                    {
                        _alertPlayer.Stop();
                        return;
                    }

                    _alertPlayer.Position = TimeSpan.Zero;
                    _alertPlayer.Volume = 0.05 + (i * 0.1);
                    _alertPlayer.Play();
                    await Task.Delay(500, cancellationToken);
                }

                if (round < 2)
                {
                    await Task.Delay(2000, cancellationToken);
                }
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
            _alertPlayer?.Stop();
        }
        catch { }
    }

    public static void Dispose()
    {
        try
        {
            _alertPlayer?.Close();
            _alertPlayer = null;
        }
        catch { }
    }
}
