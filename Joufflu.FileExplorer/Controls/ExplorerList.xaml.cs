using Joufflu.FileExplorer.Controls.Base;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;
using Joufflu.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Joufflu.FileExplorer.Controls;

/// <summary>
/// Lists the nodes of the opened folder in a <see cref="ListView"/>, opening a folder on double click.
/// </summary>
public class ExplorerList : Control
{
    #region Dependency Properties

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source), typeof(IExplorerSource), typeof(ExplorerList),
            new PropertyMetadata(null,
                (d, e) => ((ExplorerList)d).OnSourceChanged(e.OldValue as IExplorerSource,
                    e.NewValue as IExplorerSource)));

    public static readonly DependencyProperty VisibleNodesProperty = DependencyProperty.Register(
        nameof(VisibleNodes),
        typeof(ExplorerNodeKinds),
        typeof(ExplorerList),
        new FrameworkPropertyMetadata(ExplorerNodeKinds.All, (d, e) => ((ExplorerList)d).OnVisibleNodesChanged()));

    public static readonly DependencyPropertyKey ViewPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(View),
        typeof(ICollectionView),
        typeof(ExplorerList),
        new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty ViewProperty = ViewPropertyKey.DependencyProperty;

    #endregion

    /// <summary>
    /// Source of the explorer
    /// </summary>
    public IExplorerSource Source
    {
        get => (IExplorerSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Kinds of node the control shows, <see cref="ExplorerNodeKinds.All"/> by default. Set it to
    /// <see cref="ExplorerNodeKinds.Directories"/> or <see cref="ExplorerNodeKinds.Files"/> to display only one.
    /// </summary>
    public ExplorerNodeKinds VisibleNodes
    {
        get => (ExplorerNodeKinds)GetValue(VisibleNodesProperty);
        set => SetValue(VisibleNodesProperty, value);
    }

    public ICollectionView? View
    {
        get => (ICollectionView?)GetValue(ViewProperty);
        private set => SetValue(ViewPropertyKey, value);
    }

    protected const string PartItemsHost = "PART_ItemsHost";
    protected ListView? ItemsHost { get; private set; }
    private readonly IComparer comparer = ExplorerNodeComparer.Default;

    static ExplorerList()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ExplorerList),
            new FrameworkPropertyMetadata(typeof(ExplorerList)));
    }

    public ExplorerList()
    {
        // Default context menu to fix the first right click
        this.ContextMenu = new ContextMenu();
        ContextMenuOpening += ExplorerList_ContextMenuOpening;
        MouseDoubleClick += ExplorerList_MouseDoubleClick;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        ItemsHost = GetTemplateChild(PartItemsHost) as ListView;
    }

    #region On dependency property changed

    private void OnSourceChanged(IExplorerSource? previous, IExplorerSource? source)
    {
        ICollectionView? CreateView()
        {
            return source?.Current == null
                ? null
                : new ListCollectionView(source.Current.Children) { CustomSort = comparer, Filter = FilterNode };
        }

        void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IExplorerSource.Current))
                View = CreateView();
        }

        // Update view and track then source change
        if (previous != null)
            previous.PropertyChanged -= OnSourcePropertyChanged;
        if (source != null)
        {
            source.PropertyChanged += OnSourcePropertyChanged;
            View = CreateView();
        }
    }

    private void OnVisibleNodesChanged()
    {
        View?.Refresh();
    }

    #endregion

    #region UI events
    private void ExplorerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var selected = ItemsHost?.SelectedItem as IExplorerNode;
        if (selected == null)
            return;

        e.Handled = true;
    }

    private void ExplorerList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // Ignore listview columns header
        if (ItemsHost == null || MoreVisualTreeHelper.FindParent<GridViewColumnHeader>(e.OriginalSource as DependencyObject) != null)
        {
            e.Handled = true;
            return;
        }

        IExplorerNode? target = ItemsHost.SelectedItem as IExplorerNode;
        IReadOnlyList<IExplorerNode> nodes = ItemsHost.SelectedItems.Cast<IExplorerNode>().ToList();
        MenuScope scope = ItemsHost.SelectedItems.Count > 1 ? MenuScope.Multiple : MenuScope.Single;
        // If outside of a row open on the current folder
        if (MoreVisualTreeHelper.FindParent<ListViewItem>(e.OriginalSource as DependencyObject) == null)
        {
            target = Source.Current;
            scope = MenuScope.None;
        }

        if (target == null)
        {
            e.Handled = true;
            return;
        }

        var template = FindContextMenuTemplate(target.GetType(), scope);

        if (template?.LoadContent() is not ContextMenu menu)
        {
            e.Handled = true;
            return;
        }

        var element = (FrameworkElement)sender;
        menu.DataContext = new ExplorerMenuContext(Source, nodes);
        element.ContextMenu = menu;
    }
    #endregion

    #region Context menu
    /// <summary>
    /// Searches the context menu template of a node type, from the most specific type to <see cref="object"/>.
    /// </summary>
    private DataTemplate? FindContextMenuTemplate(Type nodeType, MenuScope scope)
    {
        foreach (var type in GetTypeCandidates(nodeType))
        {
            if (TryFindResource(new ContextMenuTemplateKey(type) { Scope = scope }) is DataTemplate template)
                return template;
        }

        return null;
    }

    private static IEnumerable<Type> GetTypeCandidates(Type nodeType)
    {
        for (Type? type = nodeType; type != null && type != typeof(object); type = type.BaseType)
            yield return type;

        foreach (var interfaceType in nodeType.GetInterfaces())
            yield return interfaceType;

        yield return typeof(object);
    }
    #endregion

    // TODO : CONTEXT MENU on nodes and outside
    // TODO : SELECTION Handle selection changed on listview to buble event
    // TODO : OPEN Handle double click on nodes

    /// <summary>Keeps only the nodes whose kind is in <see cref="ExplorerNodesControl.VisibleNodes"/>.</summary>
    private bool FilterNode(object item) => item is IExplorerNode node && VisibleNodes.Includes(node);
}