using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace XboxBatteryMonitor;

public class ControllerInfo : INotifyPropertyChanged
{
	private string _id = "";

	private string _name = "";

	private double _percent;

	private string _statusText = "";

	private SolidColorBrush _batteryColor = new SolidColorBrush(Colors.Transparent);

	private bool _isCharging;

	public string Id
	{
		get => _id;
		set
		{
			if (_id != value)
			{
				_id = value;
				OnPropertyChanged();
			}
		}
	}

	public string Name
	{
		get => _name;
		set
		{
			if (_name != value)
			{
				_name = value;
				OnPropertyChanged();
			}
		}
	}

	public double Percent
	{
		get => _percent;
		set
		{
			if (Math.Abs(_percent - value) > 0.001)
			{
				_percent = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(DisplayPercentText));
			}
		}
	}

	public string StatusText
	{
		get => _statusText;
		set
		{
			if (_statusText != value)
			{
				_statusText = value;
				OnPropertyChanged();
			}
		}
	}

	public SolidColorBrush BatteryColor
	{
		get => _batteryColor;
		set
		{
			if (_batteryColor != value)
			{
				_batteryColor = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsCharging
	{
		get => _isCharging;
		set
		{
			if (_isCharging != value)
			{
				_isCharging = value;
				OnPropertyChanged();
			}
		}
	}

	public double LastWirelessPercent { get; set; } = -1.0;

	public int XInputSlot { get; set; } = -1;

	public bool WasOnExternalPower { get; set; }

	public bool LowBatteryWarned { get; set; }

	public bool IsEstimated { get; set; }

	public DateTime ConnectedTimestamp { get; set; } = DateTime.UtcNow;

	public string DisplayPercentText => Percent >= 0 ? $"{Math.Round(Percent)}%" : "--%";

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
