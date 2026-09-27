using System.Globalization;
using System.Windows.Data;
using Joufflu.Data.Model;

namespace Joufflu.Data.Controls;

/// <summary>
/// The text a <see cref="DataValue"/> is read by in <see cref="DataDisplay"/>: the entry it is forced
/// to, the name of the option it picks, or its value written the way its editor shows it.
/// <para>
/// Bound to the <see cref="DataValue"/> first; the bindings after it (its value, whether it is
/// forced and to what) are only there to refresh the text when they change.
/// </para>
/// </summary>
public class DataValueTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.ElementAtOrDefault(0) is DataValue node ? TextOf(node, culture) : "";

    /// <summary>What [node] holds, as text.</summary>
    public static string TextOf(DataValue node, CultureInfo culture)
    {
        // Nothing picked writes nothing, as undefined does.
        if (node.IsManual)
            return (node.ManualEntry ?? DataManualValue.Undefined).ToString();

        return node.Value switch
        {
            null => "null",
            bool value => value ? "true" : "false",
            // A date alone is what the DatePicker fills in, the time only shows when there is one.
            DateTime value => value.ToString(value.TimeOfDay == TimeSpan.Zero ? "d" : "g", culture),
            TimeSpan value => value.ToString("c", culture),
            _ when node.Type == EnumDataType.Choice => node.Options
                .FirstOrDefault(option => Equals(option.Value, node.Value))?.Name
                ?? System.Convert.ToString(node.Value, culture) ?? "",
            _ => System.Convert.ToString(node.Value, culture) ?? ""
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(DataValueTextConverter)} only writes the text of a value.");
}
