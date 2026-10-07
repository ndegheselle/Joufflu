using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;

namespace Joufflu.Data.Model;

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
