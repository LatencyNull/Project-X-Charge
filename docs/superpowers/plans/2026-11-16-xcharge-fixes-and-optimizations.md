# X-Charge Fixes, Optimizations & Lightweight Architecture Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement critical bugfixes (10% threshold, ghost mode gaming key conflict, window height reset, startup registry read-only), remove the massive 25MB Windows SDK bloat, optimize GDI tray icon caching, calibrate anti-burn-in with a user toggle, and add multi-controller slot badges in compact mode.

**Architecture:** Refactor `XInputService`, `TrayIconService`, `SoundService`, and `StartupService` to be strictly decoupled, leak-free, and lightweight. Remove `Microsoft.Toolkit.Uwp.Notifications` (and its ~25MB dependency `Microsoft.Windows.SDK.NET.dll`) in favor of native taskbar notifications. DeepSeek V4 Flash implements the code changes while GLM 5.3 provides review and technical validation.

**Tech Stack:** .NET 8.0 SDK (`net8.0-windows`), C# 12, WPF, Win32 P/Invoke (`xinput1_4.dll`, `user32.dll`, `winmm.dll`), `Hardcodet.NotifyIcon.Wpf`.

---

## Global Constraints

- Target Framework: Change from `net8.0-windows10.0.19041.0` to `net8.0-windows` to eliminate the 24.8MB `Microsoft.Windows.SDK.NET.dll`.
- Zero compiler warnings and zero errors (`dotnet build -c Release`).
- Strict GDI handle cleanup (`DestroyIcon`) in `TrayIconService.cs` and state caching to avoid continuous GDI object creation.
- No `GetAsyncKeyState` polling loops that interfere with gaming keystrokes (`Shift`/`Ctrl`).
- Safe single-instance communication without `HWND_BROADCAST` lockups.

## Review Focus

1. **Threshold Triggering:** Battery level EMPTY must map to 5% (not 15%) so 10%, 15%, and 20% alarm thresholds trigger reliably.
2. **Ghost Mode Gaming Safety:** `Shift` and `Ctrl` must not toggle `WS_EX_TRANSPARENT`. Ghost mode must be controlled strictly via HotKey or Tray Menu.
3. **Binary Footprint:** Confirm Framework-Dependent build drops from ~26MB to under 1MB, and Standalone drops significantly.
4. **WPF Window Dimensions:** Switching Compact -> Normal -> Compact mode must not freeze `Height` or clip the multi-controller list.
5. **GDI Stability:** Taskbar icon must only be regenerated when battery percentage, charging state, or connection state changes.

---

### Task 1: Fix Battery Level Mapping & Threshold Logic
**Files:**
- Modify: `Services/XInputService.cs`
- Modify: `MainWindow.xaml.cs`

- [x] **Step 1: Update discrete battery level mapping in `MainWindow.xaml.cs`**
  Map `XInputService.BATTERY_LEVEL_EMPTY` to `5.0%`, `LOW` to `25.0%`, `MEDIUM` to `65.0%`, and `FULL` to `100.0%`.
- [x] **Step 2: Verify threshold comparisons**
  Verify that `5.0%` triggers the `10%`, `15%`, and `20%` thresholds, and `25.0%` does not trigger `20%`.
- [x] **Step 3: Test compilation**
  Run `dotnet build -c Release` to verify syntax.

---

### Task 2: Eliminate Ghost Mode Keyboard Polling Timer (`Shift`/`Ctrl` Gaming Conflict)
**Files:**
- Modify: `MainWindow.xaml.cs`
- Modify: `MainWindow.xaml`
- Modify: `Strings.cs`

- [x] **Step 1: Remove `_ghostBypassTimer` and `GetAsyncKeyState` polling**
  Remove the 50ms dispatcher timer, `GetAsyncKeyState(VK_SHIFT)`, and `GetAsyncKeyState(VK_CONTROL)`.
- [x] **Step 2: Update Ghost Mode activation logic**
  Ghost mode (`WS_EX_TRANSPARENT`) applies when `ClickThroughEnabled` is true in Compact Mode. Toggle is handled via the Tray menu or the global hotkey (`Ctrl + Shift + X`).
- [x] **Step 3: Update tooltips and strings**
  Update `Strings.cs` to clarify that Ghost Mode is toggled via the hotkey or tray icon, removing outdated references to holding Shift/Ctrl.

---

### Task 3: Fix Window Size and Height Restoration in Full Mode
**Files:**
- Modify: `MainWindow.xaml.cs`

- [x] **Step 1: Reset `Height = double.NaN` and clear local height property**
  In `SwitchToFullMode()`, call `ClearValue(HeightProperty)` and set `Height = double.NaN` before setting `SizeToContent = SizeToContent.Height`.
- [x] **Step 2: Cancel running burn-in animations on resize**
  In `ResizeThumb_DragDelta`, call `BeginAnimation(LeftProperty, null)` and `BeginAnimation(TopProperty, null)` before setting dimensions.

---

### Task 4: Fix `StartupService.cs` (Command-Query Separation)
**Files:**
- Modify: `Services/StartupService.cs`

- [x] **Step 1: Make `IsStartupEnabled()` strictly read-only**
  Remove the `writable: true` opening and path rewriting logic from `IsStartupEnabled()`. Only check if the key exists and matches.
- [x] **Step 2: Consolidate path updates into `SetStartup(true)`**
  Only update or repair the registry value when the user explicitly enables startup in `SetStartup(true)`.

---

### Task 5: Eliminate 25MB Windows SDK Bloat (`Microsoft.Toolkit.Uwp.Notifications`)
**Files:**
- Modify: `X-Charge.csproj`
- Modify: `MainWindow.xaml.cs`
- Modify: `App.xaml.cs`

- [x] **Step 1: Update TargetFramework and remove PackageReference**
  In `X-Charge.csproj`, change `<TargetFramework>` from `net8.0-windows10.0.19041.0` to `net8.0-windows` and remove `<PackageReference Include="Microsoft.Toolkit.Uwp.Notifications" ... />`.
- [x] **Step 2: Replace ToastContentBuilder with TrayIcon.ShowBalloonTip**
  In `MainWindow.xaml.cs`, replace `ToastContentBuilder` with `TrayIcon.ShowBalloonTip(Strings.Get("LowBattery"), message, BalloonIcon.Warning)`.
- [x] **Step 3: Audit output size**
  Run `dotnet publish -c Release -r win-x64 --self-contained false` and verify `Microsoft.Windows.SDK.NET.dll` is gone.

---

### Task 6: Modernize & Lighten `SoundService.cs`
**Files:**
- Modify: `Services/SoundService.cs`
- Modify: `MainWindow.xaml.cs`

- [x] **Step 1: Remove `System.Windows.Media.MediaPlayer`**
  Remove `MediaPlayer` and its COM initialization.
- [x] **Step 2: Implement lightweight low battery alert**
  Use native `PlaySound` via `winmm.dll` with system sound alias (`"SystemExclamation"`), completely avoiding unmanaged file dependencies or missing media files.

---

### Task 7: Cache Tray Icon State in `TrayIconService` / `MainWindow`
**Files:**
- Modify: `MainWindow.xaml.cs`
- Modify: `Services/TrayIconService.cs`

- [x] **Step 1: Implement state caching in `MainWindow.xaml.cs`**
  Store `_lastTrayPercent`, `_lastTrayIsOnExternalPower`, `_lastTrayIsConnected`, `_lastTrayThreshold`.
- [x] **Step 2: Skip icon generation when unchanged**
  If none of these parameters changed, update only `ToolTipText` if needed and skip `GenerateBatteryIcon`.

---

### Task 8: Calibrate Anti-Burn-In (Pixel Shift) & Add User Toggle
**Files:**
- Modify: `AppSettings.cs`
- Modify: `MainWindow.xaml`
- Modify: `MainWindow.xaml.cs`
- Modify: `Strings.cs`

- [x] **Step 1: Add `AntiBurnInEnabled` property to `AppSettings.cs`**
  Default to `true`, persist in `config.json`.
- [x] **Step 2: Add Tray Menu item**
  Add a menu item in `TrayMenu` for toggling Anti-Burn-In with localization in `Strings.cs`.
- [x] **Step 3: Calibrate orbit frequency and magnitude**
  Change orbit shift from 8px every 20s to a subtle 4px shift every 120s with smooth transition.

---

### Task 9: Add Controller Slot Indicator (P1, P2) in Compact Mode
**Files:**
- Modify: `MainWindow.xaml`
- Modify: `MainWindow.xaml.cs`

- [x] **Step 1: Add Slot Badge in Compact Mode XAML**
  Add a subtle badge (e.g. `Border` with `TextBlock` `Name="CompactSlotBadge"`) positioned cleanly at the top-left of `CompactModeBorder`.
- [x] **Step 2: Bind slot to the lowest battery controller**
  When multiple controllers are connected, show `P{slot + 1}` next to the percentage so the user immediately knows which gamepad is low. Hide the badge when only 1 controller is connected.

---

### Task 10: Complete Build, Verification & Release Script Validation
**Files:**
- Modify: `build.ps1`
- Verify: `publish/`

- [x] **Step 1: Run full clean build**
  Execute `dotnet build -c Release`.
- [x] **Step 2: Execute build script and verify packages**
  Run `.\build.ps1 -Clean` and check binary sizes and SHA256.
- [x] **Step 3: Verify single instance and application execution**
  Confirm 0 warnings, 0 errors, and successful packaging.
