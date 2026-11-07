using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Joufflu.Data.Model;
using Joufflu.Inputs.Controls;

namespace Joufflu.Data.Converters;

/// <summary>The options of the file picker of a <see cref="DataValue"/>, from its <see cref="DataValue.FileFilter"/>.</summary>
public class DataFileOptionsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => new FilePickerOptions { Filter = value as string ?? "" };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataFileOptionsConverter)} only builds the options.");
}

/// <summary>
/// Whether a field of a <see cref="Controls.DataForm"/> offers to be forced : only to the host's
/// entries that fit it, null and undefined alone being no reason to clutter a form, or when it
/// is forced already, to bring its editor back.
/// <para>Bound to the <see cref="DataNode"/>, to the host's catalog and to <see cref="DataNode.IsManual"/>, in that order.</para>
/// </summary>
public class DataManualToggleVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.ElementAtOrDefault(2) is true)
            return Visibility.Visible;
        if (values.ElementAtOrDefault(0) is not DataNode node || values.ElementAtOrDefault(1) is not IEnumerable<DataManualValue> catalog)
            return Visibility.Collapsed;

        bool canBeForced = catalog.Any(entry => entry.Fits(node.Type));
        return canBeForced ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataManualToggleVisibilityConverter)} only tells whether the toggle shows.");
}

/// <summary>The values of a <see cref="DataArray"/> of plain values, as the list a <see cref="ComboBoxTags"/> edits.</summary>
public class DataArrayValuesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DataArray array ? new DataArrayValues(array) : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataArrayValuesConverter)} only reads the array.");
}

/// <summary>
/// The values the items of [array] hold, as a list : adding a value adds an item, a copy of the array's template
/// holding it, removing one removes the first item holding it.
/// </summary>
public class DataArrayValues(DataArray array) : IList
{
    public object? this[int index]
    {
        get => ValueOf(array.Values[index]);
        set => ((DataValue)array.Values[index]).Value = value;
    }

    public bool IsReadOnly => false;
    public bool IsFixedSize => false;
    public int Count => array.Values.Count;
    public object SyncRoot => this;
    public bool IsSynchronized => false;

    public int Add(object? value)
    {
        Insert(array.Values.Count, value);
        return array.Values.Count - 1;
    }

    public void Insert(int index, object? value)
    {
        var item = array.Template?.Clone() as DataValue ?? new DataValue(EnumDataType.String, null, []);
        item.Value = value;
        array.Add(item);
        if (index >= array.Values.Count - 1)
            return;

        array.Values.Move(array.Values.Count - 1, index);
        for (int i = 0; i < array.Values.Count; i++)
            array.Values[i].Key = $"[{i}]";
    }

    public void Remove(object? value)
    {
        int index = IndexOf(value);
        if (index >= 0)
            RemoveAt(index);
    }

    public void RemoveAt(int index) => array.Remove(array.Values[index]);

    public void Clear() => array.Values.Clear();

    public int IndexOf(object? value)
    {
        for (int i = 0; i < array.Values.Count; i++)
            if (Equals(ValueOf(array.Values[i]), value))
                return i;
        return -1;
    }

    public bool Contains(object? value) => IndexOf(value) >= 0;

    public void CopyTo(Array target, int index)
    {
        foreach (object? value in this)
            target.SetValue(value, index++);
    }

    public IEnumerator GetEnumerator() => array.Values.Select(ValueOf).GetEnumerator();

    private static object? ValueOf(DataNode item) => (item as DataValue)?.Value;
}
