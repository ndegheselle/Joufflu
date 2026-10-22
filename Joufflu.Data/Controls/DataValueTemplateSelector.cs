using System.Windows;
using System.Windows.Controls;
using Joufflu.Data.Model;
using NJsonSchema;

namespace Joufflu.Data.Controls;

/// <summary>
/// The editor a <see cref="DataValue"/> is filled in through, picked from its schema so each shape
/// is typed the way it is meant to be instead of as raw text.
/// <para>
/// A template left unset falls back to <see cref="FallbackTemplate"/>, so a host only has to give
/// the editors it cares about.
/// </para>
/// </summary>
public class DataValueTemplateSelector : DataTemplateSelector
{
    /// <summary>Free text, for a schema saying no more than <c>string</c>.</summary>
    public DataTemplate? StringTemplate { get; set; }

    /// <summary>A whole number.</summary>
    public DataTemplate? IntegerTemplate { get; set; }

    /// <summary>A number with a fractional part.</summary>
    public DataTemplate? NumberTemplate { get; set; }

    /// <summary>A yes/no.</summary>
    public DataTemplate? BooleanTemplate { get; set; }

    /// <summary>A closed list of values, whatever the type they are expressed in.</summary>
    public DataTemplate? EnumerationTemplate { get; set; }

    /// <summary>A point in time (<c>date</c> or <c>date-time</c>).</summary>
    public DataTemplate? DateTemplate { get; set; }

    /// <summary>A length of time (<c>time-span</c> or <c>duration</c>).</summary>
    public DataTemplate? TimeTemplate { get; set; }

    /// <summary>What a schema with no type of its own, or one no editor covers, is filled in with.</summary>
    public DataTemplate? FallbackTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not DataValue value)
            return base.SelectTemplate(item, container);

        return TemplateFor(value.Type) ?? FallbackTemplate;
    }

    /// <summary>
    /// The input corresponding to the [type].
    /// </summary>
    private DataTemplate? TemplateFor(EnumDataType type) => type switch
    {
        EnumDataType.String => StringTemplate,
        EnumDataType.Choice => EnumerationTemplate,
        EnumDataType.Boolean => BooleanTemplate,
        EnumDataType.Integer => IntegerTemplate,
        EnumDataType.Number => NumberTemplate,
        EnumDataType.DateTime => DateTemplate,
        EnumDataType.TimeSpan => TimeTemplate,
        _ => null
    };
}
