using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;

namespace Joufflu.Data.Model;

public partial class DataArray : DataNode, IDataParent
{
    [ObservableProperty]
    private DataNode? _template;

    /// <summary>The items. Whatever enters the collection is taken as a child, whatever leaves it is let go.</summary>
    public DataNodeCollection Values { get; } = [];

    public IEnumerable<DataNode> Children => Values;

    public DataArray(string? key, DataNode? template) : base(EnumDataType.Array, key)
    {
        Template = template;
        Values.CollectionChanged += OnValuesChanged;
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
        Values.Add(node);
    }

    /// <summary>Removes [node], the items after it are keyed again by their new index.</summary>
    [RelayCommand]
    public void Remove(DataNode node)
    {
        Values.Remove(node);
        for (int i = 0; i < Values.Count; i++)
            Values[i].Key = $"[{i}]";
    }

    private void OnValuesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (DataNode value in e.OldItems)
                value.Detach();
        }
        if (e.NewItems is not null)
        {
            foreach (DataNode value in e.NewItems)
                value.Parent = this;
        }

        OnChanged();
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
            clone.Values.Add(value.Clone());

        return clone;
    }
}
