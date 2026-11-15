using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace XboxBatteryMonitor.Services;

/// <summary>
/// Encargado de la renderización dinámica de iconos en la bandeja del sistema (System Tray),
/// asegurando escalabilidad nítida para pantallas de alta densidad (Per-Monitor DPI) y
/// la liberación inmediata de los handles GDI de Win32 para evitar memory leaks.
/// </summary>
public static class TrayIconService
{
    private const int SM_CXSMICON = 49;
    private const int SM_CYSMICON = 50;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(nint handle);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    public static Icon GenerateBatteryIcon(double percent, bool isCharging, bool isConnected, int lowBatteryThreshold = 20)
    {
        int iconWidth = GetSystemMetrics(SM_CXSMICON);
        int iconHeight = GetSystemMetrics(SM_CYSMICON);

        if (iconWidth <= 0 || iconHeight <= 0)
        {
            iconWidth = 16;
            iconHeight = 16;
        }

        using Bitmap bitmap = new(iconWidth, iconHeight);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(Color.Transparent);

        Color outlineColor = Color.FromArgb(200, 200, 200);
        Color fillColor = (!isConnected || percent < 0.0)
            ? Color.FromArgb(100, 100, 100)
            : (isCharging
                ? Color.FromArgb(59, 130, 246)
                : ((percent <= lowBatteryThreshold)
                    ? Color.FromArgb(226, 75, 74)
                    : ((percent <= 50.0) ? Color.FromArgb(245, 166, 35) : Color.FromArgb(29, 158, 117))));

        float scaleX = iconWidth / 16f;
        float scaleY = iconHeight / 16f;

        // Cuerpo de la batería
        int bodyX = 0;
        int bodyY = (int)Math.Round(3 * scaleY);
        int bodyW = (int)Math.Round(13 * scaleX);
        int bodyH = (int)Math.Round(10 * scaleY);
        Rectangle bodyRect = new(bodyX, bodyY, bodyW, bodyH);

        using (Pen pen = new(outlineColor, Math.Max(1.0f, 1.2f * scaleX)))
        {
            graphics.DrawRectangle(pen, bodyRect);
        }

        // Terminal positivo de la batería
        int termX = bodyX + bodyW;
        int termY = (int)Math.Round(6 * scaleY);
        int termW = Math.Max(2, (int)Math.Round(2.5f * scaleX));
        int termH = Math.Max(2, (int)Math.Round(4 * scaleY));
        using (SolidBrush terminalBrush = new(outlineColor))
        {
            graphics.FillRectangle(terminalBrush, termX, termY, termW, termH);
        }

        // Relleno de carga proporcional
        if (isConnected && percent >= 0.0)
        {
            int maxFillW = Math.Max(1, bodyW - (int)Math.Round(2 * scaleX));
            int fillWidth = (int)Math.Round((double)maxFillW * Math.Clamp(percent, 0.0, 100.0) / 100.0);
            if (fillWidth > 0)
            {
                int fillX = bodyX + (int)Math.Round(1 * scaleX);
                int fillY = bodyY + (int)Math.Round(1 * scaleY);
                int fillH = Math.Max(1, bodyH - (int)Math.Round(2 * scaleY));
                Rectangle fillRect = new(fillX, fillY, fillWidth, fillH);
                using SolidBrush fillBrush = new(fillColor);
                graphics.FillRectangle(fillBrush, fillRect);
            }
        }

        // Rayo de carga externa si aplica
        if (isCharging && isConnected)
        {
            using Pen boltPen = new(Color.White, Math.Max(1.2f, 1.5f * scaleX));
            graphics.DrawLine(boltPen, 8 * scaleX, 3 * scaleY, 5 * scaleX, 8 * scaleY);
            graphics.DrawLine(boltPen, 5 * scaleX, 8 * scaleY, 9 * scaleX, 8 * scaleY);
            graphics.DrawLine(boltPen, 9 * scaleX, 8 * scaleY, 6 * scaleX, 13 * scaleY);
        }

        nint hIcon = bitmap.GetHicon();
        Icon result;
        try
        {
            using Icon tempIcon = Icon.FromHandle(hIcon);
            result = (Icon)tempIcon.Clone();
        }
        finally
        {
            DestroyIcon(hIcon);
        }

        return result;
    }
}
