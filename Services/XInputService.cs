using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XboxBatteryMonitor.Services;

/// <summary>
/// Proporciona acceso nativo y seguro a la API Win32 XInput 1.4.
/// Diseñado para máxima estabilidad sin generar interferencia de radiofrecuencia (RF)
/// en el adaptador inalámbrico oficial de Xbox para Windows.
/// </summary>
public static class XInputService
{
    private const string XInputDll = "xinput1_4.dll";

    public const byte BATTERY_DEVTYPE_GAMEPAD = 0x00;
    public const byte BATTERY_TYPE_DISCONNECTED = 0x00;
    public const byte BATTERY_TYPE_WIRED = 0x01;
    public const byte BATTERY_TYPE_ALKALINE = 0x02;
    public const byte BATTERY_TYPE_NIMH = 0x03;
    public const byte BATTERY_TYPE_UNKNOWN = 0xFF;

    public const byte BATTERY_LEVEL_EMPTY = 0x00;
    public const byte BATTERY_LEVEL_LOW = 0x01;
    public const byte BATTERY_LEVEL_MEDIUM = 0x02;
    public const byte BATTERY_LEVEL_FULL = 0x03;

    private const uint ERROR_SUCCESS = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_STATE
    {
        public uint dwPacketNumber;
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_BATTERY_INFORMATION
    {
        public byte BatteryType;
        public byte BatteryLevel;
    }

    [DllImport(XInputDll, EntryPoint = "XInputGetState")]
    private static extern uint XInputGetState(uint dwUserIndex, out XINPUT_STATE pState);

    [DllImport(XInputDll, EntryPoint = "XInputGetBatteryInformation")]
    private static extern uint XInputGetBatteryInformation(uint dwUserIndex, byte devType, out XINPUT_BATTERY_INFORMATION pBatteryInformation);

    /// <summary>
    /// Escanea las 4 ranuras estándar de XInput (0 a 3) y devuelve los índices conectados.
    /// </summary>
    public static List<int> GetConnectedSlots()
    {
        List<int> activeSlots = new(4);
        for (uint slot = 0; slot < 4; slot++)
        {
            try
            {
                if (XInputGetState(slot, out _) == ERROR_SUCCESS)
                {
                    activeSlots.Add((int)slot);
                }
            }
            catch
            {
                // Silencioso ante cualquier excepción de driver
            }
        }
        return activeSlots;
    }

    /// <summary>
    /// Obtiene el tipo de alimentación y nivel de carga para una ranura específica
    /// mediante llamada directa a XInputGetBatteryInformation (cero llamadas redundantes a GetState).
    /// </summary>
    public static bool TryGetBattery(int slot, out byte batteryType, out byte batteryLevel)
    {
        try
        {
            uint res = XInputGetBatteryInformation((uint)slot, BATTERY_DEVTYPE_GAMEPAD, out var info);
            if (res == ERROR_SUCCESS && info.BatteryType != BATTERY_TYPE_DISCONNECTED)
            {
                batteryType = info.BatteryType;
                batteryLevel = info.BatteryLevel;
                return true;
            }
        }
        catch
        {
            // Silencioso ante fallos temporales de dispositivo
        }

        batteryType = BATTERY_TYPE_DISCONNECTED;
        batteryLevel = BATTERY_LEVEL_EMPTY;
        return false;
    }
}
