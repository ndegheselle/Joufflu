using System.IO;

namespace Joufflu.FileExplorer.Nodes;

/// <summary>
/// Paths of nodes, compared the way the file system of Windows does.
/// </summary>
internal static class ExplorerPaths
{
    /// <summary>
    /// [path] in full and without a trailing separator, so that two spellings of one path compare
    /// equal.
    /// </summary>
    public static string Normalize(string path)
    {
        string fullPath = Path.GetFullPath(path);
        return Path.TrimEndingDirectorySeparator(fullPath);
    }
}
