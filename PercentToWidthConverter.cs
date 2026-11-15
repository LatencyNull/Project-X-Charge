using System;
using System.Globalization;
using System.Windows.Data;

namespace XboxBatteryMonitor;

/// <summary>
/// Convierte el porcentaje de batería en un ancho visual proporcional en píxeles.
/// Admite tanto IValueConverter simple como IMultiValueConverter para adaptarse
/// de forma fluida y exacta al ancho real (ActualWidth) del contenedor exterior.
/// </summary>
public class PercentToWidthConverter : IValueConverter, IMultiValueConverter
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
			double min = (num > 0.0) ? Math.Min(4.0, max) : 0.0;
			return Math.Clamp((num / 100.0) * max, min, max);
		}
		return 0.0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length >= 2 && values[0] is double percent && values[1] is double trackWidth)
		{
			if (percent >= 0.0 && trackWidth > 0.0)
			{
				double min = (percent > 0.0) ? Math.Min(4.0, trackWidth) : 0.0;
				return Math.Clamp((percent / 100.0) * trackWidth, min, trackWidth);
			}
		}
		return 0.0;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
