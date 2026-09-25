using NJsonSchema;

namespace Joufflu.Data.Model;

public record DataManualValue(EnumDataType? Type, object? Value)
{
    /// <summary>Offered by a field its object does not require.</summary>
    public static readonly DataManualValue Undefined = new DataUndefined();
    public static readonly DataManualValue Null = new DataManualValue(null, null);

    /// <summary>
    /// The same text the list reads by.
    /// </summary>
    public override string ToString() => Value?.ToString() ?? "null";

    /// <summary>
    /// Whether the entry can be forced into a field of [type]. 
    /// </summary>
    public bool Fits(EnumDataType type) => Type == null || Type == type;
}


public record DataUndefined : DataManualValue
{
    public DataUndefined() : base(null, null)
    {}

    public override string ToString() => $"undefined";
}