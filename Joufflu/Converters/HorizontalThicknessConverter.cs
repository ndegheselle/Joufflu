using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Joufflu.Converters
{
    /// <summary>
    /// Keeps the left and right of a <see cref="Thickness"/> and drops its top and bottom. A control of fixed height
    /// (a button, a text box) takes only the horizontal part of its padding and centres its content in the height of
    /// its size, so a large vertical padding never crushes the content.
    /// </summary>
    public class HorizontalThicknessConverter : IValueConverter
    {
        public static HorizontalThicknessConverter Default { get; } = new HorizontalThicknessConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Thickness thickness ? new Thickness(thickness.Left, 0, thickness.Right, 0) : new Thickness(0);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
