using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Joufflu.Data.Controls;
using Joufflu.Data.Model;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Data.Converters;

/// <summary>
/// How deep a row of <see cref="DataDisplay"/> sits: 0 for the first level, the properties of an
/// object shown or the node itself otherwise.
/// <para>Bound to the row's node, then to the node shown.</para>
/// <para>
/// Read back from the node's parents although <see cref="DataDisplayRows"/> knows each depth: the
/// rows are the nodes themselves, as an object per row would cost one more object per node of a
/// large file, millions of them.
/// </para>
/// </summary>
public class DataDepthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.ElementAtOrDefault(0) is not DataNode node)
            return 0;

        object? shown = values.ElementAtOrDefault(1);
        if (ReferenceEquals(node, shown))
            return 0;

        // An object shown has no row: its properties are the first level.
        int depth = shown is DataObject ? 0 : 1;
        for (var parent = node.Parent as DataNode; parent is not null && !ReferenceEquals(parent, shown); parent = parent.Parent as DataNode)
            depth++;
        return depth;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataDepthConverter)} only reads the depth of a row.");
}

/// <summary>
/// Shows the chevron of a row whose node has children to show: an object or an array that is not
/// forced and holds any. Hidden rather than collapsed, so every row starts its header at the same place.
/// <para>Bound to <see cref="DataNode.IsManual"/>, then to the count of the children.</para>
/// </summary>
public class DataChevronVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        bool isManual = values.ElementAtOrDefault(0) is true;
        bool hasChildren = values.ElementAtOrDefault(1) is int count && count > 0;
        return !isManual && hasChildren ? Visibility.Visible : Visibility.Hidden;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataChevronVisibilityConverter)} only shows the chevron.");
}
