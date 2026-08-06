using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Controls.Base;

/// <summary>
/// Naming a node : renaming an existing one, and naming a folder just created.
/// </summary>
/// <remarks>
/// The editor is a popup placed over the item container, rather than a text box inside the node template. The node
/// template is the extension point a consumer replaces, so an editor living in it would silently disappear with every
/// custom template, and would have to be repeated in the cell template of the list and in the header template of the
/// tree. A popup works the same for both controls and for any template.
/// <para>
/// It is declared as a template part rather than built in code : a consumer keeps control of it, the themed text box
/// style applies naturally, and a template that omits it simply has no renaming.
/// </para>
/// </remarks>
public abstract partial class ExplorerNodesControl
{
    protected const string PartRenameEditor = "PART_RenameEditor";
    protected const string PartRenameEditorBox = "PART_RenameEditorBox";

    private Popup? _renameEditor;
    private TextBox? _renameEditorBox;

    /// <summary>
    /// Closing the popup makes the box lose the focus, which is itself a commit : without this the commit would run
    /// twice, the second time on a node that has already been renamed.
    /// </summary>
    private bool _isCommittingRename;

    #region Dependency Property
    private static readonly DependencyPropertyKey EditingNodePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(EditingNode),
        typeof(IExplorerNode),
        typeof(ExplorerNodesControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty EditingNodeProperty = EditingNodePropertyKey.DependencyProperty;
    #endregion

    /// <summary>Node whose name is being edited, null the rest of the time.</summary>
    public IExplorerNode? EditingNode => (IExplorerNode?)GetValue(EditingNodeProperty);

    /// <summary>
    /// Starts editing the name of a node. A context menu reaches it through
    /// <see cref="ExplorerMenuContext.Owner"/>, the editor belonging to the control and not to the session.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanBeginRename))]
    public void BeginRename(IExplorerNode? node)
    {
        if (node == null || _renameEditor == null || _renameEditorBox == null || Session?.CanRename(node) != true)
            return;

        SetValue(EditingNodePropertyKey, node);
        ScrollToNode(node);

        // The container of a node just created does not exist yet, and one scrolled to has not been arranged : both
        // are only reachable once the layout pass triggered above has run.
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => ShowEditor(node));
    }

    private bool CanBeginRename(IExplorerNode? node)
        => node != null && _renameEditor != null && Session?.CanRename(node) == true;

    /// <summary>
    /// Creates a folder in a directory and starts editing its name, the way Windows does : that is what makes a
    /// naming dialog unnecessary.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCreateDirectory))]
    public async Task CreateDirectoryAsync(IExplorerDirectory? parent)
    {
        if (parent == null || Session == null || !Session.CanCreateDirectory(parent))
            return;

        var created = await Session.CreateDirectoryAsync(parent, Session.Source.GetNewDirectoryName(parent));
        if (created != null)
            BeginRename(created);
    }

    private bool CanCreateDirectory(IExplorerDirectory? parent)
        => parent != null && Session?.CanCreateDirectory(parent) == true;

    /// <summary>
    /// Brings a node into view before its editor is placed. Only a derived control knows how.
    /// </summary>
    protected virtual void ScrollToNode(IExplorerNode node) { }

    /// <summary>
    /// Takes the editor parts out of a freshly applied template. Called by
    /// <see cref="ExplorerNodesControl.OnApplyTemplate"/>.
    /// </summary>
    private void ApplyEditorTemplateParts()
    {
        if (_renameEditorBox != null)
        {
            _renameEditorBox.PreviewKeyDown -= OnRenameEditorBoxPreviewKeyDown;
            _renameEditorBox.LostFocus -= OnRenameEditorBoxLostFocus;
        }

        CloseEditor();

        _renameEditor = GetTemplateChild(PartRenameEditor) as Popup;
        _renameEditorBox = GetTemplateChild(PartRenameEditorBox) as TextBox;

        if (_renameEditorBox != null)
        {
            // PreviewKeyDown : the text box handles Enter and Escape itself, so they must be caught before it does.
            _renameEditorBox.PreviewKeyDown += OnRenameEditorBoxPreviewKeyDown;
            _renameEditorBox.LostFocus += OnRenameEditorBoxLostFocus;
        }
    }

    private void ShowEditor(IExplorerNode node)
    {
        if (_renameEditor == null || _renameEditorBox == null || EditingNode != node)
            return;

        var container = FindContainer(node);
        if (container == null)
        {
            SetValue(EditingNodePropertyKey, null);
            return;
        }

        _renameEditorBox.Text = node.Name;
        _renameEditorBox.MinWidth = Math.Max(container.ActualWidth, 120);

        _renameEditor.PlacementTarget = container;
        _renameEditor.Placement = PlacementMode.Relative;
        _renameEditor.IsOpen = true;

        // The content of a popup is not focusable until it has been rendered.
        Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            () =>
            {
                if (!_renameEditor.IsOpen)
                    return;

                _renameEditorBox.Focus();
                SelectNameWithoutExtension(_renameEditorBox, node);
            });
    }

    /// <summary>
    /// Preselects the part of the name the user is likely to change, leaving the extension out of the selection the
    /// way the file explorer does.
    /// </summary>
    private static void SelectNameWithoutExtension(TextBox box, IExplorerNode node)
    {
        if (node is IExplorerDirectory)
        {
            box.SelectAll();
            return;
        }

        string extension = Path.GetExtension(box.Text);
        box.Select(0, box.Text.Length - extension.Length);
    }

    private void OnRenameEditorBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = true;
                _ = CommitRenameAsync();
                break;
            case Key.Escape:
                e.Handled = true;
                CloseEditor();
                Focus();
                break;
        }
    }

    private void OnRenameEditorBoxLostFocus(object sender, RoutedEventArgs e) => _ = CommitRenameAsync();

    private async Task CommitRenameAsync()
    {
        if (_isCommittingRename)
            return;

        var node = EditingNode;
        if (node == null || _renameEditorBox == null || Session == null)
            return;

        _isCommittingRename = true;
        try
        {
            string newName = _renameEditorBox.Text.Trim();
            CloseEditor();

            if (newName.Length > 0 && newName != node.Name)
                await Session.RenameAsync(node, newName);
        }
        catch (Exception)
        {
            // A failed rename is already reported through ExplorerSession.LastError.
        }
        finally
        {
            _isCommittingRename = false;
        }
    }

    private void CloseEditor()
    {
        if (_renameEditor != null)
            _renameEditor.IsOpen = false;

        SetValue(EditingNodePropertyKey, null);
    }

    /// <summary>
    /// Item container displaying a node, searched through the visual tree.
    /// </summary>
    /// <remarks>
    /// <see cref="ItemContainerGenerator.ContainerFromItem"/> is not used : it only knows the containers generated by
    /// <see cref="ItemsHost"/> itself, and in a hierarchy a container is generated by its parent item.
    /// </remarks>
    private FrameworkElement? FindContainer(IExplorerNode node)
        => ItemsHost == null ? null : FindContainerIn(ItemsHost, node);

    private static FrameworkElement? FindContainerIn(DependencyObject parent, IExplorerNode node)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);

            if (child is FrameworkElement element
                && ReferenceEquals(element.DataContext, node)
                && ItemsControl.ItemsControlFromItemContainer(element) != null)
                return element;

            if (FindContainerIn(child, node) is { } found)
                return found;
        }

        return null;
    }
}
