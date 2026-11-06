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
