using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Toolkit;

/// <summary>
/// Gives the built-in columns of a <see cref="DataGrid"/> (text, check box and combo box) themed
/// cell elements.
/// <para>
/// These columns always assign an explicit default style to the element they generate (for
/// instance <see cref="DataGridTextColumn.DefaultEditingElementStyle"/>), so the implicit style of
/// the TextBox / CheckBox / ComboBox never reaches it and the default WPF template shows instead.
/// Columns are not in the visual tree and cannot receive an implicit style either, so these
/// properties push their style onto each matching column as it is added. A column that sets its
/// own <c>ElementStyle</c> / <c>EditingElementStyle</c> keeps it.
/// </para>
/// <para>
/// The Joufflu DataGrid style feeds them the implicit input styles
/// (<c>{DynamicResource {x:Type TextBox}}</c>…), so the cell editors look like any other input.
/// </para>
/// </summary>
public static class DataGridEditing
{
    /// <summary>Editing style of <see cref="DataGridTextColumn"/> (a TextBox).</summary>
    public static readonly DependencyProperty TextEditingStyleProperty =
        RegisterStyle("TextEditingStyle");

    public static Style? GetTextEditingStyle(DependencyObject obj) => (Style?)obj.GetValue(TextEditingStyleProperty);
    public static void SetTextEditingStyle(DependencyObject obj, Style? value) => obj.SetValue(TextEditingStyleProperty, value);

    /// <summary>Display style of <see cref="DataGridCheckBoxColumn"/> (a CheckBox that must not be hit-testable).</summary>
    public static readonly DependencyProperty CheckBoxElementStyleProperty =
        RegisterStyle("CheckBoxElementStyle");

    public static Style? GetCheckBoxElementStyle(DependencyObject obj) => (Style?)obj.GetValue(CheckBoxElementStyleProperty);
    public static void SetCheckBoxElementStyle(DependencyObject obj, Style? value) => obj.SetValue(CheckBoxElementStyleProperty, value);

    /// <summary>Editing style of <see cref="DataGridCheckBoxColumn"/> (a CheckBox).</summary>
    public static readonly DependencyProperty CheckBoxEditingStyleProperty =
        RegisterStyle("CheckBoxEditingStyle");

    public static Style? GetCheckBoxEditingStyle(DependencyObject obj) => (Style?)obj.GetValue(CheckBoxEditingStyleProperty);
    public static void SetCheckBoxEditingStyle(DependencyObject obj, Style? value) => obj.SetValue(CheckBoxEditingStyleProperty, value);

    /// <summary>
    /// Editing style of <see cref="DataGridComboBoxColumn"/> (a ComboBox). The display element is a
    /// text-only ComboBox that already looks right, so it is left alone.
    /// </summary>
    public static readonly DependencyProperty ComboBoxEditingStyleProperty =
        RegisterStyle("ComboBoxEditingStyle");

    public static Style? GetComboBoxEditingStyle(DependencyObject obj) => (Style?)obj.GetValue(ComboBoxEditingStyleProperty);
    public static void SetComboBoxEditingStyle(DependencyObject obj, Style? value) => obj.SetValue(ComboBoxEditingStyleProperty, value);

    /// <summary>Which column property each attached style feeds, and the WPF default it replaces.</summary>
    private sealed record Target(
        DependencyProperty Source,
        Type ColumnType,
        DependencyProperty ColumnProperty,
        Style DefaultStyle);

    // Declared after the attached properties: static fields initialize in textual order.
    private static readonly Target[] Targets =
    [
        new(TextEditingStyleProperty, typeof(DataGridTextColumn),
            DataGridBoundColumn.EditingElementStyleProperty, DataGridTextColumn.DefaultEditingElementStyle),
        new(CheckBoxElementStyleProperty, typeof(DataGridCheckBoxColumn),
            DataGridBoundColumn.ElementStyleProperty, DataGridCheckBoxColumn.DefaultElementStyle),
        new(CheckBoxEditingStyleProperty, typeof(DataGridCheckBoxColumn),
            DataGridBoundColumn.EditingElementStyleProperty, DataGridCheckBoxColumn.DefaultEditingElementStyle),
        new(ComboBoxEditingStyleProperty, typeof(DataGridComboBoxColumn),
            DataGridComboBoxColumn.EditingElementStyleProperty, DataGridComboBoxColumn.DefaultEditingElementStyle),
    ];

    private static DependencyProperty RegisterStyle(string name) =>
        DependencyProperty.RegisterAttached(
            name,
            typeof(Style),
            typeof(DataGridEditing),
            new PropertyMetadata(null, OnStyleChanged));

    private static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid)
            return;

        // Columns declared in XAML are already there when the grid style is applied; the ones
        // added later (including auto-generated ones) go through the collection handler, which
        // reads the current styles so it only needs hooking once per grid.
        if (!(bool)grid.GetValue(IsHookedProperty))
        {
            grid.SetValue(IsHookedProperty, true);
            grid.Columns.CollectionChanged += (_, args) =>
            {
                if (args.NewItems is null)
                    return;
                foreach (DataGridColumn column in args.NewItems)
                    foreach (Target target in Targets)
                        Apply(column, target, null, (Style?)grid.GetValue(target.Source));
            };
        }

        Target changed = Targets.First(t => t.Source == e.Property);
        foreach (DataGridColumn column in grid.Columns)
            Apply(column, changed, (Style?)e.OldValue, (Style?)e.NewValue);
    }

    private static readonly DependencyProperty IsHookedProperty =
        DependencyProperty.RegisterAttached(
            "IsHooked",
            typeof(bool),
            typeof(DataGridEditing),
            new PropertyMetadata(false));

    private static void Apply(DataGridColumn column, Target target, Style? oldStyle, Style? newStyle)
    {
        if (!target.ColumnType.IsInstanceOfType(column))
            return;

        // Only replace the WPF default (or the style we pushed previously), never a style
        // the column author chose.
        var current = (Style?)column.GetValue(target.ColumnProperty);
        bool isOurs = current == target.DefaultStyle
            || (oldStyle is not null && current == oldStyle);
        if (!isOurs)
            return;

        if (newStyle is null)
            column.ClearValue(target.ColumnProperty);
        else
            column.SetValue(target.ColumnProperty, newStyle);
    }
}
