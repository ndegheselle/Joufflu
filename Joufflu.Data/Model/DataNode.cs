using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using NJsonSchema;
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

public abstract partial class DataNode : ObservableObject, INotifyDataErrorInfo
{
    [ObservableProperty]
    private string? _key;

    /// <summary>Why [Key] is refused, null when it is fine.</summary>
    private string? _keyError;

    partial void OnKeyChanged(string? value)
    {
        if (Parent is not null)
            ValidateKeys(Parent.Children);
    }

    /// <summary>For DataNode with children and to prevent binding errors.</summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    public EnumDataType Type { get; private set; }
    /// <summary>Whether the schema takes null on top of the type it calls for.</summary>
    public bool IsNullable { get; set; }
    /// <summary>Whether the parent object requires the node: it then cannot be undefined.</summary>
    public bool IsRequired { get; set; }
    public bool CanEditKey { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsArrayItem))]
    private IDataParent? _parent;

    /// <summary>Whether the node is an array item, the only kind <see cref="Controls.DataFill"/> removes.</summary>
    public bool IsArrayItem => Parent is DataArray;
    public string? Description { get; set; }

    /// <summary> Whether the node is forced from the manual list, an object or an array then leaving its children out. </summary>
    [ObservableProperty]
    private bool _isManual;

    /// <summary>The entry picked while in manual mode.</summary>
    [ObservableProperty]
    private DataManualValue? _manualEntry;

    partial void OnIsManualChanged(bool value) => OnManualModeChanged(value);
    partial void OnManualEntryChanged(DataManualValue? value) => OnManualEntryPicked(value);

    /// <summary>Called when [IsManual] changes.</summary>
    protected virtual void OnManualModeChanged(bool isManual) { }

    /// <summary>Called when [ManualEntry] changes.</summary>
    protected virtual void OnManualEntryPicked(DataManualValue? entry) { }

    public DataNode(EnumDataType type, string? key)
    {
        Type = type;
        Key = key;
    }

    public abstract JToken? ToToken();

    /// <summary>
    /// What manual mode writes: nothing when nothing is picked, undefined included, as it writes
    /// what it is pointed at and no more.
    /// </summary>
    protected JToken? ManualToken() => ManualEntry is null or DataUndefined
        ? null
        : DataValue.TokenOf(ManualEntry.Value);

    /// <summary>Raised when the node or anything under it changes, whether it is expanded aside.</summary>
    public event EventHandler? Changed;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName != nameof(IsExpanded))
            OnChanged();
    }

    /// <summary>Raises <see cref="Changed"/> on the node and on every parent up to the root.</summary>
    protected void OnChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
        (Parent as DataNode)?.OnChanged();
    }

    #region Errors

    public bool HasErrors => _keyError is not null;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// The key error belongs to [Key] alone: reporting it as an error of the whole node (a null or
    /// empty name) would flag every <c>{Binding}</c> on the node, the tree's row presenters included.
    /// </summary>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (_keyError is not null && propertyName == nameof(Key))
            return new[] { _keyError };
        return Array.Empty<string>();
    }

    /// <summary>
    /// Flags every node of [siblings] whose key another one already uses. JSON keys are case
    /// sensitive, so is the comparison; a node without a key writes nothing and clashes with none.
    /// </summary>
    internal static void ValidateKeys(IEnumerable<DataNode> siblings)
    {
        List<DataNode> nodes = [.. siblings];
        HashSet<string> duplicates = [.. nodes
            .Where(node => node.Key is not null)
            .GroupBy(node => node.Key!, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)];

        foreach (DataNode node in nodes)
            node.SetKeyError(node.Key is not null && duplicates.Contains(node.Key)
                ? $"The key [{node.Key}] is already used."
                : null);
    }

    /// <summary>A node out of any parent has no sibling to clash with.</summary>
    internal void ClearKeyError() => SetKeyError(null);

    private void SetKeyError(string? error)
    {
        if (_keyError == error)
            return;

        _keyError = error;
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Key)));
        OnPropertyChanged(nameof(HasErrors));
    }

    #endregion

    /// <summary>
    /// A deep copy of the node, values included. The copy stands on its own: it belongs to no
    /// <see cref="Parent"/> until one takes it.
    /// </summary>
    public abstract DataNode Clone();

    /// <summary>An empty node of [type]: an object without properties, an array without template, a value at its default.</summary>
    public static DataNode Create(EnumDataType type, string key) => type switch
    {
        EnumDataType.Object => new DataObject(key),
        EnumDataType.Array => new DataArray(key, null),
        _ => new DataValue(type, key, [])
    };
}

public interface IDataParent
{
    /// <summary>The nodes whose keys share this parent's scope.</summary>
    IEnumerable<DataNode> Children { get; }

    [RelayCommand]
    public void Remove(DataNode node);
}

public partial class DataOptionsControl : ObservableObject
{
    public DataValue Value { get; }

    /// <summary>The option being typed, added by <see cref="Add"/>.</summary>
    [ObservableProperty]
    private string _newOption = "";

    public DataOptionsControl(DataValue value)
    {
        Value = value;
    }

    public void Add()
    {
        Value.AddOption(NewOption);
        NewOption = "";
    }

    /// <summary>Not empty and not already an option.</summary>
    private bool CanAdd() => !string.IsNullOrEmpty(NewOption)
        && !Value.Options.Any(option => Equals(option.Value, NewOption));

    [RelayCommand]
    public void Remove(DataEnumOption option) => Value.RemoveOption(option);
}

public partial class DataArray : DataNode, IDataParent
{
    [ObservableProperty]
    private DataNode? _template;

    public ObservableCollection<DataNode> Values { get; set; } = [];

    public IEnumerable<DataNode> Children => Values;

    public DataArray(string? key, DataNode? template) : base(EnumDataType.Array, key)
    {
        Template = template;
        Values.CollectionChanged += (_, _) => OnChanged();
    }

    /// <summary>Adds a copy of [Template], nothing when there is no template.</summary>
    public void AddFromTemplate()
    {
        if (Template is null)
            return;

        Add(Template.Clone());
    }

    /// <summary>Adds [node] as the last item, keyed by its index.</summary>
    public void Add(DataNode node)
    {
        node.Key = $"[{Values.Count}]";
        node.CanEditKey = false;
        node.Parent = this;
        Values.Add(node);
    }

    [RelayCommand]
    public void Remove(DataNode node)
    {
        if (Values.Remove(node))
            node.ClearKeyError();
        for (int i = 0; i < Values.Count; i++)
        {
            DataNode n = Values[i];
            n.Key = $"[{i}]";
        }
    }

    public override JToken? ToToken()
    {
        if (IsManual)
            return ManualToken();

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
            IsRequired = IsRequired,
            IsManual = IsManual,
            ManualEntry = ManualEntry,
        };

        // Filled in place: the constructor watches this very collection.
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
    private ObservableCollection<DataNode> _properties = [];

    /// <summary>
    /// The properties, whose keys must be unique. Whatever enters the collection, or the collection
    /// given at creation, is taken as a child and gets its key checked against the others.
    /// <para>Only set at creation, so nothing bound to it has to follow another collection.</para>
    /// </summary>
    public ObservableCollection<DataNode> Properties
    {
        get => _properties;
        init
        {
            _properties.CollectionChanged -= OnPropertiesChanged;
            _properties = value;
            _properties.CollectionChanged += OnPropertiesChanged;

            foreach (DataNode property in value)
                property.Parent = this;
            ValidateKeys(value);
            OnChanged();
        }
    }

    public IEnumerable<DataNode> Children => Properties;

    public DataObject(string? key) : base(EnumDataType.Object, key)
    {
        _properties.CollectionChanged += OnPropertiesChanged;
    }

    public void Add(DataNode node) => Properties.Add(node);

    [RelayCommand]
    public void Remove(DataNode node) => Properties.Remove(node);

    /// <summary>
    /// [baseKey] if no property uses it yet, otherwise the first of [baseKey]1, [baseKey]2... that is free.
    /// </summary>
    public string UniqueKey(string baseKey)
    {
        HashSet<string> used = [.. Properties.Select(property => property.Key).OfType<string>()];
        if (!used.Contains(baseKey))
            return baseKey;

        int index = 1;
        while (used.Contains($"{baseKey} {index}"))
            index++;
        return $"{baseKey} {index}";
    }

    private void OnPropertiesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (DataNode property in e.OldItems)
                property.ClearKeyError();
        }
        if (e.NewItems is not null)
        {
            foreach (DataNode property in e.NewItems)
                property.Parent = this;
        }

        ValidateKeys(Properties);
        OnChanged();
    }

    public override JToken? ToToken()
    {
        if (IsManual)
            return ManualToken();

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
        IsRequired = IsRequired,
        IsManual = IsManual,
        ManualEntry = ManualEntry,
    };
}

/// <summary>One choice of a closed list: the value that is filled in, under the name it is read by.</summary>
public record DataEnumOption(string Name, object? Value)
{ }

public partial class DataValue : DataNode
{
    /// <summary>What has been filled in, in the CLR type the schema's editor works in.</summary>
    [ObservableProperty]
    private object? _value;

    /// <summary>
    /// Picking a manual entry forces <see cref="Value"/>, which stays the single thing a host
    /// reads: leaving manual mode keeps whatever was forced.
    /// </summary>
    protected override void OnManualEntryPicked(DataManualValue? entry)
    {
        if (entry is not null)
            Value = entry.Value;
    }

    /// <summary> The choices a closed list offers for the enumerations. </summary>
    public ObservableCollection<DataEnumOption> Options { get; }

    private DataOptionsControl? _optionsControl;
    /// <summary>Edits [Options] from <see cref="Controls.DataEdit"/>.</summary>
    public DataOptionsControl OptionsControl => _optionsControl ??= new DataOptionsControl(this);

    /// <summary>[isNullable] is set here as the default value depends on it.</summary>
    public DataValue(EnumDataType type, string? key, IEnumerable<DataEnumOption> options, bool isNullable = false) : base(type, key)
    {
        Options = [.. options];
        Options.CollectionChanged += (_, _) => OnChanged();
        IsNullable = isNullable;
        Value = Default();
    }

    /// <summary>A forced value the editor can't hold, a reference in a number say, falls back to the default.</summary>
    protected override void OnManualModeChanged(bool isManual)
    {
        if (!isManual && !Holds(Value))
            Value = Default();
    }

    /// <summary>Adds a string option, named by its value. The first one also becomes the value when it can't be null.</summary>
    public void AddOption(string value)
    {
        Options.Add(new DataEnumOption(value, value));
        if (Value is null && !IsNullable)
            Value = value;
    }

    /// <summary>Removes [option], the value falls back to its default if it was the one picked.</summary>
    public void RemoveOption(DataEnumOption option)
    {
        if (Options.Remove(option) && Equals(Value, option.Value))
            Value = Default();
    }

    public override JToken? ToToken()
    {
        if (IsManual)
            return ManualToken();

        if (Value is null)
            return JValue.CreateNull();

        return Type switch
        {
            EnumDataType.String => Convert.ToString(Value, CultureInfo.InvariantCulture) ?? string.Empty,
            EnumDataType.Boolean => new JValue(Convert.ToBoolean(Value, CultureInfo.InvariantCulture)),
            EnumDataType.Integer => new JValue(Convert.ToInt64(Value, CultureInfo.InvariantCulture)),
            EnumDataType.Number => new JValue(Convert.ToDecimal(Value, CultureInfo.InvariantCulture)),
            EnumDataType.DateTime => new JValue(((DateTime)Value).ToString("O", CultureInfo.InvariantCulture)),
            EnumDataType.TimeSpan => new JValue(((TimeSpan)Value).ToString("c", CultureInfo.InvariantCulture)),
            _ => TokenOf(Value)
        };
    }

    /// <summary>
    /// The options (copied in a new list) and the manual entry are records, shared as they are. [ManualEntry] goes in
    /// before [Value], which it would otherwise overwrite.
    /// </summary>
    public override DataValue Clone() => new(Type, Key, Options, IsNullable)
    {
        Description = Description,
        IsManual = IsManual,
        ManualEntry = ManualEntry,
        Value = Value is JToken token ? token.DeepClone() : Value,
        IsRequired = IsRequired,
    };

    /// <summary>Whether [value] is one the editor of [Type] holds.</summary>
    private bool Holds(object? value) => value is null ? IsNullable : Type switch
    {
        EnumDataType.String => value is string,
        EnumDataType.Integer => value is long or int or short or byte,
        EnumDataType.Number => value is decimal or double or float or long or int,
        EnumDataType.Boolean => value is bool,
        EnumDataType.DateTime => value is DateTime,
        EnumDataType.TimeSpan => value is TimeSpan,
        EnumDataType.Choice => Options.Any(option => Equals(option.Value, value)),
        _ => true
    };

    /// <summary>
    /// The value a new node of [Type] starts with.
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
            EnumDataType.Choice => Options.FirstOrDefault()?.Value,
            EnumDataType.TimeSpan => TimeSpan.Zero,
            EnumDataType.DateTime => DateTime.Today,
            _ => throw new Exception($"Can't set a default value for the type [{Type}]")
        };
    }

    /// <summary> [value] as the JSON its own type amounts to, whatever the schema says it should have been. </summary>
    internal static JToken TokenOf(object? value) => value switch
    {
        null => JValue.CreateNull(),
        JToken token => token,
        _ => JToken.FromObject(value)
    };
}