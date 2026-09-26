using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

/// <summary>
/// A file of the application and not of the disk : it carries a path of a file that doesn't exist, so the explorer
/// displays it and copies its path like any other node, along with a state of its own the disk knows nothing about.
/// </summary>
/// <remarks>
/// An <see cref="IExplorerNode"/> and not an <see cref="IExplorerFile"/> : nothing on the disk stands behind it, so
/// it has no size, and the size column of the list stays empty on its row.
/// </remarks>
public class VirtualFile : ObservableObject, IExplorerNode
{
    public string Path { get; set; }

    public string Name { get; }

    public DateTime ModifiedAt { get; }

    /// <summary>
    /// Directory the virtual file belongs to. Settable : a directory is read again into new instances whenever its
    /// parent is reloaded, and the virtual file outlives that reload.
    /// </summary>
    public IExplorerDirectory? Parent { get; set; }

    /// <summary>
    /// State of the node, of the application only : displayed in a column of its own and toggled from the context
    /// menu of the virtual files.
    /// </summary>
    public bool IsPinned
    {
        get => isPinned;
        set => SetProperty(ref isPinned, value);
    }
    private bool isPinned;

    public VirtualFile(IExplorerDirectory parent, string name)
    {
        Parent = parent;
        Name = name;
        // Associated to a path inside its directory, even though no file of the disk stands behind it.
        Path = System.IO.Path.Combine(parent.Path, name);
        ModifiedAt = parent.ModifiedAt;
    }
}

/// <summary>
/// The files and the directories of <see cref="FileSystemSource"/>, with a <see cref="VirtualFile"/> added in every
/// directory : a source only has to hand its own nodes over along with the ones it reads, the controls displaying
/// them through the templates keyed on their type.
/// </summary>
public class VirtualFilesSource : FileSystemSource
{
    private const string VirtualFileName = "notes.md";

    private readonly IToastService? toasts;

    /// <summary>
    /// Virtual files by the path of their directory : a directory is read again every time it is opened, where the
    /// virtual files have to outlive that reload to keep the state they've been given.
    /// </summary>
    private readonly Dictionary<string, VirtualFile> virtualFiles = [];

    public VirtualFilesSource(string rootDirectoryPath, IToastService? toasts) : base(rootDirectoryPath, toasts)
    {
        this.toasts = toasts;
    }

    /// <summary>Adds the virtual file of a directory to the nodes read from the disk.</summary>
    protected override void LoadDirectory(IExplorerDirectory directory, int depth)
    {
        base.LoadDirectory(directory, depth);
        directory.Children.Add(GetVirtualFile(directory));
    }

    /// <summary>
    /// Nothing to hand over to the shell for a virtual file : the application opens it itself, the nodes read from
    /// the disk being left to the source it comes from.
    /// </summary>
    public override Task Open(IExplorerNode node)
    {
        if (node is not VirtualFile virtualFile)
            return base.Open(node);

        toasts?.Info($"{virtualFile.Path} is a virtual file, opened by the application.");
        return Task.CompletedTask;
    }

    private VirtualFile GetVirtualFile(IExplorerDirectory directory)
    {
        if (virtualFiles.TryGetValue(directory.Path, out VirtualFile? file))
        {
            file.Parent = directory;
            return file;
        }

        file = new VirtualFile(directory, VirtualFileName);
        virtualFiles.Add(directory.Path, file);
        return file;
    }
}
