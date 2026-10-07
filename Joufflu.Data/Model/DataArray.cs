using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;

namespace Joufflu.Data.Model;

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
