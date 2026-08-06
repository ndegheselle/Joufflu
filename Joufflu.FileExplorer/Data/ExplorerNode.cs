using System.Collections.ObjectModel;
using Joufflu.FileExplorer.Controls;

namespace Joufflu.FileExplorer.Data;

public interface IExplorerNode
{
    public string Name { get; }
    public DateTime ModifiedAt { get; }

    /// <summary>
    /// Logical path of the node, always present : it identifies the node inside the source that produced it, and is
    /// meaningful for a node living in an archive, on a server or only in memory. It is NOT necessarily a path on this
    /// machine ; see <see cref="IPhysicalExplorerNode.FileSystemPath"/> for that.
    /// </summary>
    public string Path { get; }

    /// <summary>Size in bytes, null for a directory and for a node that has no size of its own.</summary>
    public long? Size { get; }

    /// <summary>
    /// Directory containing the node, null for the root of a source. Walked up by the navigation to the parent
    /// folder and by the breadcrumb of the <see cref="ExplorerControlBar"/>.
    /// </summary>
    public IExplorerDirectory? Parent { get; }
}

public interface IExplorerDirectory : IExplorerNode
{
    /// <summary>
    /// Nodes contained by the directory. The collection belongs to the directory but is filled by the session, on the
    /// dispatcher thread only : a source returns its children instead of adding them here.
    /// </summary>
    public ObservableCollection<IExplorerNode> Children { get; }
}

/// <summary>
/// Node backed by a real file or folder of this machine. Implementing it is what enables the operations handing a
/// path over to Windows : dragging out to the file explorer, the clipboard, "Show in file explorer", "Open with".
/// A node that doesn't implement it makes all of them disappear on their own.
/// </summary>
public interface IPhysicalExplorerNode : IExplorerNode
{
    public string FileSystemPath { get; }
}

/// <summary>
/// Base of the nodes : builds <see cref="Path"/> from the parent chain, and carries the default visuals.
/// </summary>
/// <remarks>
/// Deriving from it isn't required, but it is worth it twice. <see cref="Path"/> and <see cref="Size"/> have to be
/// real members of the class for a binding to see them, a default interface implementation being invisible to WPF ;
/// and the implicit <c>DataTemplate</c> lookup walks base classes, so a node deriving from it gets the default node
/// visual of the library for free.
/// </remarks>
public abstract class ExplorerNode : IExplorerNode
{
    protected ExplorerNode(string name, DateTime modifiedAt, IExplorerDirectory? parent)
    {
        Name = name;
        ModifiedAt = modifiedAt;
        Parent = parent;
    }

    public string Name { get; }

    public DateTime ModifiedAt { get; }

    public IExplorerDirectory? Parent { get; }

    public virtual string Path => Parent == null ? Name : $"{Parent.Path}/{Name}";

    public virtual long? Size => null;

    public override string ToString() => Path;
}

public abstract class ExplorerDirectory : ExplorerNode, IExplorerDirectory
{
    protected ExplorerDirectory(string name, DateTime modifiedAt, IExplorerDirectory? parent)
        : base(name, modifiedAt, parent)
    { }

    public ObservableCollection<IExplorerNode> Children { get; } = [];
}
