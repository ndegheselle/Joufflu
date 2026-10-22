using System.IO;

namespace Joufflu.FileExplorer.Nodes;

/// <summary>
/// How the sources and the controls compare paths and name new nodes, the file system of Windows being case
/// insensitive. None of it reads the disk, so the rules are tested without one : what is taken on the disk is asked
/// to the caller.
/// </summary>
internal static class ExplorerPaths
{
    /// <summary>
    /// Whether two paths designate the same node.
    /// </summary>
    public static bool AreEqual(string left, string right)
        => string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether <paramref name="candidate"/> is <paramref name="path"/> itself or one of its parents.
    /// </summary>
    /// <remarks>
    /// Compared on whole segments, by ending both with a separator : a plain StartsWith would report "C:\foo2" as
    /// being inside "C:\foo".
    /// </remarks>
    public static bool IsSameOrAncestor(string candidate, string path)
        => (Normalize(path) + Path.DirectorySeparatorChar).StartsWith(
            Normalize(candidate) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether <paramref name="path"/> is a node directly in <paramref name="directoryPath"/>, not deeper.
    /// </summary>
    public static bool IsDirectChild(string path, string directoryPath)
    {
        try
        {
            string? parent = Path.GetDirectoryName(Normalize(path));
            return parent is not null && AreEqual(parent, directoryPath);
        }
        catch (ArgumentException)
        {
            // An invalid path is left to the transfer, which ignores what doesn't exist.
            return false;
        }
    }

    /// <summary>
    /// <paramref name="path"/> moved from under <paramref name="oldAncestor"/> to under
    /// <paramref name="newAncestor"/>, for a directory renamed or moved along with what it holds.
    /// </summary>
    public static string Rebase(string path, string oldAncestor, string newAncestor)
    {
        // Relative on whole segments and whatever the case, where cutting the old ancestor's length off the path
        // relied on both being written alike.
        string relative = Path.GetRelativePath(oldAncestor, path);
        return relative == "." ? newAncestor : Path.Combine(newAncestor, relative);
    }

    /// <summary>
    /// "File - Copy.ext", then "File - Copy (2).ext" while <paramref name="isTaken"/>, the way Windows names a copy
    /// made next to its source.
    /// </summary>
    public static string CopyName(string name, Func<string, bool> isTaken)
    {
        string bareName = Path.GetFileNameWithoutExtension(name);
        string extension = Path.GetExtension(name);

        for (int index = 1; index < int.MaxValue; index++)
        {
            string number = index > 1 ? $" ({index})" : "";
            string candidate = $"{bareName} - Copy{number}{extension}";
            if (!isTaken(candidate))
                return candidate;
        }

        return name;
    }

    /// <summary>
    /// "New folder", then "New folder (2)" while <paramref name="isTaken"/>, the way Windows does.
    /// </summary>
    public static string NewDirectoryName(Func<string, bool> isTaken)
    {
        const string baseName = "New folder";

        for (int index = 1; index < int.MaxValue; index++)
        {
            string candidate = index > 1 ? $"{baseName} ({index})" : baseName;
            if (!isTaken(candidate))
                return candidate;
        }

        return baseName;
    }

    private static string Normalize(string path) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
}
