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

    /// <summary>A length of time (<c>time</c>, <c>time-span</c> or <c>duration</c>).</summary>
    public DataTemplate? TimeTemplate { get; set; }

    /// <summary>What a schema with no type of its own, or one no editor covers, is filled in with.</summary>
    public DataTemplate? FallbackTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is not DataValue value)
            return base.SelectTemplate(item, container);

        return TemplateFor(value.Schema) ?? FallbackTemplate;
    }

    /// <summary>
    /// The editor [schema] is read by. <see cref="JsonObjectType"/> is a flag enum, so a type
    /// holding several flags is read by the first one that carries an editor.
    /// </summary>
    private DataTemplate? TemplateFor(JsonSchema schema)
    {
        // A closed list wins over the type it is written in: picking from the list is the point.
        if (schema.IsEnumeration)
            return EnumerationTemplate;

        JsonObjectType type = schema.Type;
        if (type.HasFlag(JsonObjectType.Boolean))
            return BooleanTemplate;
        if (type.HasFlag(JsonObjectType.Integer))
            return IntegerTemplate;
        if (type.HasFlag(JsonObjectType.Number))
            return NumberTemplate;
        if (type.HasFlag(JsonObjectType.String))
            return schema.Format switch
            {
                JsonFormatStrings.DateTime or JsonFormatStrings.Date => DateTemplate,
                JsonFormatStrings.Time or JsonFormatStrings.TimeSpan or JsonFormatStrings.Duration => TimeTemplate,
                _ => StringTemplate
            };

        return null;
    }
}
