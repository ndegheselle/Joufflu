using System.Windows;
using System.Windows.Data;

namespace Joufflu.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public static VisibilityConverter Default { get; } = new VisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BooleanConverter booleanConverter = new BooleanConverter();
            bool isVisible = booleanConverter.Convert(value, targetType, parameter, culture) as bool? ?? false;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}
