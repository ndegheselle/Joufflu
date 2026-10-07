using System.Globalization;
using System.Windows.Data;
using Joufflu.Data.Model;

namespace Joufflu.Data.Converters;

/// <summary>
/// The manual values a field can be set to: null where the schema takes it and undefined where the
/// schema leaves the field out, followed by the host's manual values that fit the field's own type.
/// <para>
/// Bound to the <see cref="DataNode"/> and to the host's catalog, in that order.
/// </para>
/// </summary>
public class DataManualValuesConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => EntriesOf(values);

    /// <summary>
    /// The manual values the bound field can be set to, read off the same [values] both converters
    /// take: the <see cref="DataNode"/> and the host's catalog, in that order.
    /// </summary>
    internal static List<DataManualValue> EntriesOf(object[] values)
    {
        List<DataManualValue> manualValues = [];

        if (values.ElementAtOrDefault(0) is not DataNode node)
            return manualValues;

        if (!node.IsRequired)
            manualValues.Add(DataManualValue.Undefined);
        if (node.IsNullable)
            manualValues.Add(DataManualValue.Null);

        if (values.ElementAtOrDefault(1) is IEnumerable<DataManualValue> catalog)
            manualValues.AddRange(catalog.Where(manualValue => manualValue.Fits(node.Type)));

        return manualValues;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataManualValuesConverter)} only builds the list of manual values.");
}

/// <summary>
/// Whether a field has any manual value to be set to, so that manual mode is only offered where it
/// leads somewhere: a required field of a type no manual value fits has nothing to pick from.
/// <para>Bound like <see cref="DataManualValuesConverter"/>.</para>
/// </summary>
public class DataHasManualValuesConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => DataManualValuesConverter.EntriesOf(values).Count > 0;

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataHasManualValuesConverter)} only tells whether there are manual values.");
}
