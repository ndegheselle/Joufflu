using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Joufflu.Converters
{
    /// <summary>
    /// Turns a <see cref="double"/> into a <see cref="Thickness"/> that only sets the top edge
    /// (<c>0, value, 0, 0</c>). Handy for reserving vertical space from a single measured height —
    /// e.g. padding content down by <c>ThemedWindow.TitleBarActualHeight</c> so an overlaid title
    /// bar does not sit on top of it (the height is 0 for a standard bar, so the space is optional).
    /// </summary>
    public class TopThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double top = value is double d && !double.IsNaN(d) ? d : 0;
            return new Thickness(0, top, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
