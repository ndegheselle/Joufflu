using Joufflu.FileExplorer.Controls.Base;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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
        typeof(ExplorerNodesControl),
        new FrameworkPropertyMetadata(ExplorerNodeKinds.All, (d, e) => ((ExplorerList)d).OnVisibleNodesChanged()));

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

    public ICollectionView? View { get; private set; }

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
        var source = e.OriginalSource as DependencyObject;
        var stopAt = sender as DependencyObject;

        var context = FindDataContext<IExplorerNode>(source, stopAt);
        if (context != null)
        {
            e.Handled = true;
        }
    }

    private void ExplorerList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {

    }
    #endregion

    // TODO : CONTEXT MENU on nodes and outside
    // TODO : SELECTION Handle selection changed on listview to buble event
    // TODO : OPEN Handle double click on nodes

    /// <summary>Keeps only the nodes whose kind is in <see cref="ExplorerNodesControl.VisibleNodes"/>.</summary>
    private bool FilterNode(object item) => item is IExplorerNode node && VisibleNodes.Includes(node);

    private static T? FindDataContext<T>(DependencyObject? start, DependencyObject? stop) where T : class
    {
        var current = start;

        while (current != null)
        {
            if (current is FrameworkElement fe && fe.DataContext is T match)
                return match;

            if (current == stop)
                break;

            current = VisualTreeHelper.GetParent(current)
                      ?? LogicalTreeHelper.GetParent(current); // fallback for non-visual elements
        }
        return null;
    }
}