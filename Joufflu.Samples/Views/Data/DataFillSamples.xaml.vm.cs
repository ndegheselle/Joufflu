using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Data.Model;
using NJsonSchema;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Samples.Views.Data;

public partial class DataFillSamplesViewModel : ObservableObject
{
    /// <summary>The object filled in, built from the <see cref="Order"/> schema.</summary>
    [ObservableProperty]
    private DataObject? _node;

    /// <summary>The JSON last generated from [Node].</summary>
    [ObservableProperty]
    private string? _json;

    /// <summary>What a field can be forced to, on top of null and undefined.</summary>
    public IReadOnlyList<DataManualValue> ManualValues { get; } =
    [
        new(EnumDataType.String, "TBD"),
        new(EnumDataType.Integer, -1L),
    ];

    public string FillCode =>
        "var schema = JsonSchema.FromType<Order>();\n" +
        "Node = (DataObject)schema.ToDataNode();\n\n" +
        "<jata:DataFill Node=\"{Binding Node}\" ManualValues=\"{Binding ManualValues}\" />\n\n" +
        "Json = Node?.ToToken()?.ToString();";

    public DataFillSamplesViewModel() => Generate();

    /// <summary>A fresh node from the schema, values back to their defaults.</summary>
    [RelayCommand]
    private void Generate()
    {
        var schema = JsonSchema.FromType<Order>();
        Node = (DataObject)schema.ToDataNode();
    }

    [RelayCommand]
    private void GenerateJson() => Json = Node?.ToToken()?.ToString();
}
