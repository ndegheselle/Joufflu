using System.Collections;
using System.Globalization;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Data.Model;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Data.Controls;

/// <summary>
/// The children an object or an array shows in the tree: none while it is forced, the manual
/// entry standing for the whole of it.
/// <para>Bound to the node and to <see cref="DataNode.IsManual"/>, in that order.</para>
/// </summary>
public class DataChildrenConverter : IMultiValueConverter
{
    /// <summary>Whether the children are followed by the row that adds one, for the views that edit.</summary>
    public bool WithAddRow { get; set; }

    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.ElementAtOrDefault(1) is true)
            return null;

        object? node = values.ElementAtOrDefault(0);
        IEnumerable? children = node switch
        {
            DataArray array => array.Values,
            DataObject obj => obj.Properties,
            _ => null
        };
        if (children is null || !WithAddRow)
            return children;

        object addRow = node is DataArray parentArray
            ? new DataArrayAddRow(parentArray)
            : new DataObjectAddRow((DataObject)node!);
        return new CompositeCollection { new CollectionContainer { Collection = children }, addRow };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataChildrenConverter)} only hides the children.");
}

/// <summary>The last row of an array in the tree, adding an item from the template or of a picked type.</summary>
public partial class DataArrayAddRow(DataArray array)
{
    public DataArray Array { get; } = array;

    [RelayCommand]
    private void AddFromTemplate() => Array.AddFromTemplate();

    [RelayCommand]
    private void AddOfType(EnumDataType type)
    {
        DataNode item = DataNode.Create(type, "");
        Array.Add(item);
    }
}

/// <summary>The last row of an object in the tree, adding a property of a picked type under a free key.</summary>
public partial class DataObjectAddRow(DataObject obj)
{
    public DataObject Object { get; } = obj;

    [RelayCommand]
    private void Add(EnumDataType type)
    {
        string key = Object.UniqueKey("key");
        DataNode property = DataNode.Create(type, key);
        Object.Add(property);
    }
}
