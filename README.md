# 🎮 X-Charge (v1.0)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)](https://microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-8.0%20WPF-512BD4.svg)](https://dotnet.microsoft.com/)
[![CI/CD](https://github.com/LatencyNull/X-Charge/actions/workflows/build-release.yml/badge.svg)](https://github.com/LatencyNull/X-Charge/actions)

**X-Charge** is a premium, lightweight floating battery monitor and real-time telemetry HUD for Windows 10 & 11, specifically designed for **Xbox controllers** and any **XInput-compatible gamepad** on PC.

[English](README.md) • [Español](README.es.md)

---

## ✨ Key Features

- **Pure Native XInput Engine (100% Safe & Stable)**:
  - Built purely on Win32 XInput APIs (`xinput1_4.dll`).
  - **Zero RF Interference**: completely eliminates the notorious wireless controller power-off bug during reconnection with the official Xbox Wireless Adapter for Windows.
  - Multi-controller support: monitors up to 4 gamepads simultaneously with independent slot identification.
- **Display Modes & Gaming HUD**:
  - **Normal Mode**: Comprehensive dashboard displaying controller name, battery percentage, charging state, and status bar.
  - **Compact Mode**: Minimalist circular floating widget with real-time percentage, freely draggable and resizable.
  - **Interactive Mouse Wheel Opacity**: Hover over Compact Mode and scroll the mouse wheel to adjust transparency in real time (from 15% ultra-translucent up to 100% sharp/opaque).
  - **Ghost Mode (Click-Through)**: Makes the compact floating HUD click-through (`WS_EX_TRANSPARENT`), allowing game clicks and cursor interactions to pass right through the widget without disruption.
  - **Global Hotkey (`Ctrl + Shift + X`)**: Toggle HUD visibility instantly from inside any full-screen game without alt-tabbing.
- **OLED Anti-Burn-In Protection**:
  - **Smooth Circular Orbit**: Imperceptible periodic harmonic displacement ($\Delta X = 8\cos\theta, \Delta Y = 8\sin\theta$) with gentle easing and micro text shifts every 20s to safeguard OLED/AMOLED screens.
- **System Tray Integration**:
  - Dynamic pixel-art tray icon showing real-time battery fill and external power state (with leak-free Win32 GDI management).
  - Tray Context Menu:
    - **Language Switcher**: Dynamic runtime selection across 7 languages (English, Spanish, French, German, Portuguese, Chinese, Russian).
    - **Theme Switcher**: Dark, Light, and Transparent themes.
    - **Audio Alerts**: Toggle USB connect/disconnect and low battery sounds.
    - **Customizable Thresholds**: 10%, 15%, or 20% low battery alarm levels.
    - **Ghost Mode & Global Hotkey Toggles**.
- **Data Resilience**:
  - Configuration saved in `%AppData%\X-Charge\config.json` with automated self-healing against corrupted JSON.

---

## 🎮 Controller Compatibility

- **Native XInput Controllers (Plug & Play, No Extra Software)**:
  - **Official Xbox**: Xbox Series X/S, Xbox One, Xbox One S/X, Xbox Elite Series 1 & 2, Xbox 360 (via Official USB Wireless Adapter, USB cable, or Bluetooth).
  - **Third-Party PC Controllers**: 8BitDo (Ultimate, Pro 2, SN30 Pro in X mode), GameSir (G7, Nova, Kaleid, T4 Pro), Logitech (F710, F310 in "X" mode), Razer, PowerA, PDP, Gulikit (KingKong 2/3), Flydigi, and any PC gamepad with an XInput mode switch.
- **PlayStation (DualSense / PS5, DualShock 4 / PS4) & Nintendo Switch Pro**:
  - Emulated via virtual XInput tools (such as DS4Windows, Steam Input, DualSenseX, or reWASD), **X-Charge seamlessly detects them** through their virtual XInput slot.

---

## 🛠️ Architecture & Services

The application follows a clean, decoupled modular design:
- **`Services/XInputService.cs`**: Native Win32 XInput 1.4 hardware polling and battery state.
- **`Services/TrayIconService.cs`**: GDI+ dynamic battery icon generation with strict `DestroyIcon` lifecycle management.
- **`Services/HotKeyService.cs`**: Global hotkey registration via `RegisterHotKey` / `WM_HOTKEY`.
- **`Services/WindowStyleService.cs`**: Win32 extended window styles (`WS_EX_TRANSPARENT`) for click-through.
- **`Services/StartupService.cs`**: Windows Run registry management.
- **`Services/SoundService.cs`**: Audio notifications with `winmm.dll` and escalating battery alerts.

---

## 🚀 Building & Publishing

An automated PowerShell build script `build.ps1` is included:

```powershell
# 1. Framework-Dependent Single-File build (~24 MB)
.\build.ps1 -Clean

# 2. Self-Contained standalone executable (~74 MB, no .NET runtime required)
.\build.ps1 -SelfContained -Clean

# 3. Build, deploy to a custom directory, and run
.\build.ps1 -DeployPath "C:\MyGames\X-Charge" -Run
```

Single-file executables are automatically generated under `publish\X-Charge.exe`.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
