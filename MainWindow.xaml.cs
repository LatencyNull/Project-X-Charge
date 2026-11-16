using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using XboxBatteryMonitor.Services;

namespace XboxBatteryMonitor;

public partial class MainWindow : Window
{
	private readonly DispatcherTimer _timer;
	private readonly DispatcherTimer _burnInTimer;
	private readonly HotKeyService _hotKeyService;

	private CancellationTokenSource? _soundCts;
	private bool _isExiting;
	private bool _isHiding;
	private double _compactSize = 80.0;
	private double _anchorLeft = double.NaN;
	private double _anchorTop = double.NaN;
	private double _orbitAngle;

	private double _lastTrayPercent = -999.0;
	private bool _lastTrayIsOnExternalPower;
	private bool _lastTrayIsConnected;
	private int _lastTrayThreshold = -1;

	private DispatcherTimer? _flashTimer;
	private DispatcherTimer? _opacityMeterTimer;
	private DispatcherTimer? _saveSettingsTimer;
	private int _flashRemaining;
	private int _flashColorIndex;
	private System.Windows.Media.Brush? _originalBackground;

	private static readonly SolidColorBrush[] FlashBrushes = new SolidColorBrush[]
	{
		CreateFrozenBrush(226, 75, 74),
		CreateFrozenBrush(245, 166, 35),
		CreateFrozenBrush(250, 204, 21),
		CreateFrozenBrush(226, 75, 74),
		CreateFrozenBrush(255, 96, 96),
		CreateFrozenBrush(245, 166, 35)
	};

	private static SolidColorBrush CreateFrozenBrush(byte r, byte g, byte b, byte a = 230)
	{
		SolidColorBrush brush = new(System.Windows.Media.Color.FromArgb(a, r, g, b));
		brush.Freeze();
		return brush;
	}

	public static readonly DependencyProperty TrackBarBrushProperty =
		DependencyProperty.Register(nameof(TrackBarBrush), typeof(System.Windows.Media.Brush), typeof(MainWindow),
			new PropertyMetadata(CreateFrozenBrush(51, 51, 51, 255)));

	public System.Windows.Media.Brush TrackBarBrush
	{
		get => (System.Windows.Media.Brush)GetValue(TrackBarBrushProperty);
		set => SetValue(TrackBarBrushProperty, value);
	}

	public static readonly DependencyProperty SubtitleBrushProperty =
		DependencyProperty.Register(nameof(SubtitleBrush), typeof(System.Windows.Media.Brush), typeof(MainWindow),
			new PropertyMetadata(CreateFrozenBrush(153, 153, 153, 255)));

	public System.Windows.Media.Brush SubtitleBrush
	{
		get => (System.Windows.Media.Brush)GetValue(SubtitleBrushProperty);
		set => SetValue(SubtitleBrushProperty, value);
	}

	public ObservableCollection<ControllerInfo> Controllers { get; } = new();

	public MainWindow()
	{
		InitializeComponent();
		_hotKeyService = new HotKeyService(this);
		ApplyLanguage();
		_originalBackground = FullModeBorder.Background;
		SetupTray();

		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(AppSettings.Current.PollIntervalSeconds)
		};
		_timer.Tick += (s, e) => UpdateBatteryStatus();
		_timer.Start();

		_burnInTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(120.0)
		};
		_burnInTimer.Tick += BurnInTimer_Tick;
		_burnInTimer.Start();

		// Respetar tema guardado en AppSettings.Current
		SetTheme(AppSettings.Current.ThemeIndex);

		SourceInitialized += MainWindow_SourceInitialized;
		UpdateBatteryStatus();
	}

	private bool _isInitialized;
	private const int WM_DISPLAYCHANGE = 0x007E;
	private const int WM_DPICHANGED = 0x02E0;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool ChangeWindowMessageFilter(uint message, uint dwFlag);
	private const uint MSGFLT_ADD = 1;

	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	private static extern uint RegisterWindowMessage(string lpString);

	private static readonly uint WmShowAppInstance = RegisterWindowMessage("XCHARGE_SHOW_APP");

	private void MainWindow_SourceInitialized(object? sender, EventArgs e)
	{
		EnsureWindowInitialized();
		nint handle = new WindowInteropHelper(this).Handle;
		if (handle != IntPtr.Zero)
		{
			HwndSource? source = HwndSource.FromHwnd(handle);
			source?.AddHook(WndProc);
			try
			{
				ChangeWindowMessageFilter(WmShowAppInstance, MSGFLT_ADD);
			}
			catch { }
		}
	}

	private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (msg == WM_DISPLAYCHANGE || msg == WM_DPICHANGED)
		{
			ClampWindowToCurrentMonitor();
		}
		else if (msg == WmShowAppInstance)
		{
			ShowWindow();
			handled = true;
		}
		return IntPtr.Zero;
	}

	private void Window_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			HideWindow();
			e.Handled = true;
		}
	}

	private void EnsureWindowInitialized()
	{
		if (_isInitialized) return;
		_isInitialized = true;

		// 1. Restaurar tamaño personalizado si existe
		if (AppSettings.Current.CompactSize >= 60.0 && AppSettings.Current.CompactSize <= 300.0)
		{
			_compactSize = AppSettings.Current.CompactSize;
		}

		// 2. Restaurar posición previa de ventana si está guardada (validando que no quede fuera de pantalla)
		if (AppSettings.Current.WindowLeft.HasValue && AppSettings.Current.WindowTop.HasValue)
		{
			double left = AppSettings.Current.WindowLeft.Value;
			double top = AppSettings.Current.WindowTop.Value;
			double width = AppSettings.Current.IsCompactMode ? _compactSize : 260.0;
			double height = AppSettings.Current.IsCompactMode ? _compactSize : 150.0;

			Rect virtualScreen = new(
				SystemParameters.VirtualScreenLeft,
				SystemParameters.VirtualScreenTop,
				SystemParameters.VirtualScreenWidth,
				SystemParameters.VirtualScreenHeight);

			Rect savedRect = new(left, top, width, height);
			if (virtualScreen.IntersectsWith(savedRect))
			{
				Left = left;
				Top = top;
			}
			else
			{
				// Si el monitor secundario fue desconectado, devolver al área de trabajo principal visible
				Rect workArea = SystemParameters.WorkArea;
				Left = Math.Max(workArea.Left, Math.Min(workArea.Right - width, left));
				Top = Math.Max(workArea.Top, Math.Min(workArea.Bottom - height, top));
			}
		}

		// 3. Restaurar Modo Compacto si fue el último modo activo
		if (AppSettings.Current.IsCompactMode)
		{
			SwitchToCompactMode();
		}

		// 4. Registrar atajo de teclado global si está habilitado
		if (AppSettings.Current.HotKeyEnabled)
		{
			bool registered = _hotKeyService.Register(ToggleWindow);
			if (!registered)
			{
				AppSettings.Current.HotKeyEnabled = false;
				if (HotKeyItem != null) HotKeyItem.IsChecked = false;
			}
		}

		ApplyClickThrough();
	}

	public void SaveCurrentState()
	{
		double currentLeft = (WindowState == WindowState.Minimized) ? RestoreBounds.Left : Left;
		double currentTop = (WindowState == WindowState.Minimized) ? RestoreBounds.Top : Top;

		if (currentLeft > -10000 && currentTop > -10000)
		{
			AppSettings.Current.WindowLeft = (!double.IsNaN(_anchorLeft) && CompactModeBorder.Visibility == Visibility.Visible)
				? _anchorLeft
				: currentLeft;
			AppSettings.Current.WindowTop = (!double.IsNaN(_anchorTop) && CompactModeBorder.Visibility == Visibility.Visible)
				? _anchorTop
				: currentTop;
		}
		AppSettings.Current.IsCompactMode = (CompactModeBorder.Visibility == Visibility.Visible);
		AppSettings.Current.CompactSize = _compactSize;
		AppSettings.Current.Save();
	}

	public void ClampWindowToCurrentMonitor()
	{
		Rect workArea = WindowStyleService.GetCurrentMonitorWorkArea(this);
		double curWidth = (CompactModeBorder.Visibility == Visibility.Visible) ? _compactSize : 260.0;
		double curHeight = (CompactModeBorder.Visibility == Visibility.Visible) ? _compactSize : (ActualHeight > 50.0 ? ActualHeight : 150.0);

		double clampedLeft = Math.Clamp(Left, workArea.Left, Math.Max(workArea.Left, workArea.Right - curWidth));
		double clampedTestTop = Math.Clamp(Top, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - curHeight));

		if (Math.Abs(clampedLeft - Left) > 1.0 || Math.Abs(clampedTestTop - Top) > 1.0)
		{
			Left = clampedLeft;
			Top = clampedTestTop;
			if (CompactModeBorder.Visibility == Visibility.Visible)
			{
				_anchorLeft = Left;
				_anchorTop = Top;
			}
			RequestSaveSettings();
		}
	}

	private void RequestSaveSettings()
	{
		if (_saveSettingsTimer == null)
		{
			_saveSettingsTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(500)
			};
			_saveSettingsTimer.Tick += (s, e) =>
			{
				_saveSettingsTimer?.Stop();
				AppSettings.Current.Save();
			};
		}
		_saveSettingsTimer.Stop();
		_saveSettingsTimer.Start();
	}

	private void SetupTray()
	{
		if (TrayIcon == null) return;

		// 1. Temas
		DarkThemeItem.IsChecked = (AppSettings.Current.ThemeIndex == 0);
		LightThemeItem.IsChecked = (AppSettings.Current.ThemeIndex == 1);
		TransparentThemeItem.IsChecked = (AppSettings.Current.ThemeIndex == 2);

		// 2. Idiomas
		string curLang = Strings.CurrentLanguage;
		LangEnItem.IsChecked = (curLang == "en");
		LangEsItem.IsChecked = (curLang == "es");
		LangFrItem.IsChecked = (curLang == "fr");
		LangDeItem.IsChecked = (curLang == "de");
		LangPtItem.IsChecked = (curLang == "pt");
		LangZhItem.IsChecked = (curLang == "zh");
		LangRuItem.IsChecked = (curLang == "ru");

		// 3. Sonidos
		UsbSoundItem.IsChecked = AppSettings.Current.SoundUsbEnabled;
		BatterySoundItem.IsChecked = AppSettings.Current.SoundBatteryEnabled;

		// 4. Umbrales
		int threshold = AppSettings.Current.LowBatteryThreshold;
		Th10Item.IsChecked = (threshold == 10);
		Th15Item.IsChecked = (threshold == 15);
		Th20Item.IsChecked = (threshold == 20);

		// 5. Frecuencia de sondeo
		int pollSec = AppSettings.Current.PollIntervalSeconds;
		Poll1sItem.IsChecked = (pollSec == 1);
		Poll2sItem.IsChecked = (pollSec == 2);
		Poll3sItem.IsChecked = (pollSec == 3);
		Poll5sItem.IsChecked = (pollSec == 5);

		// 6. Opciones avanzadas
		GhostModeItem.IsChecked = AppSettings.Current.ClickThroughEnabled;
		AntiBurnInItem.IsChecked = AppSettings.Current.AntiBurnInEnabled;
		HotKeyItem.IsChecked = AppSettings.Current.HotKeyEnabled;
		StartupItem.IsChecked = StartupService.IsStartupEnabled();
		StartInTrayItem.IsChecked = AppSettings.Current.StartInTray;
		HideUntilLowItem.IsChecked = AppSettings.Current.HideUntilLowBattery;
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
		if (sender is MenuItem menuItem && menuItem.Tag is string tag && int.TryParse(tag, out int themeIndex))
		{
			SetTheme(themeIndex);
			AppSettings.Current.ThemeIndex = themeIndex;
			AppSettings.Current.Save();
			SetupTray();
		}
	}

	private void LanguageItem_Click(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem menuItem && menuItem.Tag is string langCode)
		{
			if (Strings.SetLanguage(langCode))
			{
				ApplyLanguage();
				SetupTray();
				UpdateBatteryStatus();
			}
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
		if (sender is MenuItem menuItem && menuItem.Tag is string tag && int.TryParse(tag, out int threshold))
		{
			AppSettings.Current.LowBatteryThreshold = threshold;
			AppSettings.Current.Save();
			SetupTray();
			UpdateBatteryStatus();
		}
	}

	private void PollIntervalItem_Click(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem menuItem && menuItem.Tag is string tag && int.TryParse(tag, out int interval))
		{
			AppSettings.Current.PollIntervalSeconds = interval;
			AppSettings.Current.Save();
			_timer.Interval = TimeSpan.FromSeconds(interval);
			SetupTray();
		}
	}

	private void GhostModeItem_Click(object sender, RoutedEventArgs e)
	{
		AppSettings.Current.ClickThroughEnabled = GhostModeItem.IsChecked;
		AppSettings.Current.Save();
		ApplyClickThrough();
	}

	private void HotKeyItem_Click(object sender, RoutedEventArgs e)
	{
		if (HotKeyItem.IsChecked)
		{
			bool registered = _hotKeyService.Register(ToggleWindow);
			if (!registered)
			{
				HotKeyItem.IsChecked = false;
				AppSettings.Current.HotKeyEnabled = false;
				AppSettings.Current.Save();
				MessageBox.Show(Strings.Get("HotKeyConflict"), "X-Charge", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			AppSettings.Current.HotKeyEnabled = true;
			AppSettings.Current.Save();
		}
		else
		{
			AppSettings.Current.HotKeyEnabled = false;
			AppSettings.Current.Save();
			_hotKeyService.Unregister();
		}
	}

	private void StartupItem_Click(object sender, RoutedEventArgs e)
	{
		StartupService.SetStartup(StartupItem.IsChecked);
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

	private void AntiBurnInItem_Click(object sender, RoutedEventArgs e)
	{
		AppSettings.Current.AntiBurnInEnabled = AntiBurnInItem.IsChecked;
		AppSettings.Current.Save();
		if (!AppSettings.Current.AntiBurnInEnabled)
		{
			BeginAnimation(LeftProperty, null);
			BeginAnimation(TopProperty, null);
			if (TextShiftTransform != null)
			{
				TextShiftTransform.BeginAnimation(TranslateTransform.XProperty, null);
				TextShiftTransform.BeginAnimation(TranslateTransform.YProperty, null);
				TextShiftTransform.X = 0;
				TextShiftTransform.Y = 0;
			}
		}
	}

	private void ApplyClickThrough()
	{
		bool isGhostActive = AppSettings.Current.ClickThroughEnabled && (CompactModeBorder.Visibility == Visibility.Visible);
		WindowStyleService.SetClickThrough(this, isGhostActive);

		if (GhostIndicator != null)
		{
			GhostIndicator.Visibility = isGhostActive ? Visibility.Visible : Visibility.Collapsed;
			if (isGhostActive)
			{
				GhostIndicator.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 56, 189, 248));
				GhostIndicator.ToolTip = Strings.Get("GhostLockedTip");
				FullModeButton.Visibility = Visibility.Collapsed;
				ResizeThumb.Visibility = Visibility.Collapsed;
			}
			else
			{
				FullModeButton.Visibility = Visibility.Visible;
				ResizeThumb.Visibility = Visibility.Visible;
			}
		}
	}

	private void SetTheme(int index)
	{
		System.Windows.Media.Color bgColor;
		System.Windows.Media.Color borderColor;
		System.Windows.Media.Color textColor;
		System.Windows.Media.Color subtitleColor;
		System.Windows.Media.Color iconColor;
		System.Windows.Media.Color trackBarColor;

		switch (index)
		{
			case 1: // Claro
				bgColor = System.Windows.Media.Color.FromArgb(230, 240, 240, 240);
				borderColor = System.Windows.Media.Color.FromRgb(200, 200, 200);
				textColor = System.Windows.Media.Color.FromRgb(30, 30, 30);
				subtitleColor = System.Windows.Media.Color.FromRgb(110, 110, 110);
				iconColor = System.Windows.Media.Color.FromRgb(80, 80, 80);
				trackBarColor = System.Windows.Media.Color.FromRgb(215, 215, 215);
				break;
			case 2: // Transparente
				bgColor = System.Windows.Media.Color.FromArgb(120, 15, 15, 15);
				borderColor = System.Windows.Media.Color.FromArgb(80, 255, 255, 255);
				textColor = System.Windows.Media.Color.FromRgb(240, 240, 240);
				subtitleColor = System.Windows.Media.Color.FromRgb(170, 170, 170);
				iconColor = System.Windows.Media.Color.FromRgb(200, 200, 200);
				trackBarColor = System.Windows.Media.Color.FromArgb(150, 40, 40, 40);
				break;
			default: // Oscuro
				bgColor = System.Windows.Media.Color.FromArgb(230, 32, 32, 32);
				borderColor = System.Windows.Media.Color.FromRgb(64, 64, 64);
				textColor = System.Windows.Media.Color.FromRgb(220, 220, 220);
				subtitleColor = System.Windows.Media.Color.FromRgb(153, 153, 153);
				iconColor = System.Windows.Media.Color.FromRgb(170, 170, 170);
				trackBarColor = System.Windows.Media.Color.FromRgb(51, 51, 51);
				break;
		}

		SolidColorBrush bgBrush = new(bgColor); bgBrush.Freeze();
		SolidColorBrush borderBrush = new(borderColor); borderBrush.Freeze();
		SolidColorBrush textBrush = new(textColor); textBrush.Freeze();
		SolidColorBrush subBrush = new(subtitleColor); subBrush.Freeze();
		SolidColorBrush iconBrush = new(iconColor); iconBrush.Freeze();
		SolidColorBrush trackBrush = new(trackBarColor); trackBrush.Freeze();

		Foreground = textBrush;
		TrackBarBrush = trackBrush;
		SubtitleBrush = subBrush;

		FullModeBorder.Background = bgBrush;
		FullModeBorder.BorderBrush = borderBrush;
		CompactModeBorder.Background = bgBrush;
		CompactModeBorder.BorderBrush = borderBrush;
		_originalBackground = bgBrush;

		TitleText.Foreground = textBrush;
		CompactPercentText.Foreground = textBrush;
		CompactButton.Foreground = iconBrush;
		MinimizeButton.Foreground = iconBrush;
		CloseButton.Foreground = iconBrush;
		FullModeButton.Foreground = iconBrush;

		UpdateBatteryStatus();
	}

	private void ApplyLanguage()
	{
		TitleText.Text = Strings.Get("TitleText");
		CompactButton.ToolTip = Strings.Get("CompactMode");
		MinimizeButton.ToolTip = Strings.Get("MinimizeTray");
		CloseButton.ToolTip = Strings.Get("CloseTray");
		FullModeButton.ToolTip = Strings.Get("NormalMode");
		StatusText.Text = Strings.Get("Searching");
		CompactModeBorder.ToolTip = Strings.Get("CompactTip");

		if (TrayIcon != null)
		{
			TrayIcon.ToolTipText = "X-Charge";
		}

		ToggleWindowItem.Header = (Visibility == Visibility.Visible && Opacity > 0.0) ? Strings.Get("Hide") : Strings.Get("Show");
		ThemesMenuItem.Header = Strings.Get("Themes");
		DarkThemeItem.Header = Strings.Get("ThemeDark");
		LightThemeItem.Header = Strings.Get("ThemeLight");
		TransparentThemeItem.Header = Strings.Get("ThemeTransparent");
		LanguagesMenuItem.Header = Strings.Get("Language");
		SoundsMenuItem.Header = Strings.Get("PlaySounds");
		UsbSoundItem.Header = Strings.Get("SoundUsb");
		BatterySoundItem.Header = Strings.Get("SoundBattery");
		ThresholdMenuItem.Header = Strings.Get("BatteryOptions");
		PollIntervalMenuItem.Header = Strings.Get("PollInterval");
		Poll1sItem.Header = Strings.Get("Poll1s");
		Poll2sItem.Header = Strings.Get("Poll2s");
		Poll3sItem.Header = Strings.Get("Poll3s");
		Poll5sItem.Header = Strings.Get("Poll5s");
		GhostModeItem.Header = Strings.Get("GhostMode");
		AntiBurnInItem.Header = Strings.Get("AntiBurnIn");
		HotKeyItem.Header = Strings.Get("GlobalHotKey");
		StartupItem.Header = Strings.Get("StartWindows");
		StartInTrayItem.Header = Strings.Get("StartInTray");
		HideUntilLowItem.Header = Strings.Get("HideUntilLow");
		ExitItem.Header = Strings.Get("Exit");

		UpdateControllerNames();
	}

	private void UpdateControllerNames()
	{
		string baseName = Strings.Get("TitleText");
		for (int i = 0; i < Controllers.Count; i++)
		{
			Controllers[i].Name = (Controllers.Count > 1)
				? $"{baseName} {Controllers[i].XInputSlot + 1}"
				: baseName;
		}
	}

	private void UpdateTrayIconAndTooltip(double percent, bool isOnExternalPower, bool isConnected, string statusText, string summaryOrName)
	{
		if (TrayIcon == null) return;

		int threshold = AppSettings.Current.LowBatteryThreshold;
		bool stateChanged = Math.Abs(_lastTrayPercent - percent) > 0.1
			|| _lastTrayIsOnExternalPower != isOnExternalPower
			|| _lastTrayIsConnected != isConnected
			|| _lastTrayThreshold != threshold
			|| TrayIcon.Icon == null;

		if (stateChanged)
		{
			Icon? oldIcon = TrayIcon.Icon;
			TrayIcon.Icon = TrayIconService.GenerateBatteryIcon(percent, isOnExternalPower, isConnected, threshold);
			oldIcon?.Dispose();

			_lastTrayPercent = percent;
			_lastTrayIsOnExternalPower = isOnExternalPower;
			_lastTrayIsConnected = isConnected;
			_lastTrayThreshold = threshold;
		}

		string text;
		if (!isConnected)
		{
			text = $"X-Charge: {Strings.Get("NotFound")}";
		}
		else if (Controllers.Count > 1)
		{
			text = $"X-Charge: {summaryOrName}";
		}
		else if (percent >= 0.0)
		{
			text = $"X-Charge: {summaryOrName} ({statusText})";
		}
		else
		{
			text = $"X-Charge: {summaryOrName} - {Strings.Get("NoData")}";
		}

		TrayIcon.ToolTipText = (text.Length > 63) ? text.Substring(0, 60) + "..." : text;
	}

	public void ShowWindow()
	{
		_isHiding = false;

		double targetOpacity = (CompactModeBorder.Visibility == Visibility.Visible)
			? AppSettings.Current.CompactOpacity
			: 1.0;

		if (Visibility != Visibility.Visible)
		{
			Opacity = 0.0;
			Show();
		}
		WindowState = WindowState.Normal;
		Activate();
		Topmost = false;
		Topmost = true;

		ApplyClickThrough();

		DoubleAnimation animation = new(targetOpacity, TimeSpan.FromSeconds(0.2));
		animation.Completed += (s, e) =>
		{
			if (!_isHiding)
			{
				BeginAnimation(OpacityProperty, null);
				Opacity = targetOpacity;
			}
		};
		BeginAnimation(OpacityProperty, animation);

		if (!_burnInTimer.IsEnabled)
		{
			_burnInTimer.Start();
		}

		if (ToggleWindowItem != null)
		{
			ToggleWindowItem.Header = Strings.Get("Hide");
		}
	}

	private void HideWindow()
	{
		if (Visibility != Visibility.Visible || _isHiding) return;

		_isHiding = true;
		_burnInTimer.Stop();

		DoubleAnimation animation = new(0.0, TimeSpan.FromSeconds(0.2));
		animation.Completed += (s, e) =>
		{
			if (_isHiding && !_isExiting)
			{
				Hide();
				BeginAnimation(OpacityProperty, null);
			}
		};
		BeginAnimation(OpacityProperty, animation);

		if (ToggleWindowItem != null)
		{
			ToggleWindowItem.Header = Strings.Get("Show");
		}
	}

	private void ToggleWindow()
	{
		if (Visibility == Visibility.Visible && Opacity > 0.0 && !_isHiding)
		{
			HideWindow();
		}
		else
		{
			ShowWindow();
		}
	}

	private void ExitApp()
	{
		_isExiting = true;
		_timer.Stop();
		_burnInTimer.Stop();
		_saveSettingsTimer?.Stop();

		SaveCurrentState();

		_flashTimer?.Stop();
		_soundCts?.Cancel();
		SoundService.StopAlert();
		SoundService.Dispose();
		_hotKeyService.Dispose();

		if (TrayIcon != null)
		{
			TrayIcon.Visibility = Visibility.Collapsed;
			TrayIcon.Dispose();
		}

		Application.Current.Shutdown();
	}

	private void UpdateBatteryStatus()
	{
		SolidColorBrush defaultTextBrush = new((AppSettings.Current.ThemeIndex == 1)
			? System.Windows.Media.Color.FromRgb(30, 30, 30)
			: System.Windows.Media.Color.FromRgb(255, 255, 255));
		defaultTextBrush.Freeze();

		List<int> activeSlots = XInputService.GetConnectedSlots();
		bool controllerListChanged = false;

		// Sincronizar desconexiones de mandos
		foreach (ControllerInfo item in Controllers.ToList())
		{
			if (item.XInputSlot < 0 || !activeSlots.Contains(item.XInputSlot))
			{
				Controllers.Remove(item);
				controllerListChanged = true;
			}
		}

		// Registrar o actualizar mandos XInput activos
		foreach (int slot in activeSlots)
		{
			ControllerInfo? ctrl = Controllers.FirstOrDefault(c => c.XInputSlot == slot);
			if (ctrl == null)
			{
				ctrl = new ControllerInfo
				{
					XInputSlot = slot,
					Name = Strings.Get("TitleText"),
					StatusText = Strings.Get("Searching"),
					Percent = -1.0
				};
				Controllers.Add(ctrl);
				controllerListChanged = true;
			}
		}

		if (controllerListChanged)
		{
			UpdateControllerNames();
		}

		bool hasAnyLowBattery = false;
		double lowestPercent = double.MaxValue;
		int criticalSlot = -1;
		string lowestText = "--%";
		SolidColorBrush lowestBrush = defaultTextBrush;
		string allSummaries = "";
		bool anyExternalPower = false;

		if (Controllers.Count == 0)
		{
			TitleText.Text = Strings.Get("NotDetected");
			StatusText.Text = Strings.Get("NotFound");
			StatusText.Visibility = Visibility.Visible;
			CompactPercentText.Text = "--%";
			CompactPercentText.Foreground = defaultTextBrush;
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
			bool isOnExternalPower = false;
			string statusLabel;
			System.Windows.Media.Color statusColor;

			if (XInputService.TryGetBattery(controllerInfo.XInputSlot, out byte xType, out byte xLevel))
			{
				if (xType == XInputService.BATTERY_TYPE_WIRED)
				{
					isOnExternalPower = true;
					displayPercent = 100.0;
					statusLabel = Strings.Get("UsbCharged");
					statusColor = System.Windows.Media.Color.FromRgb(59, 130, 246);
				}
				else if (xType == XInputService.BATTERY_TYPE_UNKNOWN)
				{
					isOnExternalPower = false;
					displayPercent = -1.0;
					statusLabel = Strings.Get("NoData");
					statusColor = System.Windows.Media.Color.FromRgb(150, 150, 150);
				}
				else
				{
					isOnExternalPower = false;
					displayPercent = xLevel switch
					{
						XInputService.BATTERY_LEVEL_FULL => 100.0,
						XInputService.BATTERY_LEVEL_MEDIUM => 65.0,
						XInputService.BATTERY_LEVEL_LOW => 25.0,
						_ => 5.0
					};
					statusLabel = Strings.Get("NotCharged");
					statusColor = (displayPercent <= AppSettings.Current.LowBatteryThreshold)
						? System.Windows.Media.Color.FromRgb(226, 75, 74)
						: ((displayPercent <= 50.0) ? System.Windows.Media.Color.FromRgb(245, 166, 35) : System.Windows.Media.Color.FromRgb(29, 158, 117));
				}
			}
			else
			{
				controllerInfo.StatusText = Strings.Get("Searching");
				continue;
			}

			// Notificaciones de sonido al conectar/desconectar USB
			if (isOnExternalPower && !controllerInfo.WasOnExternalPower)
			{
				if (controllerInfo.HasCheckedExternalPowerOnce)
				{
					SoundService.PlayConnectSound();
				}
				// Si estaba sonando la alarma de batería baja, cancelarla y silenciarla de inmediato
				_soundCts?.Cancel();
				SoundService.StopAlert();

				// Detener parpadeo visual de alarma y restaurar fondo original
				if (_flashTimer != null && _flashTimer.IsEnabled)
				{
					_flashTimer.Stop();
					FullModeBorder.Background = _originalBackground;
					CompactModeBorder.Background = _originalBackground;
				}
			}
			else if (!isOnExternalPower && controllerInfo.WasOnExternalPower)
			{
				if (controllerInfo.HasCheckedExternalPowerOnce)
				{
					SoundService.PlayDisconnectSound();
				}
			}
			controllerInfo.WasOnExternalPower = isOnExternalPower;
			controllerInfo.HasCheckedExternalPowerOnce = true;

			// Actualizar propiedades reactivas
			controllerInfo.Percent = displayPercent;
			controllerInfo.StatusText = statusLabel;
			SolidColorBrush statusBrush = new(statusColor);
			statusBrush.Freeze();
			controllerInfo.BatteryColor = statusBrush;
			controllerInfo.IsCharging = isOnExternalPower;

			// Controller más crítico para modo compacto y tray
			if (displayPercent >= 0.0 && displayPercent <= lowestPercent)
			{
				lowestPercent = displayPercent;
				lowestText = $"{Math.Round(displayPercent)}%";
				lowestBrush = statusBrush;
				criticalSlot = controllerInfo.XInputSlot;
			}

			anyExternalPower |= isOnExternalPower;
			allSummaries += $"{ctrlName}: {Math.Round(displayPercent)}% | ";

			// Alerta de batería baja (con prevención de spam ante desconexiones momentáneas)
			if (displayPercent >= 0.0 && displayPercent <= AppSettings.Current.LowBatteryThreshold && !isOnExternalPower)
			{
				hasAnyLowBattery = true;
				if (!controllerInfo.LowBatteryWarned)
				{
					try
					{
						TrayIcon?.ShowBalloonTip(
							Strings.Get("LowBattery"),
							string.Format(Strings.Get("ControllerHas"), ctrlName, Math.Round(displayPercent)),
							BalloonIcon.Warning);
					}
					catch { }

					StartFlashAlert();
					_soundCts?.Cancel();
					_soundCts = new CancellationTokenSource();
					_ = SoundService.PlayLowBatteryAlertAsync(_soundCts.Token);

					controllerInfo.LowBatteryWarned = true;
				}
			}
			else if (displayPercent > AppSettings.Current.LowBatteryThreshold || isOnExternalPower)
			{
				// Solo restablecer la advertencia si la batería realmente subió o se conectó por cable
				controllerInfo.LowBatteryWarned = false;
			}
		}

		TitleText.Text = (Controllers.Count == 1)
			? Controllers[0].Name
			: $"{Strings.Get("Controllers")} ({Controllers.Count})";

		CompactPercentText.Text = lowestText;
		CompactPercentText.Foreground = lowestBrush;

		if (Controllers.Count > 1 && criticalSlot >= 0)
		{
			CompactSlotBadgeText.Text = $"P{criticalSlot + 1}";
			CompactSlotBadge.Visibility = Visibility.Visible;
		}
		else
		{
			CompactSlotBadge.Visibility = Visibility.Collapsed;
		}

		if (allSummaries.EndsWith(" | "))
		{
			allSummaries = allSummaries.Substring(0, allSummaries.Length - 3);
		}

		CompactModeBorder.ToolTip = (!string.IsNullOrWhiteSpace(allSummaries))
			? $"{allSummaries} | {Strings.Get("CompactTip")}"
			: Strings.Get("CompactTip");

		double trayPercent = (lowestPercent != double.MaxValue) ? lowestPercent : -1.0;
		UpdateTrayIconAndTooltip(trayPercent, anyExternalPower, isConnected: true, anyExternalPower ? Strings.Get("UsbCharged") : Strings.Get("Charged"), allSummaries);

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

	private void StartFlashAlert()
	{
		if (_flashTimer != null && _flashTimer.IsEnabled) return;

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
	}

	private void FlashTick(object? sender, EventArgs e)
	{
		if (_flashRemaining <= 0)
		{
			_flashTimer?.Stop();
			FullModeBorder.Background = _originalBackground;
			CompactModeBorder.Background = _originalBackground;
			return;
		}

		if (_flashRemaining % 2 == 0)
		{
			SolidColorBrush background = FlashBrushes[_flashColorIndex % FlashBrushes.Length];
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

	private void CompactMode_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (CompactModeBorder.Visibility != Visibility.Visible) return;

		e.Handled = true;

		double step = (e.Delta > 0) ? 0.05 : -0.05;
		double newOpacity = Math.Clamp(Math.Round(AppSettings.Current.CompactOpacity + step, 2), 0.15, 1.0);

		AppSettings.Current.CompactOpacity = newOpacity;
		RequestSaveSettings(); // Debounced: evita escrituras síncronas masivas a disco

		ApplyCompactOpacity(newOpacity, showMeter: true);
	}

	private void ApplyCompactOpacity(double opacity, bool showMeter = false)
	{
		BeginAnimation(OpacityProperty, null);
		Opacity = opacity;

		if (showMeter && CompactModeBorder.Visibility == Visibility.Visible)
		{
			int pct = (int)Math.Round(opacity * 100);
			OpacityMeterText.Text = $"{pct}%";
			OpacityMeterBar.Value = pct;

			CompactPercentText.Visibility = Visibility.Collapsed;
			OpacityMeterOverlay.Visibility = Visibility.Visible;

			if (_opacityMeterTimer == null)
			{
				_opacityMeterTimer = new DispatcherTimer
				{
					Interval = TimeSpan.FromMilliseconds(1200)
				};
				_opacityMeterTimer.Tick += (s, e) =>
				{
					_opacityMeterTimer?.Stop();
					OpacityMeterOverlay.Visibility = Visibility.Collapsed;
					CompactPercentText.Visibility = Visibility.Visible;
				};
			}
			_opacityMeterTimer.Stop();
			_opacityMeterTimer.Start();
		}
	}

	private void SwitchToCompactMode()
	{
		FullModeBorder.Visibility = Visibility.Collapsed;
		CompactModeBorder.Visibility = Visibility.Visible;
		SizeToContent = SizeToContent.Manual;
		Width = _compactSize;
		Height = _compactSize;
		CompactPercentText.FontSize = _compactSize * 0.3;
		_anchorLeft = Left;
		_anchorTop = Top;
		_orbitAngle = 0.0;

		if (TextShiftTransform != null)
		{
			TextShiftTransform.BeginAnimation(TranslateTransform.XProperty, null);
			TextShiftTransform.BeginAnimation(TranslateTransform.YProperty, null);
			TextShiftTransform.X = 0;
			TextShiftTransform.Y = 0;
		}

		ApplyCompactOpacity(AppSettings.Current.CompactOpacity, showMeter: false);
		TriggerShineAnimation();
		ApplyClickThrough();
	}

	private void SwitchToFullMode()
	{
		FullModeBorder.Visibility = Visibility.Visible;
		CompactModeBorder.Visibility = Visibility.Collapsed;
		ClearValue(HeightProperty);
		Height = double.NaN;
		SizeToContent = SizeToContent.Height;
		Width = 260.0;
		BeginAnimation(OpacityProperty, null);
		Opacity = 1.0;
		_anchorLeft = double.NaN;
		_anchorTop = double.NaN;
		BeginAnimation(LeftProperty, null);
		BeginAnimation(TopProperty, null);

		// Clampeo al área de trabajo para evitar que el ensanchamiento a 260px desborde la pantalla
		ClampWindowToCurrentMonitor();

		if (TextShiftTransform != null)
		{
			TextShiftTransform.BeginAnimation(TranslateTransform.XProperty, null);
			TextShiftTransform.BeginAnimation(TranslateTransform.YProperty, null);
			TextShiftTransform.X = 0;
			TextShiftTransform.Y = 0;
		}

		ApplyClickThrough();
	}

	private void ToggleMode_Click(object sender, RoutedEventArgs e)
	{
		if (FullModeBorder.Visibility == Visibility.Visible)
		{
			SwitchToCompactMode();
			AppSettings.Current.IsCompactMode = true;
		}
		else
		{
			SwitchToFullMode();
			AppSettings.Current.IsCompactMode = false;
		}
		RequestSaveSettings();
	}

	private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
	{
		if (CompactModeBorder.Visibility == Visibility.Visible)
		{
			BeginAnimation(LeftProperty, null);
			BeginAnimation(TopProperty, null);

			double delta = Math.Max(e.HorizontalChange, e.VerticalChange);
			double val = Math.Max(60.0, Width + delta);
			val = Math.Min(300.0, val);
			_compactSize = val;
			Width = val;
			Height = val;
			CompactPercentText.FontSize = val * 0.3;

			ClampWindowToCurrentMonitor();

			AppSettings.Current.CompactSize = _compactSize;
			RequestSaveSettings();
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

	private void TriggerShineAnimation()
	{
		try
		{
			if (ShineTransform != null)
			{
				double fromValue = (0.0 - Width) * 1.5;
				double toValue = Width * 2.0;
				DoubleAnimation animation = new(fromValue, toValue, TimeSpan.FromSeconds(1.3))
				{
					EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
				};
				ShineTransform.BeginAnimation(TranslateTransform.XProperty, animation);
			}
		}
		catch { }
	}

	private void BurnInTimer_Tick(object? sender, EventArgs e)
	{
		if (AppSettings.Current.AntiBurnInEnabled && CompactModeBorder.Visibility == Visibility.Visible && IsVisible && !_isHiding)
		{
			if (double.IsNaN(_anchorLeft) || double.IsNaN(_anchorTop))
			{
				_anchorLeft = Left;
				_anchorTop = Top;
			}

			// Órbita circular armónica sutil (R = 4px)
			_orbitAngle += Math.PI / 4.0;
			if (_orbitAngle >= Math.PI * 2.0)
			{
				_orbitAngle -= Math.PI * 2.0;
			}

			double radius = 4.0;
			double offsetX = Math.Round(radius * Math.Cos(_orbitAngle), 2);
			double offsetY = Math.Round(radius * Math.Sin(_orbitAngle), 2);

			// Resolver el monitor real donde reside la ventana (soporte multi-monitor)
			Rect workArea = WindowStyleService.GetCurrentMonitorWorkArea(this);
			double toLeft = Math.Clamp(_anchorLeft + offsetX, workArea.Left, Math.Max(workArea.Left, workArea.Right - Width));
			double toTop = Math.Clamp(_anchorTop + offsetY, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - Height));

			DoubleAnimation animLeft = new(toLeft, TimeSpan.FromSeconds(3.0))
			{
				EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
			};
			DoubleAnimation animTop = new(toTop, TimeSpan.FromSeconds(3.0))
			{
				EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
			};
			BeginAnimation(LeftProperty, animLeft);
			BeginAnimation(TopProperty, animTop);

			try
			{
				if (TextShiftTransform != null)
				{
					// Micro-desplazamiento de texto (+- 1.0px) para mitigar quemado de subpíxeles
					double textX = Math.Round(1.0 * Math.Cos(_orbitAngle * 3.0), 2);
					double textY = Math.Round(1.0 * Math.Sin(_orbitAngle * 3.0), 2);
					DoubleAnimation animTextX = new(textX, TimeSpan.FromSeconds(4.0))
					{
						EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
					};
					DoubleAnimation animTextY = new(textY, TimeSpan.FromSeconds(4.0))
					{
						EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
					};
					TextShiftTransform.BeginAnimation(TranslateTransform.XProperty, animTextX);
					TextShiftTransform.BeginAnimation(TranslateTransform.YProperty, animTextY);
				}
			}
			catch { }
		}
		else
		{
			_anchorLeft = double.NaN;
			_anchorTop = double.NaN;
			BeginAnimation(LeftProperty, null);
			BeginAnimation(TopProperty, null);
		}
	}

	private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ButtonState == MouseButtonState.Pressed)
		{
			BeginAnimation(LeftProperty, null);
			BeginAnimation(TopProperty, null);
			double startLeft = Left;
			double startTop = Top;
			try
			{
				DragMove();
			}
			catch (InvalidOperationException) { }

			// Solo actualizar y persistir el ancla si el usuario realmente arrastró la ventana
			if (Math.Abs(Left - startLeft) > 1.0 || Math.Abs(Top - startTop) > 1.0)
			{
				_anchorLeft = Left;
				_anchorTop = Top;
				_orbitAngle = 0.0;
				AppSettings.Current.WindowLeft = Left;
				AppSettings.Current.WindowTop = Top;
				RequestSaveSettings();
			}
		}
	}

	private void Window_MouseEnter(object sender, MouseEventArgs e)
	{
		if (CompactModeBorder.Visibility == Visibility.Visible)
		{
			// Si la alarma de batería baja está activa, asegurar que permanezca en opacidad sólida 1.0
			if (Controllers.Any(c => c.Percent >= 0.0 && c.Percent <= AppSettings.Current.LowBatteryThreshold && !c.IsCharging))
			{
				BeginAnimation(OpacityProperty, null);
				Opacity = 1.0;
			}
		}
	}

	private void Window_MouseLeave(object sender, MouseEventArgs e)
	{
		if (CompactModeBorder.Visibility != Visibility.Visible) return;

		if (Controllers.Any(c => c.Percent >= 0.0 && c.Percent <= AppSettings.Current.LowBatteryThreshold && !c.IsCharging))
		{
			BeginAnimation(OpacityProperty, null);
			DoubleAnimation animation = new(1.0, TimeSpan.FromSeconds(0.2));
			BeginAnimation(OpacityProperty, animation);
		}
		else
		{
			BeginAnimation(OpacityProperty, null);
			Opacity = AppSettings.Current.CompactOpacity;
		}
	}
}
