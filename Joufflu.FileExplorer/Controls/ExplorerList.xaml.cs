using Joufflu.FileExplorer.Controls.Base;
using Joufflu.FileExplorer.Data;
using Joufflu.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Joufflu.FileExplorer.Controls;

/// <summary>
/// Lists the nodes of the opened folder in a <see cref="ListView"/>, opening a folder on double click.
/// </summary>
public class ExplorerList : ExplorerNodesControl
{
    #region Dependency Properties

    public static readonly DependencyPropertyKey ViewPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(View),
        typeof(ICollectionView),
        typeof(ExplorerList),
        new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty ViewProperty = ViewPropertyKey.DependencyProperty;

    #endregion

    /// <summary>
    /// Nodes of the opened directory, sorted and filtered, as the list displays them.
    /// </summary>
    public ICollectionView? View
    {
        get => (ICollectionView?)GetValue(ViewProperty);
        private set => SetValue(ViewPropertyKey, value);
    }

    /// <summary>
    /// Columns displayed after the ones of the list (name, modification date, size), for the data a node type of your
    /// own carries : the cells are bound to the node of their row, so a
    /// <c>DisplayMemberBinding="{Binding Author}"</c> shows the Author of a custom <see cref="IExplorerNode"/>. A
    /// column can be added or removed at any time, the list follows.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fileExplorer:ExplorerList Source="{Binding Source}"&gt;
    ///     &lt;fileExplorer:ExplorerList.ExtraColumns&gt;
    ///         &lt;GridViewColumn Header="Author" DisplayMemberBinding="{Binding Author}" /&gt;
    ///     &lt;/fileExplorer:ExplorerList.ExtraColumns&gt;
    /// &lt;/fileExplorer:ExplorerList&gt;
    /// </code>
    /// </example>
    public ObservableCollection<GridViewColumn> ExtraColumns { get; } = [];

    /// <summary>Columns of the template, kept ahead of the <see cref="ExtraColumns"/>.</summary>
    private GridView? gridView;
    private int templateColumnCount;

    private readonly IComparer comparer = ExplorerNodeComparer.Default;

    static ExplorerList()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ExplorerList),
            new FrameworkPropertyMetadata(typeof(ExplorerList)));
    }

    public ExplorerList()
    {
        ExtraColumns.CollectionChanged += (_, _) => ApplyExtraColumns();
    }

    /// <summary>
    /// A list only displays the nodes of the opened directory, each one of them in a row of its own.
    /// </summary>
    protected override void OnCurrentChanged()
    {
        View = Source?.Current == null
            ? null
            : new ListCollectionView(Source.Current.Children)
            {
                CustomSort = comparer,
                Filter = FilterNode,
                IsLiveSorting = false,
                IsLiveFiltering = false
            };
    }

    protected override void OnVisibleNodesChanged() => View?.Refresh();

    public override void OnApplyTemplate()
    {
        if (ItemsHost is ListView oldList)
            oldList.SelectionChanged -= OnListSelectionChanged;

        base.OnApplyTemplate();

        if (ItemsHost is ListView newList)
            newList.SelectionChanged += OnListSelectionChanged;

        // A template of its own for each control, so its columns can be added to without touching the other lists.
        gridView = (ItemsHost as ListView)?.View as GridView;
        templateColumnCount = gridView?.Columns.Count ?? 0;
        ApplyExtraColumns();
    }

    /// <summary>
    /// Puts the <see cref="ExtraColumns"/> back at the end of the columns of the template.
    /// </summary>
    private void ApplyExtraColumns()
    {
        if (gridView == null)
            return;

        while (gridView.Columns.Count > templateColumnCount)
            gridView.Columns.RemoveAt(gridView.Columns.Count - 1);

        foreach (var column in ExtraColumns)
            gridView.Columns.Add(column);
    }

    private void OnListSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateSelectedNodes();

    protected override IReadOnlyList<IExplorerNode> GetSelectedNodes()
        => (ItemsHost as ListView)?.SelectedItems.Cast<IExplorerNode>().ToList() ?? [];

    protected override FrameworkElement? GetContainerAt(DependencyObject? source)
        // Self or parent : a click on the blank part of a row reports the row itself.
        => MoreVisualTreeHelper.FindSelfOrParent(source, typeof(ListViewItem)) as ListViewItem;

    /// <summary>Ignore listview columns header</summary>
    protected override bool IsMenuIgnored(DependencyObject? source)
        => MoreVisualTreeHelper.FindParent<GridViewColumnHeader>(source) != null;
}
