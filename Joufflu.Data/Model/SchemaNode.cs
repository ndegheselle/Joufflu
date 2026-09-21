using CommunityToolkit.Mvvm.ComponentModel;
using NJsonSchema;

namespace Joufflu.Data.Model;

/// <summary>
/// A schema read as a tree: the shape a value would have to take, rather than a value itself.
/// <see cref="DataNode"/> is the same tree on the filling-in side.
/// </summary>
public abstract partial class SchemaNode : ObservableObject
{
    public JsonSchema Schema { get; }

    /// <summary>
    /// The name the node is read by: a property's name, <see cref="SchemaFactory.ElementKey"/> for
    /// what an array holds, nothing at the root.
    /// </summary>
    public string? Key { get; }

    public JsonObjectType Type => Schema.Type;

    /// <summary>
    /// Whether the node has to be there: a property its object requires, or what an array holds.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Whether null is taken on top of the type the schema calls for. Read off the reference that
    /// led here as much as off the schema itself: a nullable reference is written as a choice
    /// between the shape and null, and the shape alone knows nothing of that choice.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Whether the node points back at a schema already open above it. Such a node is left as a
    /// leaf: a shape that contains itself has no end to walk to.
    /// </summary>
    public bool IsRecursive { get; init; }

    /// <summary>The name the schema goes by, where it has one of its own.</summary>
    public string? Title { get; init; }

    /// <summary>What the schema says of itself, where it says anything.</summary>
    public string? Description { get; init; }

    /// <summary>What the row is read by: the key, falling back to the schema's own name at the root.</summary>
    public string? Label => Key ?? Title;

    /// <summary>The type as it is read: <c>string (date-time)</c>, <c>integer?</c>, <c>enum</c>.</summary>
    public string TypeName { get; }

    /// <summary>
    /// What the schema asks of a value on top of its type — the closed list it has to be picked
    /// from, the bounds it has to sit within — null where it asks nothing.
    /// </summary>
    public string? Constraints { get; }

    /// <summary>Whether the children are shown. Open, so the shape is seen right away.</summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    protected SchemaNode(string? key, JsonSchema schema, bool isNullable)
    {
        Key = key;
        Schema = schema;
        IsNullable = isNullable;
        TypeName = NameOf(schema, isNullable);
        Constraints = ConstraintsOf(schema);
    }

    /// <summary>
    /// The type [schema] is read as, its format where it narrows one, and a trailing <c>?</c> where
    /// null is taken on top — the way a nullable reads, rather than as a type of its own.
    /// </summary>
    private static string NameOf(JsonSchema schema, bool isNullable)
    {
        string name = BaseNameOf(schema);

        if (!string.IsNullOrEmpty(schema.Format))
            name += $" ({schema.Format})";

        if (isNullable && schema.Type != JsonObjectType.Null)
            name += "?";

        return name;
    }

    /// <summary>
    /// The bare type of [schema]. <see cref="JsonObjectType"/> is a flag enum, so a type holding
    /// several flags is read by the first one that carries a shape.
    /// </summary>
    private static string BaseNameOf(JsonSchema schema)
    {
        // A closed list wins over the type it is written in: picking from the list is the point.
        if (schema.IsEnumeration)
            return "enum";

        JsonObjectType type = schema.Type;
        if (type.HasFlag(JsonObjectType.Object))
            return "object";
        if (type.HasFlag(JsonObjectType.Array))
            return "array";
        if (type.HasFlag(JsonObjectType.String))
            return "string";
        if (type.HasFlag(JsonObjectType.Integer))
            return "integer";
        if (type.HasFlag(JsonObjectType.Number))
            return "number";
        if (type.HasFlag(JsonObjectType.Boolean))
            return "boolean";
        if (type.HasFlag(JsonObjectType.File))
            return "file";
        if (type.HasFlag(JsonObjectType.Null))
            return "null";

        // A schema saying nothing of a type accepts anything.
        return "any";
    }

    /// <summary>What [schema] asks of a value on top of its type, read as one line.</summary>
    private static string? ConstraintsOf(JsonSchema schema)
    {
        List<string> parts = [];

        if (schema.IsEnumeration)
            parts.Add($"one of {string.Join(", ", DataEnumOption.OptionsOf(schema).Select(option => option.Name))}");
        if (RangeOf(schema.MinLength, schema.MaxLength) is string length)
            parts.Add($"length {length}");
        if (RangeOf(schema.Minimum, schema.Maximum) is string bounds)
            parts.Add(bounds);
        // The item counts are plain numbers rather than nullables, so no bound reads as zero.
        if (RangeOf(schema.MinItems > 0 ? schema.MinItems : null, schema.MaxItems > 0 ? schema.MaxItems : null) is string count)
            parts.Add($"{count} items");
        if (schema.MultipleOf is decimal multiple)
            parts.Add($"multiple of {multiple}");
        if (!string.IsNullOrEmpty(schema.Pattern))
            parts.Add($"pattern {schema.Pattern}");

        return parts.Count > 0 ? string.Join(" - ", parts) : null;
    }

    /// <summary>[min]..[max], as far as either is given, null where neither is.</summary>
    private static string? RangeOf(object? min, object? max) => (min, max) switch
    {
        (null, null) => null,
        (not null, null) => $"min {min}",
        (null, not null) => $"max {max}",
        _ => $"{min}..{max}"
    };
}

/// <summary>A shape made of named properties, each a node of its own.</summary>
public class SchemaObject : SchemaNode
{
    public IReadOnlyList<SchemaNode> Properties { get; init; } = [];

    public SchemaObject(string? key, JsonSchema schema, bool isNullable) : base(key, schema, isNullable) { }
}

/// <summary>
/// A shape made of repeats of one other. What it holds is read as a single child, since every
/// element of the array is filled in against that same schema.
/// </summary>
public class SchemaArray : SchemaNode
{
    /// <summary>
    /// The shape of an element, as the one child the array carries — empty for an array saying
    /// nothing of what it holds. Held as a collection, since the tree is fed one.
    /// </summary>
    public IReadOnlyList<SchemaNode> Items { get; init; } = [];

    public SchemaArray(string? key, JsonSchema schema, bool isNullable) : base(key, schema, isNullable) { }
}

/// <summary>A shape filled in as a single value.</summary>
public class SchemaValue : SchemaNode
{
    /// <summary>
    /// The choices a closed list offers, empty where the schema is not one. Read on the row through
    /// <see cref="SchemaNode.Constraints"/>; held here for a host that wants them one by one.
    /// </summary>
    public IReadOnlyList<DataEnumOption> Options { get; }

    public SchemaValue(string? key, JsonSchema schema, bool isNullable) : base(key, schema, isNullable)
    {
        Options = DataEnumOption.OptionsOf(schema);
    }
}

public static class SchemaFactory
{
    /// <summary>What an array's element is read by, standing in for the index it would carry.</summary>
    public const string ElementKey = "[]";

    extension(JsonSchema schema)
    {
        /// <summary>
        /// The tree [schema] describes. Whether it is required is the parent's to say — a schema
        /// lists the properties it requires, so a property cannot read it off itself — hence
        /// [isRequired], passed down as the tree is built.
        /// </summary>
        public SchemaNode ToSchemaNode(string? name = null, bool isRequired = false)
            => Build(schema, name, isRequired, []);
    }

    /// <summary>
    /// The node [schema] describes, with [open] the schemas already walked into above it, so a
    /// shape that contains itself is stopped rather than walked forever.
    /// </summary>
    private static SchemaNode Build(JsonSchema schema, string? key, bool isRequired, List<JsonSchema> open)
    {
        // The type schema rather than the referenced one: a shape that takes null is written as a
        // choice between that shape and null, and it is the shape that is to be walked into.
        JsonSchema actual = schema.ActualTypeSchema;
        // Which leaves only the reference knowing of the null, so it is read off both.
        bool isNullable = schema.IsNullable(SchemaType.JsonSchema) || actual.IsNullable(SchemaType.JsonSchema);
        // A reference carries the words it is used by; the schema it points at carries the shape.
        string? title = actual.Title ?? schema.Title;
        string? description = schema.Description ?? actual.Description;

        if (open.Any(walked => ReferenceEquals(walked, actual)))
        {
            return new SchemaValue(key, actual, isNullable)
            {
                IsRequired = isRequired,
                IsRecursive = true,
                Title = title,
                Description = description
            };
        }

        open.Add(actual);
        try
        {
            if (actual.IsObject)
            {
                return new SchemaObject(key, actual, isNullable)
                {
                    IsRequired = isRequired,
                    Title = title,
                    Description = description,
                    Properties = [.. actual.ActualProperties.Select(
                        property => Build(property.Value, property.Key, property.Value.IsRequired, open))]
                };
            }

            if (actual.IsArray)
            {
                // A schema listing several templates is read by the first: what is shown is the
                // shape of an element, and one of them is closer to that than none.
                JsonSchema? item = actual.Item ?? actual.Items.FirstOrDefault();
                return new SchemaArray(key, actual, isNullable)
                {
                    IsRequired = isRequired,
                    Title = title,
                    Description = description,
                    // What an array holds is not marked required: every element is filled in
                    // against that one shape, so there is nothing for the mark to tell apart.
                    Items = item is null ? [] : [Build(item, ElementKey, isRequired: false, open)]
                };
            }

            return new SchemaValue(key, actual, isNullable)
            {
                IsRequired = isRequired,
                Title = title,
                Description = description
            };
        }
        finally
        {
            open.RemoveAt(open.Count - 1);
        }
    }
}
