using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Data.Model;
using Newtonsoft.Json.Linq;
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
        new(EnumDataType.Boolean, "$previous.IsExpress"),
    ];

    public string FormCode =>
        "var schema = JsonSchema.FromType<Shipment>();\n" +
        "Node = (DataObject)schema.ToDataNode();\n\n" +
        "<data:DataForm Node=\"{Binding Node}\" ManualValues=\"{Binding ManualValues}\" />\n\n" +
        "Json = Node?.ToToken()?.ToString();";

    /// <summary>The object of the array sample, built from the <see cref="Mailing"/> schema.</summary>
    [ObservableProperty]
    private DataObject? _arrayNode;

    /// <summary>The JSON last generated from [ArrayNode].</summary>
    [ObservableProperty]
    private string? _arrayJson;

    public string ArrayCode =>
        "var node = (DataObject)JsonSchema.FromType<Mailing>().ToDataNode();\n" +
        "node.Load(JObject.Parse(\"\"\"{ \"Recipients\": [\"alice@example.com\", \"bob@example.com\"] }\"\"\"));\n\n" +
        "<data:DataForm Node=\"{Binding ArrayNode}\" />";

    public DataFormSamplesViewModel()
    {
        Generate();
        GenerateArray();
    }

    /// <summary>A fresh node from the schema, values back to their defaults.</summary>
    [RelayCommand]
    private void Generate()
    {
        var schema = JsonSchema.FromType<Shipment>();
        Node = (DataObject)schema.ToDataNode();
    }

    /// <summary>A fresh node from the <see cref="Mailing"/> schema, filled with a few recipients.</summary>
    [RelayCommand]
    private void GenerateArray()
    {
        var node = (DataObject)JsonSchema.FromType<Mailing>().ToDataNode();
        node.Load(JObject.Parse("""{ "Recipients": ["alice@example.com", "bob@example.com"] }"""));
        ArrayNode = node;
    }

    [RelayCommand]
    private void GenerateArrayJson() => ArrayJson = ArrayNode?.ToToken()?.ToString();

    [RelayCommand]
    private void GenerateJson() => Json = Node?.ToToken()?.ToString();
}
