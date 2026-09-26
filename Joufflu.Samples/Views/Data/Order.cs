using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Joufflu.Samples.Views.Data;

/// <summary>What the data sample fills in, so the schema is one a real type would produce.</summary>
public enum EnumDelivery
{
    Standard,
    Express,
    Pickup
}

/// <summary>A line of an order, so the sample has an array of objects to fill in.</summary>
public class OrderLine
{
    public string Reference { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>The shape the data sample is filled in against, derived from this very type.</summary>
public class Order
{
    /// <summary>Required, so the sample shows a field that cannot be forced to undefined.</summary>
    public required string Customer { get; set; }
    [Required]
    public DateTime PlacedOn { get; set; }
    [Description("Whatever")]
    public EnumDelivery Delivery { get; set; }
    public bool? IsPaid { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
}
