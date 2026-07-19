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
            double lRequested = ToDouble(values.Length > 0 ? values[0] : null);
            double lWidth = ToDouble(values.Length > 1 ? values[1] : null);
            double lHeight = ToDouble(values.Length > 2 ? values[2] : null);

            double lMax = Math.Min(lWidth, lHeight) / 2;
            double lRadius = lMax > 0 ? Math.Min(lRequested, lMax) : lRequested;
            return new CornerRadius(lRadius);
        }

        private static double ToDouble(object? value)
            => value is double d && !double.IsNaN(d) ? d : 0;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
