using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using Joufflu.FileExplorer.Loaders;
using Joufflu.Helpers;

namespace Joufflu.FileExplorer.Converters;

/// <summary>
/// Convert a path (or a <see cref="IExplorerPathNode"/>) to the icon windows uses for it.
/// Exposed as a resource by the explorer (<see cref="ExplorerResources.SystemIconConverter"/>) and used
/// by the default node templates.
/// </summary>
/// <remarks>
/// Icons are cached : files share the icon of their extension, while folders and files that carry their
/// own icon (executables, shortcuts, ...) are cached per path.
/// </remarks>
public class SystemIconConverter : IValueConverter
{
    /// <summary>Extensions of the files that have their own icon instead of sharing one.</summary>
    private static readonly string[] UniqueIconExtensions = [".exe", ".lnk", ".ico", ".url"];

    private readonly Dictionary<string, ImageSource?> _icons = new Dictionary<string, ImageSource?>();

    /// <summary>Use the small icon (16x16) instead of the large one (32x32).</summary>
    public bool IsSmall { get; set; } = true;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? path = value switch
        {
            IExplorerPathNode node => node.Path,
            string text => text,
            _ => null
        };

        if (string.IsNullOrEmpty(path))
            return null;

        string key = GetCacheKey(path);
        if (_icons.TryGetValue(key, out ImageSource? cached))
            return cached;

        ImageSource? icon = GetIcon(path);
        _icons[key] = icon;
        return icon;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    /// <summary>
    /// Resolve the icon of a path, returns <c>null</c> when there is none (a node that is not on the
    /// file system for example). Override to use another icons source.
    /// </summary>
    protected virtual ImageSource? GetIcon(string path) => SystemIcons.GetIcon(path, IsSmall);

    private string GetCacheKey(string path)
    {
        // Folders may have a custom icon, they can't share the one of an extension
        if (Directory.Exists(path))
            return path.ToLowerInvariant();

        string extension = Path.GetExtension(path);
        if (extension.Length == 0 || UniqueIconExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return path.ToLowerInvariant();
        return extension.ToLowerInvariant();
    }
}
