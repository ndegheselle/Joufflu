using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
namespace Joufflu.Data.Model;

public enum EnumDataType
{
    String,
    Integer,
    Number,
    Boolean,
    TimeSpan,
    DateTime,
    Choice,
    Array,
    Object
}

public abstract partial class DataNode : ObservableObject, ICloneable
{
    [ObservableProperty]
    private string? _key;

    /// <summary>For DataNode with children and to prevent binding errors.</summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    public EnumDataType Type { get; private set; }
    /// <summary>Whether the schema takes null on top of the type it calls for.</summary>
    public bool IsNullable { get; set; }
    public bool CanEditKey { get; set; } = true;

    public IDataParent? Parent { get; set; }
    public string? Description { get; set; }

    public DataNode(EnumDataType type, string? key)
    {
        Type = type;
        Key = key;
    }

    public abstract JToken? ToToken();

    /// <summary>
    /// A deep copy of the node, values included. The copy stands on its own: it belongs to no
    /// <see cref="Parent"/> until one takes it.
    /// </summary>
    public abstract DataNode Clone();
    object ICloneable.Clone() => Clone();
}

public interface IDataParent
{
    [RelayCommand]
    public void Remove(DataNode node);
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
    [RelayCommand]
    public void AddFromType(EnumDataType type) => Array.Add(type);
}

public partial class DataObjectControl
{
    public DataObject Object { get; }
    public DataObjectControl(DataObject obj)
    {
        Object = obj;
    }

    [RelayCommand]
    public void Add(EnumDataType type) => Object.Add(NodeFrom(type));
    public static DataNode NodeFrom(EnumDataType type, string key = "new") => type switch
    {
        EnumDataType.Object => new DataObject(key),
        EnumDataType.Array => new DataArray(key, null),
        _ => new DataValue(type, key, [])
    };
}

public partial class DataArray : DataNode, IDataParent
{
    [ObservableProperty]
    private DataNode? _template;

    public ObservableCollection<DataNode> Values { get; set; } = [];

    /// <summary>
    /// Values with the add control item.
    /// </summary>
    public CompositeCollection Items { get; }

    public DataArray(string? key, DataNode? template) : base(EnumDataType.Array, key)
    {
        Template = template;
        Items = [new CollectionContainer { Collection = Values }, new DataArrayControl(this)];
    }

    /// <summary>
    /// Create a new node from the [Template] and add it to the [Values].
    /// </summary>
    public void Add()
    {
        if (Template == null)
            return;

        var node = Template.Clone();
        node.Key = $"[{Values.Count}]";
        node.CanEditKey = false;
        node.Parent = this;
        Values.Add(node);
    }

    public void Add(EnumDataType type)
    {
        var node = DataObjectControl.NodeFrom(type);
        node.Key = $"[{Values.Count}]";
        node.CanEditKey = false;
        node.Parent = this;
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

    public override DataArray Clone()
    {
        var clone = new DataArray(Key, Template?.Clone())
        {
            Description = Description,
            IsExpanded = IsExpanded,
            IsNullable = IsNullable,
        };

        // Filled in place: [Items] watches this very collection.
        foreach (DataNode value in Values)
        {
            var copy = value.Clone();
            copy.Parent = clone;
            clone.Values.Add(copy);
        }

        return clone;
    }
}

public partial class DataObject : DataNode, IDataParent
{
    public ObservableCollection<DataNode> Properties { get; set; } = [];
    public CompositeCollection Items { get; }

    public DataObject(string? key) : base(EnumDataType.Object, key)
    {
        Items = [new CollectionContainer { Collection = Properties }, new DataObjectControl(this)];
    }

    [RelayCommand]
    public void Add(DataNode node)
    {
        node.Parent = this;
        Properties.Add(node);
    }

    [RelayCommand]
    public void Remove(DataNode node)
    {
        Properties.Remove(node);
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

    public override DataObject Clone() => new(Key)
    {
        Description = Description,
        IsExpanded = IsExpanded,
        Properties = [.. Properties.Select(property => property.Clone())],
        IsNullable = IsNullable,
    };
}

/// <summary>One choice of a closed list: the value that is filled in, under the name it is read by.</summary>
public record DataEnumOption(string Name, object? Value)
{}

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

    public DataValue(EnumDataType type, string? key, IReadOnlyList<DataEnumOption> options) : base(type, key)
    {
        Options = options;
        Value = Default();
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

        return Type switch
        {
            EnumDataType.String => Convert.ToString(Value, CultureInfo.InvariantCulture) ?? string.Empty,
            EnumDataType.Boolean => new JValue(Convert.ToBoolean(Value, CultureInfo.InvariantCulture)),
            EnumDataType.Integer => new JValue(Convert.ToInt64(Value, CultureInfo.InvariantCulture)),
            EnumDataType.Number => new JValue(Convert.ToDecimal(Value, CultureInfo.InvariantCulture)),
            EnumDataType.DateTime => new JValue(((DateTime)Value).ToString("O", CultureInfo.InvariantCulture)),
            EnumDataType.TimeSpan => new JValue(((TimeSpan)Value).ToString("O", CultureInfo.InvariantCulture)),
            _ => TokenOf(Value)
        };
    }

    /// <summary>
    /// The options and the manual entry are records, shared as they are. [ManualEntry] goes in
    /// before [Value], which it would otherwise overwrite.
    /// </summary>
    public override DataValue Clone() => new(Type, Key, Options)
    {
        Description = Description,
        IsManual = IsManual,
        ManualEntry = ManualEntry,
        Value = Value is JToken token ? token.DeepClone() : Value,
        IsNullable = IsNullable,
    };

    /// <summary>
    /// Default value based on the [schema]
    /// </summary>
    private object? Default()
    {
        if (IsNullable)
            return null;

        return Type switch
        {
            EnumDataType.String => "",
            EnumDataType.Integer => 0L,
            EnumDataType.Number => 0m,
            EnumDataType.Boolean => false,
            EnumDataType.Choice => Options.FirstOrDefault(),
            EnumDataType.TimeSpan => TimeSpan.Zero,
            EnumDataType.DateTime => DateTime.Today,
            _ => throw new Exception($"Can't set a default value for the type [{Type}]")
        };
    }

    /// <summary> [value] as the JSON its own type amounts to, whatever the schema says it should have been. </summary>
    private static JToken TokenOf(object? value) => value switch
    {
        null => JValue.CreateNull(),
        JToken token => token,
        _ => JToken.FromObject(value)
    };
}