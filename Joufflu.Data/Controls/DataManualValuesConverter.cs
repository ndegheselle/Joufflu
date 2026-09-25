using System.Globalization;
using System.Windows.Data;
using Joufflu.Data.Model;

namespace Joufflu.Data.Controls;

/// <summary>
/// The entries a field can be forced to: null where the schema takes it and undefined where the
/// schema leaves the field out, followed by the host's entries that fit the field's own type.
/// <para>
/// Bound to the <see cref="DataValue"/> and to the host's catalog, in that order.
/// </para>
/// </summary>
public class DataManualValuesConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => EntriesOf(values);

    /// <summary>
    /// The entries the bound field can be forced to, read off the same [values] both converters
    /// take: the <see cref="DataValue"/> and the host's catalog, in that order.
    /// </summary>
    internal static List<DataManualValue> EntriesOf(object[] values)
    {
        List<DataManualValue> entries = [];

        if (values.ElementAtOrDefault(0) is not DataValue node)
            return entries;

        entries.Add(DataManualValue.Undefined);
        if (node.IsNullable)
            entries.Add(DataManualValue.Null);

        if (values.ElementAtOrDefault(1) is IEnumerable<DataManualValue> catalog)
            entries.AddRange(catalog.Where(entry => entry.Fits(node.Schema.Type)));

        return entries;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataManualValuesConverter)} only builds the list of entries.");
}

/// <summary>
/// Whether a field has anything to be forced to, so that manual mode is only offered where it
/// leads somewhere: a required field of a type no entry fits has nothing to pick from.
/// <para>Bound like <see cref="DataManualValuesConverter"/>.</para>
/// </summary>
public class DataHasManualValuesConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => DataManualValuesConverter.EntriesOf(values).Count > 0;

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataHasManualValuesConverter)} only tells whether there are entries.");
}
