# 🎮 X-Charge (v1.0)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6.svg)](https://microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-8.0%20WPF-512BD4.svg)](https://dotnet.microsoft.com/)
[![CI/CD](https://github.com/LatencyNull/X-Charge/actions/workflows/build-release.yml/badge.svg)](https://github.com/LatencyNull/X-Charge/actions)

**X-Charge** es un monitor de batería flotante premium y HUD de telemetría en tiempo real para Windows 10 y 11, diseñado específicamente para **mandos de Xbox** y cualquier **gamepad compatible con XInput** en PC.

[English](README.md) • [Español](README.es.md)

---

## ✨ Características Principales

- **Motor XInput 100% Nativo (Estabilidad Absoluta)**:
  - Desarrollado puramente sobre las APIs Win32 de XInput (`xinput1_4.dll`).
  - **Cero interferencia RF**: elimina por completo el infame bug de apagado del mando inalámbrico al reconectarse con el adaptador inalámbrico oficial de Xbox para Windows.
  - Soporte multi-mando: monitoriza hasta 4 mandos de forma simultánea con identificación independiente de ranura.
- **Modos de Visualización y HUD para Juegos**:
  - **Modo Normal**: Panel detallado con nombre del mando, porcentaje de batería, estado de carga y barra de estado.
  - **Modo Compacto**: Widget flotante circular minimalista con porcentaje en tiempo real, libremente arrastrable y redimensionable.
  - **Opacidad interactiva con la rueda del ratón**: Pasa el cursor sobre el modo compacto y gira la rueda del ratón para regular la transparencia en tiempo real (desde 15% ultratranslúcido hasta 100% nítido).
  - **Modo Fantasma (Click-Through)**: Permite que los clics del ratón atraviesen el HUD flotante (`WS_EX_TRANSPARENT`), permitiéndote jugar y hacer clic en la pantalla del juego sin interferencias.
  - **Atajo global de teclado (`Ctrl + Shift + X`)**: Muestra u oculta el HUD al instante desde cualquier juego a pantalla completa sin salir al escritorio.
- **Protección contra Quemado en Pantallas OLED (Anti-Burn-In)**:
  - **Órbita Circular Armónica Calibrada**: Desplazamiento periódico sutil ($\Delta X = 4\cos\theta, \Delta Y = 4\sin\theta$) cada 120 segundos para proteger paneles OLED/AMOLED, con interruptor en el menú de la bandeja para usuarios con monitores LCD.
- **Integración con la Bandeja del Sistema (System Tray)**:
  - Icono pixel-art dinámico que refleja el nivel exacto de carga en la barra de tareas (con caché de estado y liberación estricta de handles GDI).
  - Menú contextual:
    - **Selector de Idioma Dinámico**: 7 idiomas con cambio en caliente sin reiniciar (Español, Inglés, Francés, Alemán, Portugués, Chino y Ruso).
    - **Selector de Temas**: Oscuro, Claro y Transparente.
    - **Alertas de Audio**: Notificaciones sonoras al conectar/desconectar USB y advertencia de batería baja.
    - **Umbrales Configurables**: Alarma al 10%, 15% o 20%.
    - **Interruptores para Modo Fantasma, Anti-Burn-In y Atajo Global**.
- **Resiliencia de Datos**:
  - Configuración guardada en `%AppData%\X-Charge\config.json` con autoreparación ante apagones repentinos o corrupción de JSON.

---

## 🎮 Compatibilidad de Mandos

- **Mandos Nativos XInput (Plug & Play, Sin Software Adicional)**:
  - **Xbox Oficiales**: Xbox Series X/S, Xbox One, Xbox One S/X, Xbox Elite Series 1 y 2, Xbox 360 (vía Adaptador Inalámbrico USB oficial, cable USB o Bluetooth).
  - **Mandos de Terceros para PC**: 8BitDo (Ultimate, Pro 2, SN30 Pro en modo X), GameSir (G7, Nova, Kaleid, T4 Pro), Logitech (F710, F310 en modo "X"), Razer, PowerA, PDP, Gulikit (KingKong 2/3), Flydigi, y cualquier mando con selector XInput.
- **PlayStation (DualSense / PS5, DualShock 4 / PS4) y Nintendo Switch Pro**:
  - Emulados mediante utilidades XInput virtuales (como DS4Windows, Steam Input, DualSenseX o reWASD), **X-Charge los detecta inmediatamente** a través de su ranura virtual XInput.

---

## 🛠️ Arquitectura y Servicios

El proyecto está diseñado bajo una arquitectura modular desacoplada:
- **`Services/XInputService.cs`**: Sondeo de hardware y telemetría de batería Win32 XInput 1.4.
- **`Services/TrayIconService.cs`**: Renderizado dinámico GDI+ de iconos de bandeja con gestión estricta de `DestroyIcon`.
- **`Services/HotKeyService.cs`**: Registro de atajo global mediante `RegisterHotKey` / `WM_HOTKEY`.
- **`Services/WindowStyleService.cs`**: Estilos extendidos Win32 (`WS_EX_TRANSPARENT`) para modo fantasma.
- **`Services/StartupService.cs`**: Gestión de arranque con Windows en el Registro.
- **`Services/SoundService.cs`**: Notificaciones de audio con `winmm.dll` y alertas de batería progresivas.

---

## 🚀 Compilación y Publicación

Se incluye un script automatizado en PowerShell `build.ps1`:

```powershell
# 1. Compilación Single-File dependiente del framework (~350 KB)
.\build.ps1 -Clean

# 2. Ejecutable autónomo independiente (~74 MB, no requiere tener .NET instalado)
.\build.ps1 -SelfContained -Clean

# 3. Compilar, desplegar en carpeta personalizada y ejecutar
.\build.ps1 -DeployPath "C:\MisProgramas\X-Charge" -Run
```

Los ejecutables generados se ubican en `publish\X-Charge.exe`.

---

## 📄 Licencia

Este proyecto está bajo la [Licencia MIT](LICENSE).
