using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Joufflu.Converters
{
    /// <summary>
    /// Whether a value is "set": a bool as is, a non-empty string, a positive int, a non-empty collection, any other
    /// non-null value. A <c>"false"</c> parameter inverts the result.
    /// </summary>
    public class BooleanConverter : IValueConverter
    {
        public static BooleanConverter Default { get; } = new BooleanConverter();

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

            // For example, with a "false" parameter a null value gives true.
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
    /// Combines the values of a MultiBinding, each read as <see cref="BooleanConverter"/> does, with <c>&amp;&amp;</c>
    /// (the default) or <c>||</c> given as the parameter.
    /// </summary>
    public class BooleansConverter : IMultiValueConverter
    {
        public static BooleansConverter Default { get; } = new BooleansConverter();

        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string lConjonction = parameter?.ToString()?.Trim() ?? "&&";
            BooleanConverter lConverter = new BooleanConverter();
            bool lResultat = lConjonction == "&&" ? true : false;

            foreach (object lValue in values)
            {
                if (lConjonction == "&&")
                    lResultat = lResultat && (bool)lConverter.Convert(lValue, targetType, parameter, culture)!;
                else
                    lResultat = lResultat || (bool)lConverter.Convert(lValue, targetType, parameter, culture)!;
            }
            return lResultat;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
