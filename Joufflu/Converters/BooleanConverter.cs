using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Joufflu.Converters
{
    /// <summary>
    /// Convert any value to a boolean.
    /// </summary>
    public class BooleanConverter : IValueConverter
    {
        public static BooleanConverter Default { get; } = new BooleanConverter();

        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">bool, indicate wheter the result should be true or false</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;

            if (value is bool b)
                result = b;
            else if (value is string s)
                result = !string.IsNullOrEmpty(s);
            else if (value is int i)
                result = i > 0;
            else if (value is ICollection collection)
                result = collection.Count > 0;
            else
                result = value != null;

            // For exemple if the parameter is "false", and the value is null, the result will be true.
            if (!bool.TryParse(parameter?.ToString(), out var target))
                target = true;

            return result == target;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Combines the values of a MultiBinding into one boolean, each read the way
    /// <see cref="BooleanConverter"/> reads it. The parameter is the operator: "&amp;&amp;" (the default)
    /// when every value must be true, "||" when one is enough.
    /// </summary>
    public class BooleansConverter : IMultiValueConverter
    {
        public static BooleansConverter Default { get; } = new BooleansConverter();

        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string conjunction = parameter?.ToString()?.Trim() ?? "&&";
            bool requiresAll = conjunction == "&&";
            BooleanConverter converter = new BooleanConverter();

            IEnumerable<bool> results = values.Select(value => (bool)converter.Convert(value, targetType, parameter, culture)!);
            return requiresAll ? results.All(result => result) : results.Any(result => result);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
