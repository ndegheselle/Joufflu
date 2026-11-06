using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Data.Model;
using NJsonSchema;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Samples.Views.Data;

public partial class DataFormSamplesViewModel : ObservableObject
{
    /// <summary>The object filled in, built from the <see cref="Shipment"/> schema.</summary>
    [ObservableProperty]
    private DataObject? _node;

    /// <summary>The JSON last generated from [Node].</summary>
    [ObservableProperty]
    private string? _json;

    /// <summary>What a field can point at instead of holding a value : only the fields they fit offer it.</summary>
    public IReadOnlyList<DataManualValue> ManualValues { get; } =
    [
        new(EnumDataType.String, "$previous.Customer"),
        new(EnumDataType.Integer, "$previous.Count"),
    ];

    public string FormCode =>
        "var schema = JsonSchema.FromType<Shipment>();\n" +
        "Node = (DataObject)schema.ToDataNode();\n\n" +
        "<data:DataForm Node=\"{Binding Node}\" ManualValues=\"{Binding ManualValues}\" />\n\n" +
        "Json = Node?.ToToken()?.ToString();";

    public DataFormSamplesViewModel() => Generate();

    /// <summary>A fresh node from the schema, values back to their defaults.</summary>
    [RelayCommand]
    private void Generate()
    {
        var schema = JsonSchema.FromType<Shipment>();
        Node = (DataObject)schema.ToDataNode();
    }

    [RelayCommand]
    private void GenerateJson() => Json = Node?.ToToken()?.ToString();
}
