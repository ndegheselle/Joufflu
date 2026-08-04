using System.Globalization;
using System.IO;
using System.Windows.Data;
using Joufflu.FileExplorer.Controls;
using Joufflu.Helpers;

namespace Joufflu.FileExplorer.Converters
{
    /// <summary>
    /// System icon of an <see cref="IExplorerNode"/> (or of a path given as a string), so that a node template can
    /// show a file and a folder the way Windows does instead of a font icon.
    /// </summary>
    /// <remarks>
    /// The icon comes from the file type, not from the file itself : an executable or an image shows the generic icon
    /// of its extension. See <see cref="SystemIcons"/>.
    /// </remarks>
    public class ExplorerIconConverter : IValueConverter
    {
        /// <summary>Shared instances, for the templates that have no resource dictionary of their own.</summary>
        public static readonly ExplorerIconConverter Small = new() { IsSmall = true };
        public static readonly ExplorerIconConverter Large = new() { IsSmall = false };

        /// <summary>
        /// True for the 16x16 icons, false for the 32x32 ones.
        /// </summary>
        public bool IsSmall { get; set; } = true;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                IExplorerNode node => SystemIcons.GetIcon(GetPath(node), node is IExplorerDirectory, IsSmall),
                // A path alone, for a breadcrumb or a picker showing a file it only knows the path of.
                string path => SystemIcons.GetIcon(path, Directory.Exists(path), IsSmall),
                _ => null
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();

        /// <summary>
        /// Path the shell identifies the type of a node with. A node that isn't a physical one falls back on its name,
        /// which is enough : the icon of a file only depends on its extension.
        /// </summary>
        private static string GetPath(IExplorerNode node) => node switch
        {
            PhysicalFile file => file.Path,
            PhysicalDirectory directory => directory.Path,
            _ => node.Name
        };
    }
}
