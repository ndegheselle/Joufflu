using System.Globalization;
using System.Windows.Data;
using Joufflu.Data.Controls;
using Joufflu.Data.Model;

namespace Joufflu.Data.Converters;

/// <summary>
/// The text a <see cref="DataNode"/> is read by in <see cref="DataDisplay"/>: the manual value it is set
/// to, the name of the option it picks, or its value written the way its editor shows it. An object
/// or an array has no text but the manual value it is set to.
/// <para>
/// Bound to the <see cref="DataNode"/> first; the bindings after it (its value, whether it is
/// in manual mode and to what) are only there to refresh the text when they change.
/// </para>
/// </summary>
public class DataValueTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.ElementAtOrDefault(0) is DataNode node ? TextOf(node, culture) : "";

    /// <summary>What [node] holds, as text.</summary>
    public static string TextOf(DataNode node, CultureInfo culture)
    {
        // Nothing picked writes nothing, as undefined does.
        if (node.IsManual)
            return (node.ManualValue ?? DataManualValue.Undefined).ToString();

        return node is DataValue value ? ValueTextOf(value, culture) : "";
    }

    private static string ValueTextOf(DataValue node, CultureInfo culture)
    {
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
