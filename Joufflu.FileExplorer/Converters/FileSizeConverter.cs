using System.Globalization;
using System.IO;
using System.Windows.Data;
using Joufflu.FileExplorer.Data;

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

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            FileInfo? info = value switch
            {
                FileInfo fileInfo => fileInfo,
                PhysicalFile file => new FileInfo(file.Path),
                string path => new FileInfo(path),
                _ => null
            };

            if (info == null || !info.Exists)
                return null;

            return Format(info.Length);
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
