using NJsonSchema;

namespace Joufflu.Data.Model;

public record DataManualValue(JsonObjectType Type, object? Value)
{
    /// <summary>Offered by a field whose schema takes null.</summary>
    public static readonly DataManualValue Null = new(JsonObjectType.Null, null);
    /// <summary>Offered by a field its object does not require.</summary>
    public static readonly DataManualValue Undefined = new DataUndefined();

    /// <summary>
    /// The same text the list reads by. A searchable combo box falls back to this when it cannot
    /// go through a DisplayMemberPath — which an item template rules out — so it is what is typed
    /// against and what is shown once the entry is picked, rather than the record's own dump.
    /// </summary>
    public override string ToString() => Value?.ToString() ?? "null";

    /// <summary>
    /// Whether the entry can be forced into a field of [type]. An entry carrying no type of its own
    /// fits anywhere; otherwise the two have to share a flag, <see cref="JsonObjectType"/> being a
    /// flag enum.
    /// </summary>
    public bool Fits(JsonObjectType type) => Type == JsonObjectType.None || (Type & type) != 0;
}


public record DataUndefined : DataManualValue
{
    public DataUndefined() : base(JsonObjectType.None, null)
    {
    }

    public override string ToString() => $"undefined";
}