using System.Globalization;
using System.Windows.Data;
using Joufflu.Data.Model;

namespace Joufflu.Data.Controls;

/// <summary>
/// The entries a field can be forced to: null and undefined, which every field takes, followed by
/// the host's entries that fit the field's own type.
/// <para>
/// Bound to the <see cref="DataValue"/> and to the host's catalog, in that order.
/// </para>
/// </summary>
public class DataManualValuesConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // Offered whatever the field holds: forcing a field to nothing is not a matter of type.
        List<DataManualValue> entries = [DataManualValue.Null, DataManualValue.Undefined];

        if (values.ElementAtOrDefault(0) is not DataValue node)
            return entries;

        if (values.ElementAtOrDefault(1) is IEnumerable<DataManualValue> catalog)
            entries.AddRange(catalog.Where(entry => entry.Fits(node.Schema.Type)));

        return entries;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataManualValuesConverter)} only builds the list of entries.");
}
