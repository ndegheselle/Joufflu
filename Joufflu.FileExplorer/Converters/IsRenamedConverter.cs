using System.Globalization;
using System.Windows.Data;
using Joufflu.FileExplorer.Controls.Base;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Converters
{
    /// <summary>
    /// Whether a node is the one being renamed, so that the control displaying it turns its name into an editable one.
    /// Values : the <see cref="IExplorerNode"/>, then the <see cref="IExplorerUi.RenamedNode"/> of the control.
    /// </summary>
    public class IsRenamedConverter : IMultiValueConverter
    {
        /// <summary>Shared instance, for the templates that have no resource dictionary of their own.</summary>
        public static readonly IsRenamedConverter Default = new();

        public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
            => values.Length == 2 && values[0] is IExplorerNode node && ReferenceEquals(node, values[1]);

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
