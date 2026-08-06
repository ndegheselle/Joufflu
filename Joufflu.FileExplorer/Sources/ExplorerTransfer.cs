using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Sources;

/// <summary>
/// Content being carried into a directory, by a drop or by a paste : the two are the same thing, so they share a
/// payload.
/// </summary>
/// <remarks>
/// A transfer describes its content twice, and a source reads whichever half it understands : a file system source
/// only ever looks at <see cref="Paths"/>, a remote source uses <see cref="Nodes"/> for a move of its own and
/// <see cref="Paths"/> for an upload. Nothing here negotiates between two sources ; the target decides, in
/// <see cref="IExplorerSource.CanAccept"/>.
/// </remarks>
public sealed class ExplorerTransfer
{
    private ExplorerTransfer(
        IReadOnlyList<IExplorerNode> nodes,
        IReadOnlyList<string> paths,
        bool isMove,
        ExplorerSession? origin)
    {
        Nodes = nodes;
        Paths = paths;
        IsMove = isMove;
        Origin = origin;
    }

    /// <summary>Nodes being carried, empty when the content comes from outside the application.</summary>
    public IReadOnlyList<IExplorerNode> Nodes { get; }

    /// <summary>
    /// File system paths of the content : the dropped files for a drop coming from Windows, or the paths of
    /// <see cref="Nodes"/> when all of them have one. Empty when nothing being carried exists on this machine.
    /// </summary>
    public IReadOnlyList<string> Paths { get; }

    /// <summary>True when the content leaves its origin, false when it is duplicated.</summary>
    public bool IsMove { get; }

    /// <summary>
    /// Session the content was taken from, null when it comes from outside the application. Compared against the
    /// target session to tell a move inside one explorer from a copy between two.
    /// </summary>
    public ExplorerSession? Origin { get; }

    /// <summary>Content taken from an explorer : the nodes, plus the paths of those that have one.</summary>
    public static ExplorerTransfer FromNodes(
        IReadOnlyList<IExplorerNode> nodes,
        bool isMove,
        ExplorerSession? origin)
    {
        string[] paths = nodes.OfType<IPhysicalExplorerNode>().Select(node => node.FileSystemPath).ToArray();
        // All or nothing : a partial list would silently drop the virtual nodes of a mixed selection.
        return new ExplorerTransfer(nodes, paths.Length == nodes.Count ? paths : [], isMove, origin);
    }

    /// <summary>Content coming from Windows : a drop from the file explorer, a paste of the clipboard.</summary>
    public static ExplorerTransfer FromPaths(IReadOnlyList<string> paths, bool isMove)
        => new([], paths, isMove, null);
}
