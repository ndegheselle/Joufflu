using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Joufflu.Converters
{
    /// <summary>
    /// Clamps a requested corner radius so it never exceeds half of the element's smaller
    /// dimension, keeping thin controls (scroll bar thumb, progress bar…) round at most
    /// instead of malformed when a large theme radius is applied.
    /// Bindings, in order: [0] requested radius (double), [1] ActualWidth, [2] ActualHeight.
    /// </summary>
    public class MaxCornerRadiusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double requested = ToDouble(values.Length > 0 ? values[0] : null);
            double width = ToDouble(values.Length > 1 ? values[1] : null);
            double height = ToDouble(values.Length > 2 ? values[2] : null);

            double max = Math.Min(width, height) / 2;
            double radius = max > 0 ? Math.Min(requested, max) : requested;
            return new CornerRadius(radius);
        }

        private static double ToDouble(object? value)
            => value is double d && !double.IsNaN(d) ? d : 0;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
