using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using LiveChartsCore;
using System.Linq;

namespace MikroTikMonitor.ViewModels.Converters
{
    public class CollectionToSeriesMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Check if we have any values to process
                if (values == null || values.Length == 0)
                {
                    return null;
                }

                // Handle the first value (main collection)
                if (values[0] is IEnumerable collection)
                {
                    // Check if it's already an ISeries collection, which is the case for our usage
                    if (collection is ObservableCollection<ISeries> series)
                    {
                        return series;
                    }
                    
                    // If other conversions are needed, they could be added here
                }

                // Return null if we couldn't convert
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}