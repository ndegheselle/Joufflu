using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Data.Model;
using Newtonsoft.Json.Linq;
using NJsonSchema;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Samples.Views.Data;

public partial class DataDisplaySamplesViewModel : ObservableObject
{
    /// <summary>The JSON shown, with a manual value and a null among the others.</summary>
    public string Json { get; } = """
        {
          "Customer": "TBD",
          "PlacedOn": "2026-09-27T00:00:00",
          "Delivery": 1,
          "IsPaid": null,
          "Lines": [
            { "Reference": "A-102", "Quantity": 2, "UnitPrice": 12.5 },
            { "Reference": "B-7", "Quantity": 1, "UnitPrice": 99.9 }
          ]
        }
        """;

    /// <summary>The <see cref="Order"/> schema's tree, filled with [Json].</summary>
    public DataObject Node { get; }

    public string DisplayCode =>
        "Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();\n" +
        "Node.Load(JToken.Parse(json), ManualValues);\n\n" +
        "<jata:DataDisplay Node=\"{Binding Node}\" />";

    public DataDisplaySamplesViewModel()
    {
        Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();
        Node.Load(JToken.Parse(Json), [new DataManualValue(EnumDataType.String, "TBD")]);
    }
}
