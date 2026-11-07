using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Joufflu.Data.Model;
using NJsonSchema.Annotations;

namespace Joufflu.Samples.Views.Data;

/// <summary>What the form sample fills in : each property shows a field the form reads from the schema.</summary>
public class Shipment
{
    [Display(Name = "Customer name")]
    [Description("Who receives the parcel.")]
    [Required]
    public string Customer { get; set; } = "";

    [Description("The delivery note, picked with the open dialog or typed.")]
    [JsonSchemaExtensionData(DataInputs.SchemaKey, DataInputs.File)]
    [JsonSchemaExtensionData(DataInputs.FileFilterSchemaKey, "PDF files|*.pdf|All files|*.*")]
    public string DeliveryNote { get; set; } = "";

    [Description("Between 1 and 20 parcels.")]
    [Range(1, 20)]
    public int Parcels { get; set; } = 1;

    [Description("Left at the door, with the neighbour...")]
    [JsonSchemaExtensionData(DataInputs.SchemaKey, DataInputs.Multiline)]
    public string Instructions { get; set; } = "";

    [Description("A French postcode : five digits.")]
    [RegularExpression("^[0-9]{5}$")]
    public string Postcode { get; set; } = "";

    public EnumDelivery Delivery { get; set; }

    [Display(Name = "Express delivery")]
    [Description("Delivered within a day, a single checkbox holding its label.")]
    public bool IsExpress { get; set; }

    public List<OrderLine> Lines { get; set; } = [];
}
