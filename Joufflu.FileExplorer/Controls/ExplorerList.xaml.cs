using Joufflu.FileExplorer.Controls.Base;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Joufflu.FileExplorer.Controls;

/// <summary>
/// Lists the nodes of the opened folder in a <see cref="ListView"/>, opening a folder on double click.
/// </summary>
public class ExplorerList : Control
{
    private readonly IComparer comparer = ExplorerNodeComparer.Default;

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

    static ExplorerList()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ExplorerList),
            new FrameworkPropertyMetadata(typeof(ExplorerList)));
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

    // TODO : CONTEXT MENU on nodes and outside
    // TODO : SELECTION Handle selection changed on listview to buble event
    // TODO : OPEN Handle double click on nodes

    /// <summary>Keeps only the nodes whose kind is in <see cref="ExplorerNodesControl.VisibleNodes"/>.</summary>
    private bool FilterNode(object item) => item is IExplorerNode node && VisibleNodes.Includes(node);
}