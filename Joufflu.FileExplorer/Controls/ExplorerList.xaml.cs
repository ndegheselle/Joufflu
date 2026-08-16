using Joufflu.FileExplorer.Controls.Base;
using Joufflu.FileExplorer.Data;
using Joufflu.Helpers;
using System.Collections;
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

    private readonly IComparer comparer = ExplorerNodeComparer.Default;

    static ExplorerList()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ExplorerList),
            new FrameworkPropertyMetadata(typeof(ExplorerList)));
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
