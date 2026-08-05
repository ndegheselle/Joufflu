using System.Collections;
using System.Runtime.InteropServices;

namespace Joufflu.FileExplorer.Data;

/// <summary>
/// Sorts nodes like the Windows explorer : directories first, then a natural (digit aware) name comparison.
/// </summary>
public partial class ExplorerNodeComparer : IComparer<IExplorerNode>, IComparer
{
    public static readonly ExplorerNodeComparer Default = new ExplorerNodeComparer();

    [DllImport("shlwapi.dll", EntryPoint = "StrCmpLogicalW", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogical(string x, string y);

    public int Compare(IExplorerNode? x, IExplorerNode? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (x is null)
            return -1;
        if (y is null)
            return 1;

        int typeComparison = IsDirectory(y).CompareTo(IsDirectory(x));
        if (typeComparison != 0)
            return typeComparison;

        return StrCmpLogical(x.Name, y.Name);
    }

    public int Compare(object? x, object? y) => Compare(x as IExplorerNode, y as IExplorerNode);

    private static bool IsDirectory(IExplorerNode node) => node is IExplorerDirectory;
}
