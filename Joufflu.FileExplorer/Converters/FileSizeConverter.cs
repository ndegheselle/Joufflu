using System.Globalization;
using System.IO;
using System.Windows.Data;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Converters
{
    /// <summary>
    /// Human readable size of an <see cref="IExplorerNode"/> (or of a size in bytes, or of a <see cref="FileInfo"/>).
    /// Directories and nodes without a size convert to null, so their cell stays empty.
    /// </summary>
    /// <remarks>
    /// The size of a node is read when the node is built, so this never touches the disk : a converter is evaluated on
    /// every layout pass of the cell that uses it.
    /// </remarks>
    public class FileSizeConverter : IValueConverter
    {
        /// <summary>Shared instance, for the templates that have no resource dictionary of their own.</summary>
        public static readonly FileSizeConverter Default = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            long? size = value switch
            {
                IExplorerNode node => node.Size,
                long bytes => bytes,
                int bytes => bytes,
                FileInfo info => info.Exists ? info.Length : null,
                _ => null
            };

            return size == null ? null : Format(size.Value);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static string Format(long size)
        {
            if (size >= (1 << 30))
                return $"{size >> 30} Go";
            if (size >= (1 << 20))
                return $"{size >> 20} Mo";
            if (size >= (1 << 10))
                return $"{size >> 10} Ko";
            return $"{size} o";
        }
    }
}
