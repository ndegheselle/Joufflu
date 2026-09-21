using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using NJsonSchema;
namespace Joufflu.Data.Model;

public abstract partial class DataNode : ObservableObject
{
    public JsonSchema Schema { get; set; }

    [ObservableProperty]
    private string? _key;

    public JsonObjectType Type => Schema.Type;
    public string? Description => Schema.Description;

    /// <summary>
    /// Whether the node has to be there: a property its object requires, or an element of an array.
    /// Such a node cannot be left out, so it cannot be forced to undefined.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>Whether the schema takes null on top of the type it calls for.</summary>
    public bool IsNullable => Schema.IsNullable(SchemaType.JsonSchema);

    public DataArray? ParentArray { get; set; }

    public DataNode(string? key, JsonSchema schema)
    {
        Key = key;
        Schema = schema;
    }

    public abstract JToken? ToToken();
}

public partial class DataArrayControl
{
    public DataArray Array { get; }
    public DataArrayControl(DataArray array)
    {
        Array = array;
    }

    [RelayCommand]
    public void Add() => Array.Add();
}

public partial class DataArray : DataNode
{
    public JsonSchema Template { get; set; }
    public ObservableCollection<DataNode> Values { get; set; } = [];

    [ObservableProperty]
    private bool _isExpanded = true;
    public CompositeCollection Items { get; }

    public DataArray(string? name, JsonSchema schema) : base(name, schema)
    {
        Template = schema.Item ?? throw new Exception("Schemas with multiple templates are not supported. Only [Item] is supported not [Items].");
        Items = [new CollectionContainer { Collection = Values }, new DataArrayControl(this)];
    }

    /// <summary>
    /// Create a new node from the [Template] and add it to the [Values].
    /// </summary>
    [RelayCommand]
    public void Add()
    {
        var node = Template.ToDataNode($"[{Values.Count}]", isRequired: true);
        node.ParentArray = this;
        Values.Add(node);
    }

    [RelayCommand]
    public void Remove(DataNode node)
    {
        Values.Remove(node);
        for (int i = 0; i < Values.Count; i++)
        {
            DataNode n = Values[i];
            n.Key = $"[{i}]";
        }
    }

    public override JToken? ToToken()
    {
        var json = new JArray();
        foreach (DataNode element in Values)
        {
            if (element.ToToken() is JToken value)
                json.Add(value);
        }

        return json;
    }
}

public partial class DataObject : DataNode
{
    public List<DataNode> Properties { get; set; } = [];

    /// <summary>Whether the properties are shown. Open, so the shape is seen right away.</summary>
    [ObservableProperty]
    private bool _isExpanded = true;
    public DataObject(string? name, JsonSchema schema) : base(name, schema)
    {
    }

    public override JToken? ToToken()
    {
        var json = new JObject();
        foreach (DataNode property in Properties)
        {
            if (property.Key is null)
                continue;
            if (property.ToToken() is JToken value)
                json[property.Key] = value;
        }

        return json;
    }
}

/// <summary>One choice of a closed list: the value that is filled in, under the name it is read by.</summary>
public record DataEnumOption(string Name, object? Value)
{
    /// <summary>
    /// The choices [schema] offers. <c>x-enumNames</c> is optional and pairs with the values by
    /// position, so a name is only taken where there is one to take.
    /// </summary>
    public static IReadOnlyList<DataEnumOption> OptionsOf(JsonSchema schema)
    {
        if (!schema.IsEnumeration)
            return [];

        string[] names = [.. schema.EnumerationNames];
        return [.. schema.Enumeration.Select((value, index) =>
            new DataEnumOption(index < names.Length ? names[index] : $"{value}", value))];
    }
}

public partial class DataValue : DataNode
{
    /// <summary>What has been filled in, in the CLR type the schema's editor works in.</summary>
    [ObservableProperty]
    private object? _value;

    /// <summary> Whether the value is forced from the manual list. </summary>
    [ObservableProperty]
    private bool _isManual;

    /// <summary>
    /// The entry picked while in manual mode. Picking one forces <see cref="Value"/>, which stays
    /// the single thing a host reads: leaving manual mode keeps whatever was forced.
    /// </summary>
    [ObservableProperty]
    private DataManualValue? _manualEntry;

    partial void OnManualEntryChanged(DataManualValue? value)
    {
        if (value is not null)
            Value = value.Value;
    }

    /// <summary> The choices a closed list offers for the enumarations. </summary>
    public IReadOnlyList<DataEnumOption> Options { get; }

    public DataValue(string? name, JsonSchema schema) : base(name, schema)
    {
        Options = DataEnumOption.OptionsOf(schema);
        Value = DefaultOf(schema);
    }

    public override JToken? ToToken()
    {
        if (IsManual)
        {
            // Nothing picked writes nothing, undefined included: manual mode writes what it is
            // pointed at and no more.
            if (ManualEntry is null or DataUndefined)
                return null;
            return TokenOf(ManualEntry.Value);
        }

        if (Value is null)
            return JValue.CreateNull();

        // A closed list is written in whatever type its own values are expressed in.
        if (Schema.IsEnumeration)
            return TokenOf(Value);

        // Read the same way the editors are picked, so what is written back is of the type the
        // editor the value was filled in through works in.
        JsonObjectType type = Schema.Type;
        if (type.HasFlag(JsonObjectType.Boolean))
            return new JValue(Convert.ToBoolean(Value, CultureInfo.InvariantCulture));
        if (type.HasFlag(JsonObjectType.Integer))
            return new JValue(Convert.ToInt64(Value, CultureInfo.InvariantCulture));
        if (type.HasFlag(JsonObjectType.Number))
            return new JValue(Convert.ToDecimal(Value, CultureInfo.InvariantCulture));
        if (type.HasFlag(JsonObjectType.String))
            return new JValue(StringOf(Value, Schema.Format));

        return TokenOf(Value);
    }

    /// <summary>
    /// Default value based on the [schema]
    /// </summary>
    private static object? DefaultOf(JsonSchema schema)
    {
        if (schema.IsNullable(SchemaType.JsonSchema))
            return null;

        // A closed list has no value of its own to fall back on, so it starts on its first choice
        if (schema.IsEnumeration)
            return schema.Enumeration.FirstOrDefault();

        JsonObjectType type = schema.Type;
        if (type.HasFlag(JsonObjectType.Boolean))
            return false;
        if (type.HasFlag(JsonObjectType.Integer))
            return 0L;
        if (type.HasFlag(JsonObjectType.Number))
            return 0m;
        if (type.HasFlag(JsonObjectType.String))
            return schema.Format switch
            {
                JsonFormatStrings.DateTime or JsonFormatStrings.Date => DateTime.Today,
                JsonFormatStrings.Time or JsonFormatStrings.TimeSpan or JsonFormatStrings.Duration => TimeSpan.Zero,
                _ => string.Empty
            };

        // A schema saying nothing of a type is filled in as text, which is what its editor is.
        return string.Empty;
    }

    /// <summary> [value] as the JSON its own type amounts to, whatever the schema says it should have been. </summary>
    private static JToken TokenOf(object? value) => value switch
    {
        null => JValue.CreateNull(),
        JToken token => token,
        _ => JToken.FromObject(value)
    };

    /// <summary> [value] as the text [format] is read as. </summary>
    private static string StringOf(object value, string? format) => (value, format) switch
    {
        (DateTime date, JsonFormatStrings.Date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        (DateTime date, _) => date.ToString("O", CultureInfo.InvariantCulture),
        (DateTimeOffset date, _) => date.ToString("O", CultureInfo.InvariantCulture),
        (TimeSpan time, JsonFormatStrings.Time) => time.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture),
        (TimeSpan time, _) => time.ToString("c", CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
    };
}



public static class DataFactory
{
    extension(JsonSchema schema)
    {
        /// <summary>
        /// The node [schema] describes. Whether it is required is the parent's to say — a schema
        /// lists the properties it requires, so a property cannot read it off itself — hence
        /// [isRequired], passed down as the tree is built.
        /// </summary>
        public DataNode ToDataNode(string? name = null, bool isRequired = false)
        {
            schema = schema.ActualSchema;

            if (schema.IsObject)
            {
                var node = new DataObject(name, schema)
                {
                    IsRequired = isRequired,
                    Properties = schema.ActualProperties
                        .Select(prop => prop.Value.ToDataNode(prop.Key, prop.Value.IsRequired))
                        .ToList()
                };

                return node;
            }
            else if (schema.IsArray)
            {
                return new DataArray(name, schema) { IsRequired = isRequired };
            }

            return new DataValue(name, schema) { IsRequired = isRequired };
        }
    }
}