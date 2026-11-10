using System;
using System.Globalization;
using System.Windows.Data;

namespace XboxBatteryMonitor;

public class PercentToWidthConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is double num && num >= 0.0)
		{
			double max = 175.0;
			if (parameter is string paramStr && double.TryParse(paramStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double pMax))
			{
				max = pMax;
			}
			return Math.Clamp((num / 100.0) * max, 4.0, max);
		}
		return 0.0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
