using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

#region A source with data and a command of its own
/// <summary>
/// A file with a review state the source attaches to it, shown in a column of its own.
/// </summary>
/// <remarks>
/// The library never asks a node to notify its changes, its own nodes being rebuilt rather than modified. A node a
/// consumer does modify wants it, and [INotifyPropertyChanged] adds it to a class that already has a base : without it
/// the Review column would not repaint.
/// </remarks>
[INotifyPropertyChanged]
public partial class ReviewedFile : PhysicalFile
{
    public ReviewedFile(FileInfo info, IExplorerDirectory? parent) : base(info, parent) { }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Review))]
    public partial bool IsReviewed { get; set; }

    public string Review => IsReviewed ? "Reviewed" : "To review";
}

/// <summary>
/// A physical source building ReviewedFile instead of PhysicalFile, with a command of its own.
/// </summary>
/// <remarks>
/// Nothing in the library knows about MarkReviewedCommand : a menu item reaches it through Session.Source, bindings
/// resolving against the runtime type. Adding a command to the explorer takes no registration.
/// </remarks>
public partial class ReviewSource : PhysicalExplorerSource
{
    public ReviewSource(string rootPath) : base(rootPath) { }

    /// <summary>The source builds the nodes, so it decides their type.</summary>
    protected override IExplorerNode CreateFile(FileInfo info, IExplorerDirectory parent)
        => new ReviewedFile(info, parent);

    [RelayCommand(CanExecute = nameof(CanMarkReviewed))]
    private void MarkReviewed(IReadOnlyList<IExplorerNode>? nodes)
    {
        foreach (var file in nodes!.OfType<ReviewedFile>())
            file.IsReviewed = true;
    }

    private static bool CanMarkReviewed(IReadOnlyList<IExplorerNode>? nodes)
        => nodes?.OfType<ReviewedFile>().Any() == true;
}
#endregion

#region A source with nothing on the disk
public class NoteFile : ExplorerNode
{
    public NoteFile(string name, string content, IExplorerDirectory? parent)
        : base(name, DateTime.Now, parent)
        => Content = content;

    public string Content { get; }

    public override long? Size => Content.Length;
}

public class NoteFolder : ExplorerDirectory
{
    public NoteFolder(string name, IExplorerDirectory? parent) : base(name, DateTime.Now, parent) { }
}

/// <summary>
/// A source with no disk behind it : two members, no mutation.
/// </summary>
/// <remarks>
/// Renaming, deleting, pasting, "Show in file explorer" and dragging out to Windows all stay unavailable on their own,
/// nothing having been opted into and no node carrying a file system path.
/// </remarks>
public class NotesSource : IExplorerSource
{
    public IExplorerDirectory? CreateRoot()
    {
        var root = new NoteFolder("Notes", null);

        var ideas = new NoteFolder("Ideas", root);
        ideas.Children.Add(new NoteFile("dark-mode.md", "Flip the theme at runtime, no restart.", ideas));
        ideas.Children.Add(new NoteFile("shortcuts.md", "F2 renames, Del recycles, Ctrl+C copies.", ideas));

        root.Children.Add(ideas);
        root.Children.Add(new NoteFile("readme.md", "Nothing here exists on the disk.", root));
        return root;
    }

    public Task<IEnumerable<IExplorerNode>> GetChildrenAsync(
        IExplorerDirectory directory,
        CancellationToken cancellationToken)
        => Task.FromResult<IEnumerable<IExplorerNode>>(directory.Children.ToArray());
}
#endregion

public class ExplorerSourceSamplesViewModel : ObservableObject
{
    public ExplorerSession ReviewSession { get; }

    public ExplorerSession NotesSession { get; }

    private string? _openedNote;

    /// <summary>Content of the note last opened, a virtual node having no application to be handed to.</summary>
    public string? OpenedNote { get => _openedNote; private set => SetProperty(ref _openedNote, value); }

    public ExplorerSourceSamplesViewModel()
    {
        ReviewSession = new ExplorerSession(new ReviewSource(Directory.GetCurrentDirectory()));
        NotesSession = new ExplorerSession(new NotesSource());
        _ = ReviewSession.OpenRootAsync();
        _ = NotesSession.OpenRootAsync();
    }

    public void OpenNote(IExplorerNode node)
    {
        if (node is NoteFile note)
            OpenedNote = note.Content;
    }

    public string CustomSourceCode =>
        """
        public class ReviewedFile(FileInfo info, IExplorerDirectory? parent) : PhysicalFile(info, parent)
        {
            public bool IsReviewed { get; set; }
            public string Review => IsReviewed ? "Reviewed" : "To review";
        }

        public partial class ReviewSource(string rootPath) : PhysicalExplorerSource(rootPath)
        {
            // The source builds the nodes, so it decides their type.
            protected override IExplorerNode CreateFile(FileInfo info, IExplorerDirectory parent)
                => new ReviewedFile(info, parent);

            [RelayCommand]
            private void MarkReviewed(IReadOnlyList<IExplorerNode>? nodes)
            {
                foreach (var file in nodes!.OfType<ReviewedFile>())
                    file.IsReviewed = true;
            }
        }
        """
        + """


        <fileExplorer:ExplorerList Session="{Binding ReviewSession}" VisibleNodes="Files">
            <fileExplorer:ExplorerList.Resources>
                <!-- Keyed on the node type, so it is found before the default keyed on PhysicalFile. -->
                <DataTemplate x:Key="{base:ContextMenuTemplateKey local:ReviewedFile}">
                    <ContextMenu>
                        <MenuItem Header="Mark as reviewed"
                                  Command="{Binding Session.Source.MarkReviewedCommand}"
                                  CommandParameter="{Binding Nodes}" />
                        <Separator />
                        <MenuItem Header="Rename"
                                  Command="{Binding Owner.BeginRenameCommand}"
                                  CommandParameter="{Binding Node}" />
                    </ContextMenu>
                </DataTemplate>
            </fileExplorer:ExplorerList.Resources>
            <fileExplorer:ExplorerList.ExtraColumns>
                <GridViewColumn Header="Review" DisplayMemberBinding="{Binding Review}" />
            </fileExplorer:ExplorerList.ExtraColumns>
        </fileExplorer:ExplorerList>
        """;

    public string VirtualSourceCode =>
        """
        public class NoteFile(string name, string content, IExplorerDirectory? parent)
            : ExplorerNode(name, DateTime.Now, parent)
        {
            public string Content { get; } = content;
            public override long? Size => Content.Length;
        }

        // Two members, no mutation : everything not opted into stays unavailable on its own.
        public class NotesSource : IExplorerSource
        {
            public IExplorerDirectory? CreateRoot()
            {
                var root = new NoteFolder("Notes", null);
                root.Children.Add(new NoteFile("readme.md", "Nothing here is on the disk.", root));
                return root;
            }

            public Task<IEnumerable<IExplorerNode>> GetChildrenAsync(
                IExplorerDirectory directory, CancellationToken cancellationToken)
                => Task.FromResult<IEnumerable<IExplorerNode>>(directory.Children.ToArray());
        }
        """
        + """


        <fileExplorer:ExplorerTree Session="{Binding NotesSession}"
                                   VisibleNodes="All"
                                   NodeActivated="OnNoteActivated" />
        """;

    public string RemoteSourceCode =>
        """
        // A source for anything else follows the same shape. Only the two required members, plus the one
        // capability this one has : taking files in.
        public class FtpSource(FtpClient client) : IExplorerSource
        {
            public IExplorerDirectory? CreateRoot() => new FtpDirectory("/", null);

            public async Task<IEnumerable<IExplorerNode>> GetChildrenAsync(
                IExplorerDirectory directory, CancellationToken cancellationToken)
                => (await client.ListAsync(directory.Path, cancellationToken))
                    .Select(entry => entry.IsDirectory
                        ? new FtpDirectory(entry.Name, directory)
                        : (IExplorerNode)new FtpFile(entry.Name, entry.Size, directory));

            // Uploading : a drop of files coming from Windows carries their paths.
            public bool CanAccept(ExplorerTransfer transfer, IExplorerDirectory target)
                => transfer.Paths.Count > 0;

            public Task AcceptAsync(
                ExplorerTransfer transfer, IExplorerDirectory target, CancellationToken cancellationToken)
                => client.UploadAsync(transfer.Paths, target.Path, cancellationToken);
        }
        """;
}
