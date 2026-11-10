# 🎮 X-Charge (v1.0)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)](https://microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-8.0%20WPF-512BD4.svg)](https://dotnet.microsoft.com/)

**X-Charge** is a premium, lightweight floating battery monitor and real-time telemetry HUD for Windows 10 & 11, specifically designed for **Xbox controllers** and any **XInput-compatible gamepad** on PC.

[English](README.md) • [Español](README.es.md)

---

## ✨ Key Features

- **Pure Native XInput Engine (100% Safe & Stable)**:
  - Built purely on Win32 XInput APIs (`XInputGetState` & `XInputGetBatteryInformation`).
  - **Zero RF Interference**: completely eliminates the notorious wireless controller power-off bug during reconnection with the official Xbox Wireless Adapter for Windows.
  - Instant connect & disconnect detection without stale COM or WinRT zombie references in memory.
  - Multi-controller support: monitors up to 4 gamepads simultaneously with independent slot identification.
- **Power & Charging Detection**:
  - Instant distinction between wired USB power (`UsbCharged`) and wireless battery status (`Alkaline` / `NiMH` / `Play & Charge Kit`).
  - 100% battery bug fix: percentages never disappear, and UI progress bars never overflow.
  - Visual and audio low-battery notifications with configurable thresholds (10%, 15%, or 20%).
- **Display Modes**:
  - **Normal Mode**: Comprehensive dashboard displaying controller name, battery percentage, charging state, and status bar.
  - **Compact Mode**: Minimalist circular floating widget with real-time percentage, freely draggable and resizable.
  - **Interactive Mouse Wheel Opacity**: Hover over Compact Mode and scroll the mouse wheel to adjust transparency in real time (from 15% ultra-translucent up to 100% sharp/opaque). Features an animated HUD gauge and remembers your preference across reboots.
- **OLED Anti-Burn-In Protection**:
  - **Smooth Circular Orbit**: Imperceptible periodic harmonic displacement ($\Delta X = 8\cos\theta, \Delta Y = 8\sin\theta$) with gentle easing and micro text shifts every 20s to safeguard OLED/AMOLED screens without altering your chosen opacity.
- **System Tray Integration**:
  - Dynamic pixel-art tray icon showing real-time battery fill and external power symbol.
  - Context menu: theme switching (Dark, Light, Transparent), USB/battery audio alerts, Windows startup, and customizable alert thresholds.
- **Multi-Language Support**: 7 built-in languages (English, Spanish, French, German, Portuguese, Chinese, and Russian).
- **Data Resilience**:
  - Reliable settings storage in `%AppData%\X-Charge\config.json` with automated migration and corrupted config self-healing.
- **Resource Hygiene**:
  - Rigorous cleanup of static system events (`UserPreferenceChanged`) and single-instance Mutex lifecycle management.

---

## 🎮 Controller Compatibility

- **Native XInput Controllers (Plug & Play, No Extra Software)**:
  - **Official Xbox**: Xbox Series X/S, Xbox One, Xbox One S/X, Xbox Elite Series 1 & 2, Xbox 360 (via Official USB Wireless Adapter, USB cable, or Bluetooth).
  - **Third-Party PC Controllers**: 8BitDo (Ultimate, Pro 2, SN30 Pro in X mode), GameSir (G7, Nova, Kaleid, T4 Pro), Logitech (F710, F310 in "X" mode), Razer, PowerA, PDP, Gulikit (KingKong 2/3), Flydigi, and any PC gamepad with an XInput mode switch.
- **PlayStation (DualSense / PS5, DualShock 4 / PS4) & Nintendo Switch Pro**:
  - Windows does not natively map PlayStation or Switch controllers to XInput (they register as standard DirectInput/HID devices).
  - **However, if using mapping tools such as DS4Windows, Steam Input, DualSenseX, or reWASD**, these utilities emulate a virtual Xbox / XInput controller in Windows. In this setup, **X-Charge seamlessly detects them** through the virtual XInput slot.

---

## 🛠️ Tech Stack

- **Framework**: .NET 8.0 SDK with WPF (`net8.0-windows10.0.19041.0`)
- **Minimum OS**: Windows 10 build 17763 (10.0.17763.0) or higher (Windows 11 fully supported)
- **Hardware APIs**: `xinput1_4.dll` (with seamless fallback to `xinput9_1_0.dll`)
- **NuGet Packages**:
  - `Hardcodet.NotifyIcon.Wpf` (v1.1.0)
  - `Microsoft.Toolkit.Uwp.Notifications` (v7.1.3)

---

## 🚀 Building & Publishing

An automated PowerShell build script `build.ps1` is included in the root directory:

```powershell
# 1. Framework-Dependent Single-File build (~24 MB)
.\build.ps1 -Clean

# 2. Self-Contained standalone executable (~74 MB, no .NET installation required on target machine)
.\build.ps1 -SelfContained -Clean

# 3. Build and launch immediately
.\build.ps1 -Run
```

Single-file executables are automatically generated under `publish\X-Charge.exe`.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
