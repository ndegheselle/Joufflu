using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.FileExplorer.Controls;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Helpers;

namespace Joufflu.FileExplorer.Sources;

/// <summary>
/// Where the user is inside a source : the opened directory, the path leading to it, the history of the visited ones,
/// and the commands acting on them. Several controls sharing one session all show the same directory, which is how an
/// <see cref="Explorer"/> keeps its tree, its list and its control bar together.
/// </summary>
/// <remarks>
/// This is not something to reimplement : a source is the extension point, a session only mediates between it and the
/// controls. It is left open all the same, so that a consumer may add commands of its own to it.
/// <para>
/// Every method must be called from the dispatcher thread : the session fills the
/// <see cref="IExplorerDirectory.Children"/> collections the controls are bound to.
/// </para>
/// </remarks>
public partial class ExplorerSession : ObservableObject
{
    /// <summary>
    /// Directories opened so far in navigation order, <see cref="HistoryIndex"/> pointing at the current one.
    /// </summary>
    private readonly List<IExplorerDirectory> _history = [];

    /// <summary>
    /// Directories being read right now, so that a tree expanding a level twice doesn't read it twice.
    /// </summary>
    private readonly HashSet<IExplorerDirectory> _loading = [];

    /// <summary>
    /// Cancels the navigation in flight. A new one supersedes it rather than queueing behind it : the user asking for
    /// another directory means they lost interest in the previous one.
    /// </summary>
    private CancellationTokenSource? _navigationCancellation;

    /// <summary>Operations running, <see cref="IsBusy"/> being true while there is at least one.</summary>
    private int _runningOperations;

    private IReadOnlyList<string> _clipboardPaths = [];
    private bool _clipboardIsMove;

    public ExplorerSession(IExplorerSource source)
    {
        Source = source;
        HistoryIndex = -1;
    }

    /// <summary>
    /// Source of the nodes. Public so that a context menu can reach the commands a consumer added to its own source,
    /// with <c>{Binding Session.Source.MyCommand}</c> : nothing has to be registered for that to work.
    /// </summary>
    public IExplorerSource Source { get; }

    [ObservableProperty]
    public partial IExplorerDirectory? Root { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPath))]
    [NotifyCanExecuteChangedFor(nameof(OpenParentCommand))]
    public partial IExplorerDirectory? Current { get; private set; }

    /// <summary>
    /// Directories leading to <see cref="Current"/>, from its topmost parent to itself, displayed as a breadcrumb by
    /// the <see cref="ExplorerControlBar"/>.
    /// </summary>
    public IReadOnlyList<IExplorerDirectory> CurrentPath => BuildPath(Current);

    /// <summary>True while at least one operation is running, for a progress bar to bind to.</summary>
    [ObservableProperty]
    public partial bool IsBusy { get; private set; }

    /// <summary>
    /// Message of the last failed operation, cleared by the next one that succeeds. Null when all is well.
    /// </summary>
    [ObservableProperty]
    public partial string? LastError { get; private set; }

    /// <summary>
    /// Position in the history. Private, but observable so that Back and Forward re-evaluate themselves instead of
    /// being notified by hand from every place that navigates.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoBackCommand), nameof(GoForwardCommand))]
    private partial int HistoryIndex { get; set; }

    #region Navigation
    /// <summary>
    /// Builds the root of the source and opens it, forgetting the history. Call it once the session is bound.
    /// </summary>
    /// <remarks>
    /// AllowConcurrentExecutions on every navigation command : without it an AsyncRelayCommand reports CanExecute
    /// false while it runs, so the breadcrumb and the Back and Forward buttons would all grey out mid navigation,
    /// which is the opposite of what superseding means.
    /// </remarks>
    [RelayCommand(AllowConcurrentExecutions = true)]
    public async Task OpenRootAsync()
    {
        _history.Clear();
        HistoryIndex = -1;
        GoBackCommand.NotifyCanExecuteChanged();
        GoForwardCommand.NotifyCanExecuteChanged();

        try
        {
            Root = Source.CreateRoot();
        }
        catch (Exception exception)
        {
            Root = null;
            Fail(exception);
            return;
        }

        if (Root != null)
            await OpenCoreAsync(Root, record: true);
    }

    /// <summary>
    /// Reads the children of a directory and makes it the <see cref="Current"/> one.
    /// </summary>
    /// <param name="node">
    /// Typed as a node and not as a directory because a menu, a breadcrumb or a double click hands over whatever it
    /// has, and a RelayCommand throws on a parameter it cannot cast. Anything that is not a directory is ignored.
    /// </param>
    [RelayCommand(AllowConcurrentExecutions = true, CanExecute = nameof(CanOpen))]
    public Task OpenAsync(IExplorerNode? node)
        => node is IExplorerDirectory directory ? OpenCoreAsync(directory, record: true) : Task.CompletedTask;

    private static bool CanOpen(IExplorerNode? node) => node is IExplorerDirectory;

    [RelayCommand(AllowConcurrentExecutions = true, CanExecute = nameof(CanOpenParent))]
    public Task OpenParentAsync()
        => Current?.Parent is { } parent ? OpenCoreAsync(parent, record: true) : Task.CompletedTask;

    private bool CanOpenParent() => Current?.Parent != null;

    [RelayCommand(AllowConcurrentExecutions = true, CanExecute = nameof(CanGoBack))]
    public Task GoBackAsync() => GoToAsync(HistoryIndex - 1);

    private bool CanGoBack() => HistoryIndex > 0;

    [RelayCommand(AllowConcurrentExecutions = true, CanExecute = nameof(CanGoForward))]
    public Task GoForwardAsync() => GoToAsync(HistoryIndex + 1);

    private bool CanGoForward() => HistoryIndex >= 0 && HistoryIndex < _history.Count - 1;

    /// <summary>
    /// Re-reads the children of a directory, <see cref="Current"/> by default.
    /// </summary>
    /// <remarks>
    /// The parameter is nullable so that a toolbar button needs no CommandParameter at all.
    /// </remarks>
    [RelayCommand(AllowConcurrentExecutions = true)]
    public Task RefreshAsync(IExplorerDirectory? directory)
    {
        var target = directory ?? Current;
        return target == null ? Task.CompletedTask : ReloadAsync(target);
    }

    /// <summary>
    /// Reads the children of a directory without navigating to it, for a control showing a hierarchy it only needs
    /// the content of : a tree expanding a level.
    /// </summary>
    public async Task LoadAsync(IExplorerDirectory directory)
    {
        if (!_loading.Add(directory))
            return;

        BeginOperation();
        try
        {
            var children = await Source.GetChildrenAsync(directory, CancellationToken.None);
            Fill(directory, children);
            LastError = null;
        }
        catch (OperationCanceledException)
        { }
        catch (Exception exception)
        {
            Fail(exception);
        }
        finally
        {
            _loading.Remove(directory);
            EndOperation();
        }
    }

    /// <summary>
    /// Opens the directory at an index of the history, leaving the history itself untouched.
    /// </summary>
    private Task GoToAsync(int index)
    {
        if (index < 0 || index >= _history.Count)
            return Task.CompletedTask;

        HistoryIndex = index;
        return OpenCoreAsync(_history[index], record: false);
    }

    private async Task OpenCoreAsync(IExplorerDirectory directory, bool record)
    {
        var cancellation = new CancellationTokenSource();
        _navigationCancellation?.Cancel();
        _navigationCancellation = cancellation;

        BeginOperation();
        try
        {
            var children = await Source.GetChildrenAsync(directory, cancellation.Token);

            // Only the latest navigation may write : an earlier one resuming here has been superseded, and writing
            // Current or the children would tear the state the user is looking at.
            if (cancellation != _navigationCancellation || cancellation.IsCancellationRequested)
                return;

            Fill(directory, children);
            Current = directory;
            if (record)
                Record(directory);
            LastError = null;
        }
        catch (OperationCanceledException)
        { }
        catch (Exception exception)
        {
            if (cancellation == _navigationCancellation)
                Fail(exception);
        }
        finally
        {
            if (cancellation == _navigationCancellation)
                _navigationCancellation = null;
            cancellation.Dispose();
            EndOperation();
        }
    }

    /// <summary>
    /// Records a navigation. Navigating after a few <see cref="GoBackCommand"/> drops the directories that were
    /// ahead, the way a browser does.
    /// </summary>
    private void Record(IExplorerDirectory directory)
    {
        // Reopening the current directory is a refresh, not a navigation.
        if (HistoryIndex >= 0 && _history[HistoryIndex] == directory)
            return;

        _history.RemoveRange(HistoryIndex + 1, _history.Count - HistoryIndex - 1);
        _history.Add(directory);
        HistoryIndex = _history.Count - 1;
    }

    /// <summary>
    /// Directories from the topmost parent of a directory down to itself, itself last.
    /// </summary>
    private static IReadOnlyList<IExplorerDirectory> BuildPath(IExplorerDirectory? directory)
    {
        List<IExplorerDirectory> path = [];
        for (IExplorerDirectory? node = directory; node != null; node = node.Parent)
            path.Insert(0, node);
        return path;
    }
    #endregion

    #region Operations
    [RelayCommand(CanExecute = nameof(CanDelete))]
    public Task DeleteAsync(IReadOnlyList<IExplorerNode>? nodes) => DeleteCoreAsync(nodes, permanent: false);

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public Task DeletePermanentlyAsync(IReadOnlyList<IExplorerNode>? nodes) => DeleteCoreAsync(nodes, permanent: true);

    public bool CanDelete(IReadOnlyList<IExplorerNode>? nodes) => nodes is { Count: > 0 } && Source.CanDelete(nodes);

    public bool CanRename(IExplorerNode node) => Source.CanRename(node);

    public async Task RenameAsync(IExplorerNode node, string newName)
    {
        if (!CanRename(node) || newName == node.Name)
            return;

        BeginOperation();
        try
        {
            await Source.RenameAsync(node, newName, CancellationToken.None);
            LastError = null;
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
        finally
        {
            EndOperation();
        }

        // Outside the try : the reload has to happen even when the rename partly succeeded, so that what is shown
        // matches what is really there.
        if (node.Parent != null)
            await ReloadAsync(node.Parent);
    }

    public bool CanCreateDirectory(IExplorerDirectory parent) => Source.CanCreateDirectory(parent);

    /// <summary>
    /// Creates a directory and returns it, found back in the reloaded parent. Null when it could not be created.
    /// </summary>
    public async Task<IExplorerDirectory?> CreateDirectoryAsync(IExplorerDirectory parent, string name)
    {
        if (!CanCreateDirectory(parent))
            return null;

        BeginOperation();
        try
        {
            await Source.CreateDirectoryAsync(parent, name, CancellationToken.None);
            LastError = null;
        }
        catch (Exception exception)
        {
            Fail(exception);
            return null;
        }
        finally
        {
            EndOperation();
        }

        await ReloadAsync(parent);
        return parent.Children.OfType<IExplorerDirectory>()
            .FirstOrDefault(directory => directory.Name == name);
    }

    public bool CanAccept(ExplorerTransfer transfer, IExplorerDirectory target) => Source.CanAccept(transfer, target);

    /// <summary>
    /// Carries the content of a transfer into a directory, then reloads what it changed : the target, and the origin
    /// too when the content left it.
    /// </summary>
    public async Task AcceptAsync(ExplorerTransfer transfer, IExplorerDirectory target)
    {
        if (!CanAccept(transfer, target))
            return;

        BeginOperation();
        try
        {
            await Source.AcceptAsync(transfer, target, CancellationToken.None);
            LastError = null;
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
        finally
        {
            EndOperation();
        }

        await ReloadAsync(target);

        if (transfer.IsMove && transfer.Origin != null && transfer.Origin != this)
            await transfer.Origin.RefreshAsync(null);
    }

    [RelayCommand(CanExecute = nameof(CanTransferOut))]
    public void Copy(IReadOnlyList<IExplorerNode>? nodes) => SetClipboard(nodes, isMove: false);

    [RelayCommand(CanExecute = nameof(CanTransferOut))]
    public void Cut(IReadOnlyList<IExplorerNode>? nodes) => SetClipboard(nodes, isMove: true);

    /// <summary>Puts the paths of the nodes in the clipboard as text, one per line.</summary>
    [RelayCommand(CanExecute = nameof(CanTransferOut))]
    public void CopyPath(IReadOnlyList<IExplorerNode>? nodes)
    {
        if (nodes == null || nodes.Count == 0)
            return;

        try
        {
            System.Windows.Clipboard.SetText(
                string.Join(
                    Environment.NewLine,
                    nodes.OfType<IPhysicalExplorerNode>().Select(node => node.FileSystemPath)));
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    /// <summary>
    /// Only the nodes Windows can be handed a path to leave the explorer, which is also what makes cut, copy and
    /// dragging out disappear for a node that exists nowhere on the disk.
    /// </summary>
    private static bool CanTransferOut(IReadOnlyList<IExplorerNode>? nodes)
        => nodes is { Count: > 0 } && nodes.All(node => node is IPhysicalExplorerNode);

    [RelayCommand(CanExecute = nameof(CanPaste))]
    public Task PasteAsync(IExplorerDirectory? target)
    {
        var directory = target ?? Current;
        if (directory == null || _clipboardPaths.Count == 0)
            return Task.CompletedTask;

        return AcceptAsync(ExplorerTransfer.FromPaths(_clipboardPaths, _clipboardIsMove), directory);
    }

    private bool CanPaste(IExplorerDirectory? target)
    {
        var directory = target ?? Current;
        return directory != null
            && _clipboardPaths.Count > 0
            && CanAccept(ExplorerTransfer.FromPaths(_clipboardPaths, _clipboardIsMove), directory);
    }

    /// <summary>
    /// Re-reads what the clipboard holds and notifies <see cref="PasteCommand"/>.
    /// </summary>
    /// <remarks>
    /// Called when a menu opens rather than from a CanExecute : reading the clipboard is an interop call that may
    /// block or throw, and a RelayCommand does not follow CommandManager.RequerySuggested anyway.
    /// </remarks>
    public void RefreshClipboardState()
    {
        _clipboardPaths = ExplorerClipboard.GetPaths(out _clipboardIsMove);
        PasteCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(IsPhysical))]
    public void ShowInFileExplorer(IExplorerNode? node)
    {
        if (node is IPhysicalExplorerNode physical)
            SystemShell.ShowInFileExplorer(physical.FileSystemPath);
    }

    [RelayCommand(CanExecute = nameof(IsPhysical))]
    public void OpenWithDefaultApplication(IExplorerNode? node)
    {
        if (node is IPhysicalExplorerNode physical)
            SystemShell.OpenWithDefaultApplication(physical.FileSystemPath);
    }

    private static bool IsPhysical(IExplorerNode? node) => node is IPhysicalExplorerNode;

    private async Task DeleteCoreAsync(IReadOnlyList<IExplorerNode>? nodes, bool permanent)
    {
        if (nodes == null || !CanDelete(nodes))
            return;

        // Captured before the delete : the parents are what has to be reloaded afterwards.
        IExplorerDirectory[] parents = nodes.Select(node => node.Parent)
            .OfType<IExplorerDirectory>()
            .Distinct()
            .ToArray();

        BeginOperation();
        try
        {
            await Source.DeleteAsync(nodes, permanent, CancellationToken.None);
            LastError = null;
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
        finally
        {
            EndOperation();
        }

        foreach (var parent in parents)
            await ReloadAsync(parent);
    }

    private void SetClipboard(IReadOnlyList<IExplorerNode>? nodes, bool isMove)
    {
        if (nodes == null)
            return;

        string[] paths = nodes.OfType<IPhysicalExplorerNode>().Select(node => node.FileSystemPath).ToArray();
        if (!ExplorerClipboard.TrySetPaths(paths, isMove))
            LastError = "The clipboard could not be written to.";
    }

    private async Task ReloadAsync(IExplorerDirectory directory)
    {
        BeginOperation();
        try
        {
            var children = await Source.GetChildrenAsync(directory, CancellationToken.None);
            Fill(directory, children);
            LastError = null;
        }
        catch (OperationCanceledException)
        { }
        catch (Exception exception)
        {
            Fail(exception);
        }
        finally
        {
            EndOperation();
        }
    }
    #endregion

    /// <summary>
    /// Publishes the children of a directory.
    /// </summary>
    /// <remarks>
    /// Kept as the same collection instance rather than replaced, so that the collection views the controls build on
    /// top of it survive a reload. Mutating it is why every method of the session belongs to the dispatcher thread : an
    /// ObservableCollection a CollectionView is attached to throws when it is touched from anywhere else.
    /// </remarks>
    private static void Fill(IExplorerDirectory directory, IEnumerable<IExplorerNode> children)
    {
        directory.Children.Clear();
        foreach (var child in children)
            directory.Children.Add(child);
    }

    private void BeginOperation()
    {
        if (++_runningOperations == 1)
            IsBusy = true;
    }

    private void EndOperation()
    {
        if (--_runningOperations == 0)
            IsBusy = false;
    }

    private void Fail(Exception exception) => LastError = exception.Message;
}
