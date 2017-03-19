using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ulterius.Windows.Management.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        public bool HiddenOnFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ^ IsInverted 
                ? Visibility.Visible 
                : (HiddenOnFalse ? Visibility.Hidden : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}