using System;
using System.IO;
using System.Text.Json;

namespace XboxBatteryMonitor;

public class AppSettings
{
	private int _lowBatteryThreshold = 20;
	private int _pollIntervalSeconds = 3;
	private int _flashCount = 13;
	private double _compactOpacity = 0.85;
	private double _compactSize = 80.0;

	public int LowBatteryThreshold
	{
		get => _lowBatteryThreshold;
		set => _lowBatteryThreshold = Math.Clamp(value, 5, 50);
	}

	public int ThemeIndex { get; set; } = 0;

	public bool SoundUsbEnabled { get; set; } = true;

	public bool SoundBatteryEnabled { get; set; } = true;

	public bool StartInTray { get; set; } = false;

	public int PollIntervalSeconds
	{
		get => _pollIntervalSeconds;
		set => _pollIntervalSeconds = Math.Clamp(value, 1, 60);
	}

	public int FlashCount
	{
		get => _flashCount;
		set => _flashCount = Math.Clamp(value, 1, 30);
	}

	public bool HideUntilLowBattery { get; set; } = false;

	public string Language { get; set; } = "";

	public bool ClickThroughEnabled { get; set; } = false;

	public bool HotKeyEnabled { get; set; } = true;

	public bool AntiBurnInEnabled { get; set; } = true;

	public bool IsCompactMode { get; set; } = false;

	public double CompactSize
	{
		get => _compactSize;
		set => _compactSize = Math.Clamp(value, 60.0, 300.0);
	}

	public double? WindowLeft { get; set; } = null;

	public double? WindowTop { get; set; } = null;

	public double CompactOpacity
	{
		get => _compactOpacity;
		set => _compactOpacity = Math.Clamp(Math.Round(value, 2), 0.15, 1.0);
	}

	private static string ConfigFolder
	{
		get
		{
			string newFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "X-Charge");
			string legacyFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XboxBatteryMonitor");

			if (!Directory.Exists(newFolder))
			{
				Directory.CreateDirectory(newFolder);
				// Migración automática desde XboxBatteryMonitor si existe
				try
				{
					string legacyConfig = Path.Combine(legacyFolder, "config.json");
					string newConfig = Path.Combine(newFolder, "config.json");
					if (File.Exists(legacyConfig) && !File.Exists(newConfig))
					{
						File.Copy(legacyConfig, newConfig, overwrite: false);
					}
				}
				catch { }
			}

			return newFolder;
		}
	}

	private static string ConfigPath => Path.Combine(ConfigFolder, "config.json");

	public static AppSettings Current { get; } = Load();

	private static AppSettings Load()
	{
		string path = ConfigPath;
		string bakPath = ConfigPath + ".bak";

		if (File.Exists(path))
		{
			try
			{
				string json = File.ReadAllText(path);
				if (!string.IsNullOrWhiteSpace(json))
				{
					return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
				}
			}
			catch (Exception)
			{
				// Si el JSON principal está dañado, respaldar para diagnóstico
				try
				{
					string backup = path + ".corrupted";
					if (!File.Exists(backup))
					{
						File.Copy(path, backup);
					}
				}
				catch { }

				// Intentar restaurar desde respaldo .bak automático
				if (File.Exists(bakPath))
				{
					try
					{
						string bakJson = File.ReadAllText(bakPath);
						if (!string.IsNullOrWhiteSpace(bakJson))
						{
							return JsonSerializer.Deserialize<AppSettings>(bakJson) ?? new AppSettings();
						}
					}
					catch { }
				}
			}
		}
		else if (File.Exists(bakPath))
		{
			try
			{
				string bakJson = File.ReadAllText(bakPath);
				if (!string.IsNullOrWhiteSpace(bakJson))
				{
					return JsonSerializer.Deserialize<AppSettings>(bakJson) ?? new AppSettings();
				}
			}
			catch { }
		}

		return new AppSettings();
	}

	private static readonly object _saveLock = new();

	public void Save()
	{
		lock (_saveLock)
		{
			try
			{
				string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
				{
					WriteIndented = true
				});
				string tempPath = ConfigPath + ".tmp";
				string bakPath = ConfigPath + ".bak";

				File.WriteAllText(tempPath, json);

				// Mantener copia de seguridad .bak antes de sobreescribir el archivo activo
				if (File.Exists(ConfigPath))
				{
					try
					{
						File.Copy(ConfigPath, bakPath, overwrite: true);
					}
					catch { }
				}

				File.Move(tempPath, ConfigPath, overwrite: true);
			}
			catch { }
		}
	}
}
