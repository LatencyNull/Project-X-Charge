using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;

namespace XboxBatteryMonitor;

public partial class MainWindow : Window
{
	private readonly DispatcherTimer _timer;

	private bool _isExiting;

	private double _compactSize = 80.0;

	private DispatcherTimer _burnInTimer;

	private UserPreferenceChangedEventHandler? _userPreferenceChangedHandler;

	private double _anchorLeft = double.NaN;

	private double _anchorTop = double.NaN;

	private double _orbitAngle;

	private MediaPlayer? _alertMediaPlayer;

	private DispatcherTimer? _flashTimer;

	private int _flashRemaining;

	private int _flashColorIndex;

	private System.Windows.Media.Brush? _originalBackground;

	private static readonly System.Windows.Media.Color[] FlashColors = new System.Windows.Media.Color[6]
	{
		System.Windows.Media.Color.FromRgb(226, 75, 74),
		System.Windows.Media.Color.FromRgb(245, 166, 35),
		System.Windows.Media.Color.FromRgb(250, 204, 21),
		System.Windows.Media.Color.FromRgb(226, 75, 74),
		System.Windows.Media.Color.FromRgb(byte.MaxValue, 96, 96),
		System.Windows.Media.Color.FromRgb(245, 166, 35)
	};

	public ObservableCollection<ControllerInfo> Controllers { get; } = new ObservableCollection<ControllerInfo>();


	private Border ActiveBorder
	{
		get
		{
			if (FullModeBorder.Visibility != 0)
			{
				return CompactModeBorder;
			}
			return FullModeBorder;
		}
	}

	[DllImport("winmm.dll", SetLastError = true)]
	public static extern bool PlaySound(string pszSound, nint hmod, uint fdwSound);

	public MainWindow()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		InitializeComponent();
		ApplyLanguage();
		_originalBackground = FullModeBorder.Background;
		SetupTray();
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(AppSettings.Current.PollIntervalSeconds)
		};
		_timer.Tick += delegate
		{
			UpdateBatteryStatus();
		};
		_timer.Start();
		UpdateBatteryStatus();
		_burnInTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(20.0)
		};
		_burnInTimer.Tick += BurnInTimer_Tick;
		_burnInTimer.Start();
		AutoSyncWindowsTheme();
		_userPreferenceChangedHandler = delegate(object s, UserPreferenceChangedEventArgs e)
		{
			if (e.Category == UserPreferenceCategory.General)
			{
				((DispatcherObject)this).Dispatcher.Invoke((Action)AutoSyncWindowsTheme);
			}
		};
		SystemEvents.UserPreferenceChanged += _userPreferenceChangedHandler;
		Rect workArea = SystemParameters.WorkArea;
		base.Left = workArea.Right - base.Width - 20.0;
		base.Top = workArea.Top + 20.0;
		base.Closing += delegate(object? _, CancelEventArgs e)
		{
			if (!_isExiting)
			{
				e.Cancel = true;
				HideWindow();
			}
			else
			{
				if (_userPreferenceChangedHandler != null)
				{
					SystemEvents.UserPreferenceChanged -= _userPreferenceChangedHandler;
					_userPreferenceChangedHandler = null;
				}
			}
		};
	}

	private void SetupTray()
	{
		ToggleWindowItem.Header = Strings.Get("Show");
		DarkThemeItem.IsChecked = AppSettings.Current.ThemeIndex == 0;
		LightThemeItem.IsChecked = AppSettings.Current.ThemeIndex == 1;
		TransparentThemeItem.IsChecked = AppSettings.Current.ThemeIndex == 2;
		UsbSoundItem.IsChecked = AppSettings.Current.SoundUsbEnabled;
		BatterySoundItem.IsChecked = AppSettings.Current.SoundBatteryEnabled;
		Th10Item.IsChecked = AppSettings.Current.LowBatteryThreshold == 10;
		Th15Item.IsChecked = AppSettings.Current.LowBatteryThreshold == 15;
		Th20Item.IsChecked = AppSettings.Current.LowBatteryThreshold == 20;
		StartupItem.IsChecked = IsStartupEnabled();
		if (HideUntilLowItem != null)
		{
			HideUntilLowItem.IsChecked = AppSettings.Current.HideUntilLowBattery;
		}
		if (StartInTrayItem != null)
		{
			StartInTrayItem.IsChecked = AppSettings.Current.StartInTray;
		}
		TrayIcon.Icon = GenerateBatteryIcon(-1.0, isCharging: false, isConnected: false);
		SetTheme(AppSettings.Current.ThemeIndex);
	}

	private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
	{
		ToggleWindow();
	}

	private void ToggleWindowItem_Click(object sender, RoutedEventArgs e)
	{
		ToggleWindow();
	}

	private void ThemeItem_Click(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem { Tag: string tag } && int.TryParse(tag, out var result))
		{
			SetTheme(result);
		}
	}

	private void UsbSoundItem_Click(object sender, RoutedEventArgs e)
	{
		AppSettings.Current.SoundUsbEnabled = UsbSoundItem.IsChecked;
		AppSettings.Current.Save();
	}

	private void BatterySoundItem_Click(object sender, RoutedEventArgs e)
	{
		AppSettings.Current.SoundBatteryEnabled = BatterySoundItem.IsChecked;
		AppSettings.Current.Save();
	}

	private void ThresholdItem_Click(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem { Tag: string tag } menuItem && int.TryParse(tag, out var result))
		{
			Th10Item.IsChecked = false;
			Th15Item.IsChecked = false;
			Th20Item.IsChecked = false;
			menuItem.IsChecked = true;
			AppSettings.Current.LowBatteryThreshold = result;
			AppSettings.Current.Save();
			UpdateBatteryStatus();
		}
	}

	private void StartupItem_Click(object sender, RoutedEventArgs e)
	{
		ToggleStartup(StartupItem.IsChecked);
	}

	private void StartInTrayItem_Click(object sender, RoutedEventArgs e)
	{
		AppSettings.Current.StartInTray = StartInTrayItem.IsChecked;
		AppSettings.Current.Save();
	}

	private void HideUntilLowItem_Click(object sender, RoutedEventArgs e)
	{
		AppSettings.Current.HideUntilLowBattery = HideUntilLowItem.IsChecked;
		AppSettings.Current.Save();
		UpdateBatteryStatus();
	}

	private void ExitItem_Click(object sender, RoutedEventArgs e)
	{
		ExitApp();
	}

	private void SetTheme(int index)
	{
		AppSettings.Current.ThemeIndex = index;
		AppSettings.Current.Save();
		if (DarkThemeItem != null)
		{
			DarkThemeItem.IsChecked = index == 0;
		}
		if (LightThemeItem != null)
		{
			LightThemeItem.IsChecked = index == 1;
		}
		if (TransparentThemeItem != null)
		{
			TransparentThemeItem.IsChecked = index == 2;
		}
		System.Windows.Media.Color color;
		System.Windows.Media.Color color2;
		System.Windows.Media.Color color3;
		System.Windows.Media.Color color4;
		System.Windows.Media.Color color5;
		if (AppSettings.Current.ThemeIndex == 0)
		{
			color = System.Windows.Media.Color.FromArgb(230, 32, 32, 32);
			color2 = System.Windows.Media.Color.FromRgb(64, 64, 64);
			color3 = System.Windows.Media.Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
			color4 = System.Windows.Media.Color.FromRgb(153, 153, 153);
			color5 = System.Windows.Media.Color.FromRgb(170, 170, 170);
			System.Windows.Media.Color.FromRgb(51, 51, 51);
		}
		else if (AppSettings.Current.ThemeIndex == 1)
		{
			color = System.Windows.Media.Color.FromArgb(230, 245, 245, 245);
			color2 = System.Windows.Media.Color.FromRgb(200, 200, 200);
			color3 = System.Windows.Media.Color.FromRgb(30, 30, 30);
			color4 = System.Windows.Media.Color.FromRgb(100, 100, 100);
			color5 = System.Windows.Media.Color.FromRgb(100, 100, 100);
			System.Windows.Media.Color.FromRgb(220, 220, 220);
		}
		else
		{
			color = System.Windows.Media.Color.FromArgb(2, 0, 0, 0);
			color2 = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);
			color3 = System.Windows.Media.Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue);
			color4 = System.Windows.Media.Color.FromRgb(200, 200, 200);
			color5 = System.Windows.Media.Color.FromRgb(200, 200, 200);
			System.Windows.Media.Color.FromArgb(100, 51, 51, 51);
		}
		FullModeBorder.Background = new SolidColorBrush(color);
		CompactModeBorder.Background = new SolidColorBrush(color);
		FullModeBorder.BorderBrush = new SolidColorBrush(color2);
		CompactModeBorder.BorderBrush = new SolidColorBrush(color2);
		_originalBackground = FullModeBorder.Background;
		TitleText.Foreground = new SolidColorBrush(color3);
		StatusText.Foreground = new SolidColorBrush(color4);
		CompactButton.Foreground = new SolidColorBrush(color5);
		MinimizeButton.Foreground = new SolidColorBrush(color5);
		CloseButton.Foreground = new SolidColorBrush(color5);
		FullModeButton.Foreground = new SolidColorBrush(color5);
		UpdateBatteryStatus();
	}

	private static Icon GenerateBatteryIcon(double percent, bool isCharging, bool isConnected)
	{
		using Bitmap bitmap = new Bitmap(16, 16);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(System.Drawing.Color.Transparent);
		System.Drawing.Color color = System.Drawing.Color.FromArgb(200, 200, 200);
		System.Drawing.Color color2 = ((!isConnected || percent < 0.0) ? System.Drawing.Color.FromArgb(100, 100, 100) : (isCharging ? System.Drawing.Color.FromArgb(59, 130, 246) : ((percent <= (double)AppSettings.Current.LowBatteryThreshold) ? System.Drawing.Color.FromArgb(226, 75, 74) : ((!(percent <= 50.0)) ? System.Drawing.Color.FromArgb(29, 158, 117) : System.Drawing.Color.FromArgb(245, 166, 35)))));
		Rectangle rect = new Rectangle(0, 3, 13, 10);
		using (System.Drawing.Pen pen = new System.Drawing.Pen(color, 1.2f))
		{
			graphics.DrawRectangle(pen, rect);
		}
		using (SolidBrush brush = new SolidBrush(color))
		{
			graphics.FillRectangle(brush, 13, 6, 3, 4);
		}
		if (isConnected && percent >= 0.0)
		{
			int num = (int)(11.0 * Math.Clamp(percent, 0.0, 100.0) / 100.0);
			if (num > 0)
			{
				Rectangle rect2 = new Rectangle(1, 4, num, 8);
				using SolidBrush brush2 = new SolidBrush(color2);
				graphics.FillRectangle(brush2, rect2);
			}
		}
		if (isCharging && isConnected)
		{
			using System.Drawing.Pen pen2 = new System.Drawing.Pen(System.Drawing.Color.White, 1.5f);
			graphics.DrawLine(pen2, 8, 3, 5, 8);
			graphics.DrawLine(pen2, 5, 8, 9, 8);
			graphics.DrawLine(pen2, 9, 8, 6, 13);
		}
		nint hicon = bitmap.GetHicon();
		Icon result;
		using (Icon tempIcon = System.Drawing.Icon.FromHandle(hicon))
		{
			result = (Icon)tempIcon.Clone();
		}
		DestroyIcon(hicon);
		return result;
	}

	[DllImport("user32.dll")]
	private static extern bool DestroyIcon(nint handle);

	private void ApplyLanguage()
	{
		TitleText.Text = Strings.Get("TitleText");
		CompactButton.ToolTip = Strings.Get("CompactMode");
		MinimizeButton.ToolTip = Strings.Get("MinimizeTray");
		CloseButton.ToolTip = Strings.Get("CloseTray");
		FullModeButton.ToolTip = Strings.Get("NormalMode");
		StatusText.Text = Strings.Get("Searching");
		if (TrayIcon != null)
		{
			TrayIcon.ToolTipText = "X-Charge";
		}
		if (ToggleWindowItem != null)
		{
			ToggleWindowItem.Header = Strings.Get("Show");
		}
		if (ThemesMenuItem != null)
		{
			ThemesMenuItem.Header = Strings.Get("Themes");
		}
		if (SoundsMenuItem != null)
		{
			SoundsMenuItem.Header = Strings.Get("PlaySounds");
		}
		if (ThresholdMenuItem != null)
		{
			ThresholdMenuItem.Header = Strings.Get("BatteryOptions");
		}
		if (StartupItem != null)
		{
			StartupItem.Header = Strings.Get("StartWindows");
		}
		if (StartInTrayItem != null)
		{
			StartInTrayItem.Header = Strings.Get("StartInTray");
		}
		if (ExitItem != null)
		{
			ExitItem.Header = Strings.Get("Exit");
		}
	}

	private void AutoSyncWindowsTheme()
	{
		try
		{
			object? value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", 1);
			int theme = ((value == null) ? 1 : ((int)value));
			SetTheme(theme);
		}
		catch
		{
		}
	}

	private void UpdateTrayIconAndTooltip(double percent, bool isOnExternalPower, bool isConnected, string statusText, string controllerName)
	{
		if (TrayIcon != null)
		{
			Icon icon = TrayIcon.Icon;
			TrayIcon.Icon = GenerateBatteryIcon(percent, isOnExternalPower, isConnected);
			icon?.Dispose();
			string text;
			if (!isConnected)
			{
				text = Strings.Get("Controller") + " - " + Strings.Get("NotConnected");
			}
			else if (percent >= 0.0)
			{
				int num = (int)Math.Round(percent / 100.0 * 40.0);
				string value = string.Format(Strings.Get("HoursLeft"), num);
				text = $"{controllerName} - {Math.Round(percent)}% {value} - {statusText}";
			}
			else
			{
				text = controllerName + " - " + Strings.Get("NoData");
			}
			TrayIcon.ToolTipText = ((text.Length > 63) ? text.Substring(0, 63) : text);
		}
	}

	private void ShowWindow()
	{
		if (base.Visibility != 0)
		{
			base.Opacity = 0.0;
			Show();
		}
		base.WindowState = WindowState.Normal;
		Activate();
		DoubleAnimation animation = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.2));
		BeginAnimation(UIElement.OpacityProperty, animation);
	}

	private void HideWindow()
	{
		if (base.Visibility != Visibility.Visible)
		{
			return;
		}
		DoubleAnimation doubleAnimation = new DoubleAnimation(0.0, TimeSpan.FromSeconds(0.2));
		doubleAnimation.Completed += delegate
		{
			if (!_isExiting)
			{
				Hide();
			}
		};
		BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
	}

	private void ToggleWindow()
	{
		if (base.Visibility == Visibility.Visible && base.Opacity > 0.0)
		{
			HideWindow();
			if (ToggleWindowItem != null)
			{
				ToggleWindowItem.Header = Strings.Get("Show");
			}
		}
		else
		{
			ShowWindow();
			if (ToggleWindowItem != null)
			{
				ToggleWindowItem.Header = Strings.Get("Hide");
			}
		}
	}

	private bool IsStartupEnabled()
	{
		try
		{
			using RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: false);
			return (registryKey?.GetValue("X-Charge") != null) || (registryKey?.GetValue("XboxBatteryMonitor") != null);
		}
		catch
		{
			return false;
		}
	}

	private void ToggleStartup(bool enable)
	{
		try
		{
			using RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
			if (registryKey != null)
			{
				registryKey.DeleteValue("XboxBatteryMonitor", throwOnMissingValue: false);
				if (!enable)
				{
					registryKey.DeleteValue("X-Charge", throwOnMissingValue: false);
					return;
				}
				string exePath = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "X-Charge.exe");
				registryKey.SetValue("X-Charge", $"\"{exePath}\"");
			}
		}
		catch
		{
		}
	}

	private void ExitApp()
	{
		_isExiting = true;
		_timer.Stop();
		_burnInTimer?.Stop();
		if (_userPreferenceChangedHandler != null)
		{
			SystemEvents.UserPreferenceChanged -= _userPreferenceChangedHandler;
			_userPreferenceChangedHandler = null;
		}
		DispatcherTimer? flashTimer = _flashTimer;
		if (flashTimer != null)
		{
			flashTimer.Stop();
		}
		try
		{
			_alertMediaPlayer?.Close();
			_alertMediaPlayer = null;
		}
		catch
		{
		}
		if (TrayIcon != null)
		{
			((UIElement)(object)TrayIcon).Visibility = Visibility.Collapsed;
			TrayIcon.Dispose();
		}
		Application.Current.Shutdown();
	}
	private void PlayConnectSound()
	{
		if (!AppSettings.Current.SoundUsbEnabled)
		{
			return;
		}
		try
		{
			PlaySound("DeviceConnect", IntPtr.Zero, 2162689u);
		}
		catch
		{
		}
	}

	private void PlayDisconnectSound()
	{
		if (!AppSettings.Current.SoundUsbEnabled)
		{
			return;
		}
		try
		{
			PlaySound("DeviceDisconnect", IntPtr.Zero, 2162689u);
		}
		catch
		{
		}
	}

	
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

	[DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
	private static extern uint XInputGetState14(uint dwUserIndex, out XINPUT_STATE pState);

	[DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState")]
	private static extern uint XInputGetState910(uint dwUserIndex, out XINPUT_STATE pState);

	[DllImport("xinput1_4.dll", EntryPoint = "XInputGetBatteryInformation")]
	private static extern uint XInputGetBatteryInformation14(uint dwUserIndex, byte devType, out XINPUT_BATTERY_INFORMATION pBatteryInformation);

	[DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetBatteryInformation")]
	private static extern uint XInputGetBatteryInformation910(uint dwUserIndex, byte devType, out XINPUT_BATTERY_INFORMATION pBatteryInformation);

	private static bool TryGetXInputBattery(uint userIndex, out byte batteryType, out byte batteryLevel)
	{
		try
		{
			uint resState = XInputGetState14(userIndex, out _);
			if (resState == 0)
			{
				uint res = XInputGetBatteryInformation14(userIndex, 0, out var info);
				if (res == 0 && info.BatteryType != 0) // 0 = BATTERY_TYPE_DISCONNECTED
				{
					batteryType = info.BatteryType;
					batteryLevel = info.BatteryLevel;
					return true;
				}
			}
		}
		catch
		{
			try
			{
				uint resState = XInputGetState910(userIndex, out _);
				if (resState == 0)
				{
					uint res = XInputGetBatteryInformation910(userIndex, 0, out var info);
					if (res == 0 && info.BatteryType != 0)
					{
						batteryType = info.BatteryType;
						batteryLevel = info.BatteryLevel;
						return true;
					}
				}
			}
			catch { }
		}
		batteryType = 0;
		batteryLevel = 0;
		return false;
	}

	private void UpdateBatteryStatus()
	{
		SolidColorBrush solidColorBrush = new SolidColorBrush((AppSettings.Current.ThemeIndex == 1) ? System.Windows.Media.Color.FromRgb(30, 30, 30) : System.Windows.Media.Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue));

		// 1. Escaneo nativo y directo de ranuras XInput (0 a 3).
		// XInput es la API oficial y 100% estable de Microsoft para mandos Xbox (Receptor USB Inalámbrico, Cable y Bluetooth).
		// No genera interferencia por radiofrecuencia (RF) ni causa apagados por comandos I/O incompatibles.
		List<int> activeXInputSlots = new List<int>();
		for (uint slot = 0; slot < 4; slot++)
		{
			uint s = 1167; // ERROR_DEVICE_NOT_CONNECTED
			try
			{
				s = XInputGetState14(slot, out _);
			}
			catch
			{
				try
				{
					s = XInputGetState910(slot, out _);
				}
				catch { }
			}

			if (s == 0)
			{
				activeXInputSlots.Add((int)slot);
			}
		}

		// 2. Sincronización inmediata de mandos desconectados:
		// Al apagarse el mando, su slot XInput deja de responder y se elimina al instante.
		foreach (ControllerInfo item in Controllers.ToList())
		{
			if (item.XInputSlot < 0 || !activeXInputSlots.Contains(item.XInputSlot))
			{
				Controllers.Remove(item);
			}
		}

		// 3. Registrar o actualizar mandos XInput activos
		foreach (int slot in activeXInputSlots)
		{
			ControllerInfo? ctrl = Controllers.FirstOrDefault(c => c.XInputSlot == slot);
			if (ctrl == null)
			{
				ctrl = new ControllerInfo
				{
					Id = $"XInput_{slot}",
					XInputSlot = slot,
					Name = Strings.Get("TitleText") + (activeXInputSlots.Count > 1 ? $" {slot + 1}" : ""),
					ConnectedTimestamp = DateTime.UtcNow,
					StatusText = Strings.Get("Searching"),
					Percent = -1.0
				};
				Controllers.Add(ctrl);
			}
		}

		bool hasAnyLowBattery = false;
		double lowestPercent = double.MaxValue;
		string lowestText = "--%";
		SolidColorBrush lowestBrush = solidColorBrush;
		string allSummaries = "";
		bool anyCharging = false;

		if (Controllers.Count == 0)
		{
			TitleText.Text = Strings.Get("NotDetected");
			StatusText.Text = Strings.Get("NotFound");
			StatusText.Visibility = Visibility.Visible;
			CompactPercentText.Text = "--%";
			CompactPercentText.Foreground = solidColorBrush;
			UpdateTrayIconAndTooltip(-1.0, isOnExternalPower: false, isConnected: false, "", Strings.Get("Controller"));
			if (AppSettings.Current.HideUntilLowBattery)
			{
				HideWindow();
			}
			return;
		}

		StatusText.Visibility = Visibility.Collapsed;

		foreach (ControllerInfo controllerInfo in Controllers)
		{
			string ctrlName = controllerInfo.Name;
			double displayPercent = -1.0;
			bool isCharging = false;
			bool isOnExternalPower = false;
			string statusLabel = "";
			System.Windows.Media.Color statusColor;

			if (TryGetXInputBattery((uint)controllerInfo.XInputSlot, out byte xType, out byte xLevel))
			{
				if (xType == 1) // BATTERY_TYPE_WIRED
				{
					isOnExternalPower = true;
					displayPercent = 100.0;
					isCharging = false;
					statusLabel = Strings.Get("UsbCharged");
					statusColor = System.Windows.Media.Color.FromRgb(59, 130, 246);
				}
				else // BATTERY_TYPE_ALKALINE (2), NIMH (3) o UNKNOWN (255)
				{
					isOnExternalPower = false;
					isCharging = false;
					displayPercent = xLevel switch
					{
						3 => 100.0,
						2 => 70.0,
						1 => 35.0,
						_ => 15.0
					};
					controllerInfo.LastWirelessPercent = displayPercent;
					statusLabel = Strings.Get("NotCharged");
					statusColor = (displayPercent <= (double)AppSettings.Current.LowBatteryThreshold)
						? System.Windows.Media.Color.FromRgb(226, 75, 74)
						: ((displayPercent <= 50.0) ? System.Windows.Media.Color.FromRgb(245, 166, 35) : System.Windows.Media.Color.FromRgb(29, 158, 117));
				}
			}
			else
			{
				controllerInfo.StatusText = Strings.Get("Searching");
				continue;
			}

			// Actualizar sonido de conexión / desconexión USB
			if (isOnExternalPower && !controllerInfo.WasOnExternalPower)
			{
				PlayConnectSound();
			}
			else if (!isOnExternalPower && controllerInfo.WasOnExternalPower)
			{
				PlayDisconnectSound();
			}
			controllerInfo.WasOnExternalPower = isOnExternalPower;

			// Actualizar propiedades reactivas del mando
			controllerInfo.Percent = displayPercent;
			controllerInfo.StatusText = statusLabel;
			controllerInfo.BatteryColor = new SolidColorBrush(statusColor);
			controllerInfo.IsCharging = isCharging;

			// Calcular mando más crítico para modo compacto y System Tray
			if (displayPercent >= 0.0 && displayPercent <= lowestPercent)
			{
				lowestPercent = displayPercent;
				lowestText = $"{Math.Round(displayPercent)}%";
				lowestBrush = new SolidColorBrush(statusColor);
			}

			anyCharging |= isCharging;
			allSummaries += $"{ctrlName}: {Math.Round(displayPercent)}% | ";

			// Alerta de batería baja
			if (displayPercent >= 0.0 && displayPercent <= (double)AppSettings.Current.LowBatteryThreshold && !isOnExternalPower)
			{
				hasAnyLowBattery = true;
				if (!controllerInfo.LowBatteryWarned)
				{
					new ToastContentBuilder()
						.AddText(Strings.Get("LowBattery"))
						.AddText(string.Format(Strings.Get("ControllerHas"), ctrlName, Math.Round(displayPercent)))
						.Show();
					StartFlashAlert();
					controllerInfo.LowBatteryWarned = true;
				}
			}
			else
			{
				controllerInfo.LowBatteryWarned = false;
			}
		}

		if (Controllers.Count == 1)
		{
			TitleText.Text = Controllers[0].Name;
		}
		else
		{
			TitleText.Text = $"{Strings.Get("Controllers")} ({Controllers.Count})";
		}

		CompactPercentText.Text = lowestText;
		CompactPercentText.Foreground = lowestBrush;

		if (allSummaries.EndsWith(" | "))
		{
			allSummaries = allSummaries.Substring(0, allSummaries.Length - 3);
		}

		double trayPercent = (lowestPercent != double.MaxValue) ? lowestPercent : -1.0;
		UpdateTrayIconAndTooltip(trayPercent, anyCharging, isConnected: true, anyCharging ? Strings.Get("Charging") : Strings.Get("Charged"), allSummaries);

		if (AppSettings.Current.HideUntilLowBattery)
		{
			if (hasAnyLowBattery)
			{
				ShowWindow();
			}
			else
			{
				HideWindow();
			}
		}
	}

	private async void StartFlashAlert()
	{
		if (_flashTimer != null && _flashTimer.IsEnabled)
		{
			return;
		}
		_flashRemaining = AppSettings.Current.FlashCount * 2;
		_flashColorIndex = 0;
		ShowWindow();
		if (_flashTimer == null)
		{
			_flashTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(400.0)
			};
			_flashTimer.Tick += FlashTick;
		}
		_flashTimer.Start();
		if (!AppSettings.Current.SoundBatteryEnabled)
		{
			return;
		}
		try
		{
			string winMedia = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media", "Windows Notify System Generic.wav");
			if (!File.Exists(winMedia))
			{
				winMedia = @"C:\Windows\Media\Windows Notify System Generic.wav";
			}
			if (File.Exists(winMedia))
			{
				Uri uri = new Uri(winMedia, UriKind.Absolute);
				if (_alertMediaPlayer == null)
				{
					_alertMediaPlayer = new MediaPlayer();
				}
				_alertMediaPlayer.Open(uri);
				for (int round = 0; round < 3; round++)
				{
					for (int i = 0; i < 3; i++)
					{
						if (!AppSettings.Current.SoundBatteryEnabled || _isExiting)
						{
							_alertMediaPlayer.Stop();
							return;
						}
						_alertMediaPlayer.Position = TimeSpan.Zero;
						_alertMediaPlayer.Volume = 0.05 + (double)i * 0.1;
						_alertMediaPlayer.Play();
						await Task.Delay(500);
					}
					if (round < 2)
					{
						await Task.Delay(2000);
					}
				}
			}
		}
		catch
		{
		}
	}

	private void FlashTick(object? sender, EventArgs e)
	{
		if (_flashRemaining <= 0)
		{
			DispatcherTimer? flashTimer = _flashTimer;
			if (flashTimer != null)
			{
				flashTimer.Stop();
			}
			FullModeBorder.Background = _originalBackground;
			CompactModeBorder.Background = _originalBackground;
			return;
		}
		if (_flashRemaining % 2 == 0)
		{
			System.Windows.Media.Color color = FlashColors[_flashColorIndex % FlashColors.Length];
			SolidColorBrush background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(230, color.R, color.G, color.B));
			FullModeBorder.Background = background;
			CompactModeBorder.Background = background;
			_flashColorIndex++;
		}
		else
		{
			FullModeBorder.Background = _originalBackground;
			CompactModeBorder.Background = _originalBackground;
		}
		_flashRemaining--;
	}

	private void StartBreathingOpacity()
	{
		if (CompactModeBorder.Visibility != Visibility.Visible || !base.IsVisible)
		{
			return;
		}

		if (base.IsMouseOver)
		{
			BeginAnimation(UIElement.OpacityProperty, null);
			base.Opacity = 1.0;
			return;
		}

		if (Controllers.Any((ControllerInfo c) => c.Percent <= (double)AppSettings.Current.LowBatteryThreshold && !c.IsCharging))
		{
			BeginAnimation(UIElement.OpacityProperty, null);
			base.Opacity = 1.0;
			return;
		}

		DoubleAnimation breathingAnimation = new DoubleAnimation
		{
			From = 0.28,
			To = 0.48,
			Duration = TimeSpan.FromSeconds(5.0),
			AutoReverse = true,
			RepeatBehavior = RepeatBehavior.Forever,
			EasingFunction = new SineEase
			{
				EasingMode = EasingMode.EaseInOut
			}
		};
		BeginAnimation(UIElement.OpacityProperty, breathingAnimation);
	}

	private void ToggleMode_Click(object sender, RoutedEventArgs e)
	{
		if (FullModeBorder.Visibility == Visibility.Visible)
		{
			FullModeBorder.Visibility = Visibility.Collapsed;
			CompactModeBorder.Visibility = Visibility.Visible;
			base.SizeToContent = SizeToContent.Manual;
			base.Width = _compactSize;
			base.Height = _compactSize;
			CompactPercentText.FontSize = _compactSize * 0.3;
			_anchorLeft = base.Left;
			_anchorTop = base.Top;
			_orbitAngle = 0.0;
			StartBreathingOpacity();
			TriggerShineAnimation();
		}
		else
		{
			FullModeBorder.Visibility = Visibility.Visible;
			CompactModeBorder.Visibility = Visibility.Collapsed;
			base.SizeToContent = SizeToContent.Height;
			base.Width = 260.0;
			BeginAnimation(UIElement.OpacityProperty, null);
			base.Opacity = 1.0;
			_anchorLeft = double.NaN;
			_anchorTop = double.NaN;
			BeginAnimation(Window.LeftProperty, null);
			BeginAnimation(Window.TopProperty, null);
			if (TextShiftTransform != null)
			{
				TextShiftTransform.BeginAnimation(TranslateTransform.XProperty, null);
				TextShiftTransform.BeginAnimation(TranslateTransform.YProperty, null);
				TextShiftTransform.X = 0;
				TextShiftTransform.Y = 0;
			}
		}
	}

	private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
	{
		if (CompactModeBorder.Visibility == Visibility.Visible)
		{
			double num = Math.Max(e.HorizontalChange, e.VerticalChange);
			double val = Math.Max(60.0, base.Width + num);
			val = (_compactSize = (base.Height = (base.Width = Math.Min(300.0, val))));
			CompactPercentText.FontSize = val * 0.3;
		}
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		HideWindow();
	}

	private void MinimizeButton_Click(object sender, RoutedEventArgs e)
	{
		HideWindow();
	}

	private void CompactMode_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ClickCount == 2)
		{
			BurnInTimer_Tick(null, EventArgs.Empty);
		}
	}

	private void TestShineButton_Click(object sender, RoutedEventArgs e)
	{
		BurnInTimer_Tick(null, EventArgs.Empty);
	}

	private void TriggerShineAnimation()
	{
		try
		{
			if (ShineTransform != null)
			{
				double fromValue = (0.0 - base.Width) * 1.5;
				double toValue = base.Width * 2.0;
				DoubleAnimation animation = new DoubleAnimation(fromValue, toValue, TimeSpan.FromSeconds(1.3))
				{
					EasingFunction = new CubicEase
					{
						EasingMode = EasingMode.EaseInOut
					}
				};
				ShineTransform.BeginAnimation(TranslateTransform.XProperty, animation);
			}
		}
		catch
		{
		}
	}

	private void BurnInTimer_Tick(object? sender, EventArgs e)
	{
		if (CompactModeBorder.Visibility == Visibility.Visible && base.IsVisible)
		{
			if (double.IsNaN(_anchorLeft))
			{
				_anchorLeft = base.Left;
				_anchorTop = base.Top;
			}

			// Órbita circular suave (R = 8px)
			_orbitAngle += Math.PI / 4.0;
			if (_orbitAngle >= Math.PI * 2.0)
			{
				_orbitAngle -= Math.PI * 2.0;
			}

			double radius = 8.0;
			double offsetX = Math.Round(radius * Math.Cos(_orbitAngle), 2);
			double offsetY = Math.Round(radius * Math.Sin(_orbitAngle), 2);

			Rect workArea = SystemParameters.WorkArea;
			double toLeft = Math.Clamp(_anchorLeft + offsetX, workArea.Left, workArea.Right - base.Width);
			double toTop = Math.Clamp(_anchorTop + offsetY, workArea.Top, workArea.Bottom - base.Height);

			DoubleAnimation animLeft = new DoubleAnimation(toLeft, TimeSpan.FromSeconds(7.0))
			{
				EasingFunction = new QuadraticEase
				{
					EasingMode = EasingMode.EaseInOut
				}
			};
			DoubleAnimation animTop = new DoubleAnimation(toTop, TimeSpan.FromSeconds(7.0))
			{
				EasingFunction = new QuadraticEase
				{
					EasingMode = EasingMode.EaseInOut
				}
			};
			BeginAnimation(Window.LeftProperty, animLeft);
			BeginAnimation(Window.TopProperty, animTop);

			try
			{
				if (TextShiftTransform != null)
				{
					// Micro-desplazamiento de texto (+- 1.0px) para mitigar quemado de subpíxeles
					double textX = Math.Round(1.0 * Math.Cos(_orbitAngle * 3.0), 2);
					double textY = Math.Round(1.0 * Math.Sin(_orbitAngle * 3.0), 2);
					DoubleAnimation animTextX = new DoubleAnimation(textX, TimeSpan.FromSeconds(4.0))
					{
						EasingFunction = new QuadraticEase
						{
							EasingMode = EasingMode.EaseInOut
						}
					};
					DoubleAnimation animTextY = new DoubleAnimation(textY, TimeSpan.FromSeconds(4.0))
					{
						EasingFunction = new QuadraticEase
						{
							EasingMode = EasingMode.EaseInOut
						}
					};
					TextShiftTransform.BeginAnimation(TranslateTransform.XProperty, animTextX);
					TextShiftTransform.BeginAnimation(TranslateTransform.YProperty, animTextY);
				}
			}
			catch
			{
			}

			TriggerShineAnimation();

			if (!base.IsMouseOver && !Controllers.Any((ControllerInfo c) => c.Percent <= (double)AppSettings.Current.LowBatteryThreshold && !c.IsCharging))
			{
				StartBreathingOpacity();
			}
		}
		else
		{
			_anchorLeft = double.NaN;
			_anchorTop = double.NaN;
			BeginAnimation(Window.LeftProperty, null);
			BeginAnimation(Window.TopProperty, null);
		}
	}

	private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ButtonState == MouseButtonState.Pressed)
		{
			BeginAnimation(Window.LeftProperty, null);
			BeginAnimation(Window.TopProperty, null);
			DragMove();
			_anchorLeft = base.Left;
			_anchorTop = base.Top;
		}
	}

	private void Window_MouseEnter(object sender, MouseEventArgs e)
	{
		if (CompactModeBorder.Visibility == Visibility.Visible)
		{
			BeginAnimation(UIElement.OpacityProperty, null);
			DoubleAnimation animation = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.15));
			BeginAnimation(UIElement.OpacityProperty, animation);
		}
	}

	private void Window_MouseLeave(object sender, MouseEventArgs e)
	{
		if (CompactModeBorder.Visibility != Visibility.Visible)
		{
			return; // En modo normal nunca se atenúa
		}

		if (Controllers.Any((ControllerInfo c) => c.Percent <= (double)AppSettings.Current.LowBatteryThreshold && !c.IsCharging))
		{
			BeginAnimation(UIElement.OpacityProperty, null);
			DoubleAnimation animation = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.2));
			BeginAnimation(UIElement.OpacityProperty, animation);
		}
		else
		{
			StartBreathingOpacity();
		}
	}

	}
