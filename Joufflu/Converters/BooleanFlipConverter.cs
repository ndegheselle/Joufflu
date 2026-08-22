using System.Windows.Data;

namespace Joufflu.Converters
{
    public class BooleanFlipConverter : IValueConverter
    {
        public static BooleanFlipConverter Default = new BooleanFlipConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool booleanValue)
                return !booleanValue;
            return value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is bool booleanValue)
                return !booleanValue;
            return value;
        }
    }
}
