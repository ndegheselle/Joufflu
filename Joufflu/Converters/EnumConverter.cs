using System.Globalization;
using System.Windows.Data;

namespace Joufflu.Converters
{
    /// <summary>
    /// True when the bound enum value equals the <c>ConverterParameter</c>. Made for binding a
    /// group of mutually-exclusive toggles (e.g. RadioButtons) to a single enum property:
    /// each toggle passes the enum member it represents as the parameter.
    /// </summary>
    public class EnumMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || parameter is null)
                return false;

            return value.Equals(parameter);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Only the newly-checked toggle writes back its own enum member; the ones being
            // unchecked report false and must leave the source untouched.
            if (value is true && parameter is not null)
                return parameter;

            return Binding.DoNothing;
        }
    }
}
