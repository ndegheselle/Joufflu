using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Joufflu.Data.Model;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Data.Controls;

/// <summary>
/// The rows of <see cref="DataDisplay"/>: the shown tree laid flat, each node followed by its
/// children while it is expanded and not forced.
/// <para>
/// A tree view virtualizes each level apart, and once siblings differ in height (one expanded, the
/// others not) it walks all of them on every scroll: a large array of objects then takes a second
/// per scroll. A flat list of rows of a single height only ever lays out the rows in view.
/// </para>
/// <para>
/// The rows follow the tree: a node expanded, collapsed, forced or given other children has its rows
/// rebuilt. Changes are gathered and applied at once through [applyLater], so loading a large array
/// into a shown node rebuilds its rows once instead of once per item.
/// </para>
/// </summary>
internal sealed class DataDisplayRows : ObservableCollection<DataNode>
{
    private readonly Action<Action> _applyLater;

    // One handler for every node followed: a handler per node would cost an allocation per node.
    private readonly PropertyChangedEventHandler _onNodeChanged;
    private readonly NotifyCollectionChangedEventHandler _onChildrenChanged;

    /// <summary>How deep each row sits, side by side with the rows: what a row's children are told by.</summary>
    private readonly List<int> _depths = [];

    /// <summary>The nodes whose rows are to be rebuilt by the next <see cref="ApplyChanges"/>.</summary>
    private readonly HashSet<DataNode> _changed = [];
    private bool _isApplyPending;
    private DataNode? _shown;

    public DataDisplayRows(Action<Action> applyLater)
    {
        _applyLater = applyLater;
        _onNodeChanged = OnNodeChanged;
        _onChildrenChanged = OnChildrenChanged;
    }

    /// <summary>
    /// The rows themselves, changed in bulk and announced with a single reset: the list does not
    /// announce a range, and announcing rows one by one would cost a layout pass each.
    /// ObservableCollection always holds its items in a List.
    /// </summary>
    private List<DataNode> Rows => (List<DataNode>)Items;

    /// <summary>
    /// Shows [node]: its properties as the first level for an object, the node itself for anything
    /// else, nothing for null. The tree shown before is no longer followed.
    /// </summary>
    public void Show(DataNode? node)
    {
        if (_shown is not null)
            Unfollow(_shown);

        _shown = node;
        _changed.Clear();
        Rebuild();
    }

    private void Rebuild()
    {
        Rows.Clear();
        _depths.Clear();

        if (_shown is DataObject root)
        {
            Follow(root, root.Properties);
            foreach (DataNode property in root.Properties)
                AddRows(Rows, _depths, property, 0);
        }
        else if (_shown is not null)
        {
            AddRows(Rows, _depths, _shown, 0);
        }

        AnnounceReset();
    }

    /// <summary>Adds the row of [node] and the rows it shows under it.</summary>
    private void AddRows(List<DataNode> rows, List<int> depths, DataNode node, int depth)
    {
        rows.Add(node);
        depths.Add(depth);

        if (ChildrenOf(node) is not DataNodeCollection children)
            return;

        Follow(node, children);
        if (!ShowsChildren(node))
            return;

        foreach (DataNode child in children)
            AddRows(rows, depths, child, depth + 1);
    }

    /// <summary>Rebuilds the rows under the row at [index].</summary>
    private void RebuildChildren(int index)
    {
        int depth = _depths[index];
        int end = index + 1;
        while (end < Rows.Count && _depths[end] > depth)
            end++;

        Rows.RemoveRange(index + 1, end - index - 1);
        _depths.RemoveRange(index + 1, end - index - 1);

        DataNode node = Rows[index];
        if (ChildrenOf(node) is not DataNodeCollection children || !ShowsChildren(node))
            return;

        List<DataNode> rows = [];
        List<int> depths = [];
        foreach (DataNode child in children)
            AddRows(rows, depths, child, depth + 1);

        Rows.InsertRange(index + 1, rows);
        _depths.InsertRange(index + 1, depths);
    }

    #region Following the tree

    private void Follow(DataNode node, DataNodeCollection children)
    {
        // Removed first: a node shown again after a collapse is followed once only.
        node.PropertyChanged -= _onNodeChanged;
        node.PropertyChanged += _onNodeChanged;
        children.CollectionChanged -= _onChildrenChanged;
        children.CollectionChanged += _onChildrenChanged;
    }

    /// <summary>Stops following [node] and everything under it, so the tree does not keep the rows alive.</summary>
    private void Unfollow(DataNode node)
    {
        if (ChildrenOf(node) is not DataNodeCollection children)
            return;

        node.PropertyChanged -= _onNodeChanged;
        children.CollectionChanged -= _onChildrenChanged;
        foreach (DataNode child in children)
            Unfollow(child);
    }

    private void OnNodeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DataNode.IsExpanded) or nameof(DataNode.IsManual))
            MarkChanged((DataNode)sender!);
    }

    private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (((DataNodeCollection)sender!).Owner is DataNode owner)
            MarkChanged(owner);
    }

    private void MarkChanged(DataNode node)
    {
        _changed.Add(node);
        if (_isApplyPending)
            return;

        _isApplyPending = true;
        _applyLater(ApplyChanges);
    }

    /// <summary>Rebuilds the rows under every node marked as changed, the ones not shown being left out.</summary>
    private void ApplyChanges()
    {
        _isApplyPending = false;
        if (_changed.Count == 0)
            return;

        // The root object has no row: its properties are the first level.
        if (_shown is not null && _changed.Contains(_shown))
        {
            _changed.Clear();
            Rebuild();
            return;
        }

        List<DataNode> remaining = [.. _changed];
        _changed.Clear();
        // Many nodes at once (a tree loaded again, say): looking for each of them costs more than starting over.
        if (remaining.Count > MaxChangesApart)
        {
            Rebuild();
            return;
        }

        // Bottom up: rebuilding the rows under a node leaves the indexes above it as they were.
        for (int index = Rows.Count - 1; index >= 0 && remaining.Count > 0; index--)
        {
            int found = IndexOfReference(remaining, Rows[index]);
            if (found < 0)
                continue;

            remaining.RemoveAt(found);
            RebuildChildren(index);
        }

        // The ones left are not shown: under a collapsed node, nothing to rebuild.
        AnnounceReset();
    }

    private const int MaxChangesApart = 16;

    /// <summary>
    /// Where [node] is in [nodes], by reference only: a hash lookup would read the row's node, and
    /// reading millions of nodes scattered in memory is what makes a lookup per row slow.
    /// </summary>
    private static int IndexOfReference(List<DataNode> nodes, DataNode node)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (ReferenceEquals(nodes[i], node))
                return i;
        }
        return -1;
    }

    #endregion

    private void AnnounceReset()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private static bool ShowsChildren(DataNode node) => node.IsExpanded && !node.IsManual;

    private static DataNodeCollection? ChildrenOf(DataNode node) => node switch
    {
        DataArray array => array.Values,
        DataObject obj => obj.Properties,
        _ => null
    };
}
