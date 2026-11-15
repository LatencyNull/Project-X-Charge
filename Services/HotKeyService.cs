using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace XboxBatteryMonitor.Services;

/// <summary>
/// Registra y administra atajos globales de teclado en Windows (Win32 RegisterHotKey).
/// Permite alternar la visibilidad de la ventana desde cualquier juego a pantalla completa.
/// </summary>
public sealed class HotKeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_NOREPEAT = 0x4000;
    private const uint VK_X = 0x58; // Tecla 'X'

    private const int HotKeyId = 9001;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);

    private readonly Window _window;
    private HwndSource? _source;
    private bool _isRegistered;
    private Action? _onHotKeyPressed;

    public HotKeyService(Window window)
    {
        _window = window;
    }

    public bool Register(Action onHotKeyPressed)
    {
        if (_isRegistered) return true;

        _onHotKeyPressed = onHotKeyPressed;
        nint handle = new WindowInteropHelper(_window).EnsureHandle();
        _source = HwndSource.FromHwnd(handle);
        _source?.AddHook(HwndHook);

        _isRegistered = RegisterHotKey(handle, HotKeyId, MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT, VK_X);
        if (!_isRegistered)
        {
            _source?.RemoveHook(HwndHook);
            _source = null;
        }
        return _isRegistered;
    }

    public void Unregister()
    {
        if (_isRegistered)
        {
            nint handle = new WindowInteropHelper(_window).Handle;
            if (handle != IntPtr.Zero)
            {
                UnregisterHotKey(handle, HotKeyId);
            }
            _isRegistered = false;
        }

        if (_source != null)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
        }
    }

    private nint HwndHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HotKeyId)
        {
            _onHotKeyPressed?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
    }
}
