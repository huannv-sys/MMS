using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MikroTikMonitor.ViewModels.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = string.IsNullOrEmpty(value as string);
            bool inverse = parameter as string == "Inverse";
            
            if (inverse)
            {
                return isEmpty ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return isEmpty ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}