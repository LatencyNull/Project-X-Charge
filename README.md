# 🎮 X-Charge (v2.0)

**X-Charge** es un monitor flotante premium de batería y telemetría en tiempo real para Windows 10/11, optimizado especialmente para mandos **Xbox (Series X/S, One, Elite Series 2, 360)** conectados por **Receptor Inalámbrico USB Oficial**, cable USB o Bluetooth.

---

## ✨ Características Principales

- **Motor Nativo XInput (100% Seguro y Estable)**:
  - Arquitectura puramente Win32 (`XInputGetState` y `XInputGetBatteryInformation`).
  - **Cero interferencias de radiofrecuencia**: erradicado el fallo de apagado del mando al reconectar con el receptor inalámbrico de Xbox.
  - Detección inmediata de conexión y desconexión sin objetos zombi en memoria.
  - Soporte de hasta 4 mandos en simultáneo con identificación independiente de ranuras.
- **Detección de Estados y Alimentación Externa**:
  - Distinción instantánea entre conexión por cable USB (`UsbCharged`) y batería inalámbrica (`Alcalina` / `NiMH` / `Pack de Carga`).
  - Corrección completa del bug del 100%: nunca más desaparece el porcentaje ni se desborda la barra gráfica.
  - Alertas visuales y sonoras de batería baja configurables (umbral del 10%, 15% o 20%).
- **Modos de Visualización**:
  - **Modo Normal**: Ventana elegante con telemetría detallada, nivel de carga, colorimetría según estado y barra de progreso.
  - **Modo Compacto**: Indicador flotante circular minimalista con porcentaje en tiempo real, redimensionable mediante arrastre y posicionable libremente.
- **Protección contra Quemado OLED (Anti-Burn-In)**:
  - **Órbita Circular Suave**: Desplazamiento periódico imperceptible en trayectoria armónica ($\Delta X = 8\cos\theta, \Delta Y = 8\sin\theta$) con aceleración suave.
  - **Respiración de Opacidad (Fade Breathing)**: Oscilación continua de transparencia que se pausa suavemente al posar el cursor y se reanuda al salir.
- **Bandeja del Sistema (System Tray)**:
  - Icono dinámico generado al vuelo con porcentaje e indicador de cable.
  - Menú contextual completo: cambio de temas (Oscuro, Claro, Transparente), sonidos de conexión/desconexión USB, alertas de batería, auto-inicio con Windows y umbral de alerta.
- **Multi-idioma**: 7 idiomas incluidos (Español, Inglés, Francés, Alemán, Portugués, Chino y Ruso).
- **Resiliencia de Datos**:
  - Configuración persistente en `%AppData%\X-Charge\config.json` con migración automática y tolerancia a fallos.
- **Higiene de Recursos**:
  - Desuscripción rigurosa de eventos estáticos del sistema (`UserPreferenceChanged`).
  - Liberación segura de Mutex de instancia única.

---

## 🛠️ Stack Tecnológico

- **Framework**: .NET 8.0 SDK con WPF (`net8.0-windows10.0.19041.0`)
- **Versión Mínima de Windows**: Windows 10 build 17763 (10.0.17763.0)
- **APIs de Hardware**: `xinput1_4.dll` (con fallback a `xinput9_1_0.dll`)
- **Paquetes NuGet**:
  - `Hardcodet.NotifyIcon.Wpf` (v1.1.0)
  - `Microsoft.Toolkit.Uwp.Notifications` (v7.1.3)

---

## 🚀 Compilación y Publicación Automatizada

Se incluye el script PowerShell `build.ps1` en la raíz del repositorio:

```powershell
# 1. Compilación limpia y empaquetado Single-File dependiente del framework (~24 MB)
.\build.ps1 -Clean

# 2. Publicación autónoma (Self-Contained con compresión, ~74 MB, no requiere .NET en destino)
.\build.ps1 -SelfContained -Clean

# 3. Compilar y ejecutar de inmediato
.\build.ps1 -Run
```

El script genera automáticamente el ejecutable único en `publish\X-Charge.exe` y sincroniza la instalación de producción si existe.

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT.
