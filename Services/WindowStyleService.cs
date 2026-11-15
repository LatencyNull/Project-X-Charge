using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace XboxBatteryMonitor.Services;

/// <summary>
/// Controla estilos avanzados de ventana Win32 (como WS_EX_TRANSPARENT para modo fantasma / click-through)
/// y resolución precisa del área de trabajo en entornos multimonitor de alta densidad.
/// </summary>
public static class WindowStyleService
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const uint MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern int GetWindowLong32(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SwpFlags = SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE;

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint hwnd, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

    private static nint GetWindowLong(nint hWnd, int nIndex)
    {
        return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
    }

    private static nint SetWindowLong(nint hWnd, int nIndex, nint dwNewLong)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
            : SetWindowLong32(hWnd, nIndex, (int)dwNewLong);
    }

    /// <summary>
    /// Habilita o deshabilita el paso directo de eventos de ratón a través de la ventana.
    /// </summary>
    public static void SetClickThrough(Window window, bool enable)
    {
        try
        {
            nint handle = new WindowInteropHelper(window).EnsureHandle();
            if (handle == IntPtr.Zero) return;

            nint currentStyle = GetWindowLong(handle, GWL_EXSTYLE);
            nint newStyle;

            if (enable)
            {
                newStyle = currentStyle | WS_EX_TRANSPARENT;
            }
            else
            {
                newStyle = currentStyle & ~WS_EX_TRANSPARENT;
            }

            SetWindowLong(handle, GWL_EXSTYLE, newStyle);
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SwpFlags);
        }
        catch
        {
            // Silencioso si el handle de la ventana aún no está listo
        }
    }

    /// <summary>
    /// Obtiene el área de trabajo real (Work Area) del monitor donde reside la ventana actualmente,
    /// admitiendo configuraciones multimonitor y escalado DPI sin restringirse a la pantalla principal.
    /// </summary>
    public static Rect GetCurrentMonitorWorkArea(Window window)
    {
        try
        {
            nint handle = new WindowInteropHelper(window).EnsureHandle();
            if (handle != IntPtr.Zero)
            {
                nint hMonitor = MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST);
                if (hMonitor != IntPtr.Zero)
                {
                    MONITORINFO mi = new() { cbSize = Marshal.SizeOf<MONITORINFO>() };
                    if (GetMonitorInfo(hMonitor, ref mi))
                    {
                        DpiScale dpi = VisualTreeHelper.GetDpi(window);
                        double left = mi.rcWork.Left / dpi.DpiScaleX;
                        double top = mi.rcWork.Top / dpi.DpiScaleY;
                        double right = mi.rcWork.Right / dpi.DpiScaleX;
                        double bottom = mi.rcWork.Bottom / dpi.DpiScaleY;
                        return new Rect(left, top, Math.Max(100.0, right - left), Math.Max(100.0, bottom - top));
                    }
                }
            }
        }
        catch { }

        return SystemParameters.WorkArea;
    }
}
