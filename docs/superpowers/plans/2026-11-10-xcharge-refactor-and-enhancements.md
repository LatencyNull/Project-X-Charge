# X-Charge Architecture Refactor & Enhancements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor X-Charge into a clean, modular service-oriented architecture, resolve all code smells and decompilation artifacts, fix hardcoded environment paths, and implement Click-Through (Ghost Mode), Global Hotkey, Dynamic Language Selector, and GitHub CI/CD workflows.

**Architecture:** Decompose the monolithic `MainWindow.xaml.cs` into focused, dedicated services (`XInputService`, `TrayIconService`, `StartupService`, `SoundService`, `HotKeyService`, `WindowStyleService`). Modernize `build.ps1`, `Strings.cs`, `AppSettings.cs`, and add GitHub community templates and automated GitHub Actions workflows.

**Tech Stack:** .NET 8.0 SDK, C# 12, WPF, Win32 P/Invoke (`xinput1_4.dll`, `user32.dll`, `winmm.dll`), GitHub Actions.

---

## Global Constraints
- Target Framework: `net8.0-windows10.0.19041.0`.
- Preserve 100% pure native XInput 1.4 API to prevent any RF interference on Xbox Wireless Adapters.
- Zero decompilation artifacts (no `//IL_...` comments, no redundant IL casts).
- No hardcoded personal or machine-specific directories anywhere in the repository.
- Ensure strict GDI handle cleanup (`DestroyIcon`) in dynamic icon generation.

---

## Review Focus
1. **GDI & Win32 Handle Leaks:** Every `CreateIconIndirect` / `GetHicon` must be accompanied by `DestroyIcon`.
2. **RF / Controller Safety:** Do not introduce WinRT or HID handles that interfere with the Xbox Wireless Adapter.
3. **Thread Safety in WPF:** Timer callbacks, tray menus, and hotkeys must safely marshal to the UI Dispatcher.
4. **Dynamic Localization:** Switching language at runtime must update all window UI elements and Tray context menus immediately.
5. **Click-Through Mode:** Enabling click-through must not freeze or prevent disabling it via the tray menu.

---

### Task 1: Clean Build Script & Git Hygiene
**Files:**
- Modify: `build.ps1`

- [ ] **Step 1: Parametrize deployment path in `build.ps1`**
  Remove hardcoded `H:\Programas Instalados W10\X-Charge`. Add optional parameter `[string]$DeployPath = $null`.
- [ ] **Step 2: Verify `build.ps1` syntax**
  Ensure script runs without hardcoded private machine paths.

---

### Task 2: Settings, Localization & App Entry Point
**Files:**
- Modify: `AppSettings.cs`
- Modify: `Strings.cs`
- Modify: `App.xaml.cs`

- [ ] **Step 1: Enhance `AppSettings.cs`**
  Add `Language` (string, default `""`), `ClickThrough` (bool, default `false`), and `HotKeyEnabled` (bool, default `true`).
- [ ] **Step 2: Extend `Strings.cs` with dynamic language switching & new keys**
  Add `SetLanguage(string code)`, `AvailableLanguages` dictionary, and keys for:
  `AlreadyRunning`, `Language`, `ClickThrough`, `HotKey`, `GhostModeActive`.
- [ ] **Step 3: Update `App.xaml.cs` single-instance check**
  Use `Strings.Get("AlreadyRunning")` instead of hardcoded Spanish string.

---

### Task 3: Extract Dedicated Win32 & Hardware Services
**Files:**
- Create: `Services/XInputService.cs`
- Create: `Services/TrayIconService.cs`
- Create: `Services/StartupService.cs`
- Create: `Services/SoundService.cs`
- Create: `Services/HotKeyService.cs`
- Create: `Services/WindowStyleService.cs`

- [ ] **Step 1: Implement `Services/XInputService.cs`**
  Encapsulate `xinput1_4.dll` P/Invokes, structs, constants, slot detection (0-3), and battery query.
- [ ] **Step 2: Implement `Services/TrayIconService.cs`**
  Encapsulate GDI+ bitmap creation, battery icon drawing, `GetHicon()`, and `DestroyIcon()`.
- [ ] **Step 3: Implement `Services/StartupService.cs`**
  Encapsulate Windows Registry Run key reading and writing.
- [ ] **Step 4: Implement `Services/SoundService.cs`**
  Encapsulate USB connect/disconnect sounds (`winmm.dll`) and low battery audio notification.
- [ ] **Step 5: Implement `Services/HotKeyService.cs`**
  Implement global hotkey registration (`RegisterHotKey` / `UnregisterHotKey`) for toggling window visibility.
- [ ] **Step 6: Implement `Services/WindowStyleService.cs`**
  Implement `WS_EX_TRANSPARENT` / `WS_EX_LAYERED` toggle for Click-Through / Ghost Mode.

---

### Task 4: Refactor & Clean `MainWindow.xaml` and `MainWindow.xaml.cs`
**Files:**
- Modify: `MainWindow.xaml`
- Modify: `MainWindow.xaml.cs`

- [ ] **Step 1: Update `MainWindow.xaml` Tray Context Menu**
  Add Language submenu, Click-Through toggle, and Hotkey toggle.
- [ ] **Step 2: Clean `MainWindow.xaml.cs`**
  Remove all `//IL_...` comments, remove redundant casts, remove dead code `TestShineButton_Click`, remove artificial hours estimate.
- [ ] **Step 3: Connect `MainWindow.xaml.cs` to the new Services**
  Delegate hardware, tray, sound, registry, and hotkey actions to their respective services.

---

### Task 5: GitHub Community & CI/CD Files
**Files:**
- Create: `.github/workflows/build-release.yml`
- Create: `.github/ISSUE_TEMPLATE/bug_report.md`
- Create: `.github/ISSUE_TEMPLATE/feature_request.md`
- Create: `CONTRIBUTING.md`
- Modify: `README.md`
- Modify: `README.es.md`

- [ ] **Step 1: Create GitHub Actions build and release workflow**
- [ ] **Step 2: Create Issue templates and Contributing guidelines**
- [ ] **Step 3: Update README files with new features and documentation**

---

### Task 6: Verification & Compilation
- [ ] **Step 1: Compile with `dotnet build -c Release`**
  Confirm 0 errors and 0 warnings.
- [ ] **Step 2: Publish with `dotnet publish -c Release`**
  Confirm single-file publishing succeeds.
