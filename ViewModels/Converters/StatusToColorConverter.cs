using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MikroTikMonitor.ViewModels.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "bound" => new SolidColorBrush(Colors.LimeGreen),
                    "active" => new SolidColorBrush(Colors.LimeGreen),
                    "online" => new SolidColorBrush(Colors.LimeGreen),
                    "connected" => new SolidColorBrush(Colors.LimeGreen),
                    "disconnected" => new SolidColorBrush(Colors.Red),
                    "offline" => new SolidColorBrush(Colors.Red),
                    "error" => new SolidColorBrush(Colors.Red),
                    "warning" => new SolidColorBrush(Colors.Orange),
                    "info" => new SolidColorBrush(Colors.DeepSkyBlue),
                    "debug" => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            else if (value is int severityLevel)
            {
                return severityLevel switch
                {
                    0 => new SolidColorBrush(Colors.Gray),     // Debug
                    1 => new SolidColorBrush(Colors.DeepSkyBlue), // Info
                    2 => new SolidColorBrush(Colors.Orange),   // Warning
                    3 => new SolidColorBrush(Colors.Red),      // Error
                    4 => new SolidColorBrush(Colors.DarkRed),  // Critical
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}