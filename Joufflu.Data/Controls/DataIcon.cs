using System.Windows;
using Joufflu.Assets.Fonts;
using Joufflu.Data.Model;
using Joufflu.Toolkit;

namespace Joufflu.Data.Controls;

/// <summary>
/// The glyph a <see cref="EnumDataType"/> is read by, so a shape is taken in without reading it.
/// The name of the type is left to the tooltip.
/// </summary>
public class DataIcon : FontIcon
{
    static DataIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataIcon), new FrameworkPropertyMetadata(typeof(FontIcon)));
    }

    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
        nameof(Type), typeof(EnumDataType?), typeof(DataIcon),
        new PropertyMetadata(EnumDataType.String, (d, _) => ((DataIcon)d).Refresh()));

    /// <summary>What the icon stands for. Null for a value of no type, like the manual null entry.</summary>
    public EnumDataType? Type
    {
        get => (EnumDataType?)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public DataIcon() { Refresh(); }

    private void Refresh()
    {
        Text = GlyphOf(Type);
        Tooltip.SetContent(this, Type?.ToString() ?? "null");
    }

    /// <summary>The glyph [type] is read by.</summary>
    public static string GlyphOf(EnumDataType? type) => type switch
    {
        null => LucideFontIcons.CircleSlash,
        EnumDataType.Object => LucideFontIcons.Braces,
        EnumDataType.Array => LucideFontIcons.Brackets,
        EnumDataType.String => LucideFontIcons.Type,
        EnumDataType.Integer or EnumDataType.Number => LucideFontIcons.Hash,
        EnumDataType.Boolean => LucideFontIcons.ToggleLeft,
        EnumDataType.DateTime => LucideFontIcons.Calendar,
        EnumDataType.TimeSpan => LucideFontIcons.Timer,
        EnumDataType.Choice => LucideFontIcons.List,
        _ => LucideFontIcons.CircleHelp
    };
}
