using Joufflu.FileExplorer.Data;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Joufflu.FileExplorer.Converters
{
    /// <summary>
    /// Human readable size of a file <see cref="IExplorerNode"/> (or of a <see cref="FileInfo"/> or a path given as a
    /// string). Directories and nodes without a size on disk convert to null, so their cell stays empty.
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        /// <summary>Shared instance, for the templates that have no resource dictionary of their own.</summary>
        public static readonly FileSizeConverter Default = new();

        /// <summary>
        /// The node itself is converted and not its size : a directory, or a node type of an application that carries
        /// no size at all, has no Size property to bind to, where binding one would report an error of its own.
        /// </summary>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            long? size = value switch
            {
                IExplorerFile file => file.Size,
                // Directories and the other nodes are sizeless, their cell stays empty.
                IExplorerNode => null,
                FileInfo info => info.Length,
                string path => File.Exists(path) ? new FileInfo(path).Length : null,
                long length => length,
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
