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

    /// <summary>
    /// Whether the entry can be picked as it is, rather than only shown as a hint.
    /// </summary>
    public virtual bool IsPickable => true;
}

/// <summary>
/// Entries made of a fixed text around a name the user types, written with the name between braces :
/// <c>$row.columns['{column name}']</c> offers <c>$row.columns['unit price']</c> once "unit price" is
/// typed. Until then it is shown as a hint, the braces left out, and can't be picked.
/// </summary>
public record DataManualPattern : DataManualValue
{
    public string Prefix { get; }
    public string Suffix { get; }

    public DataManualPattern(EnumDataType? type, string pattern) : base(type, HintOf(pattern))
    {
        int start = pattern.IndexOf('{');
        int end = pattern.IndexOf('}', start + 1);
        if (start < 0 || end < 0)
            throw new ArgumentException($"[{pattern}] has no {{name}} to fill in.", nameof(pattern));

        Prefix = pattern[..start];
        Suffix = pattern[(end + 1)..];
    }

    public override bool IsPickable => false;

    public override string ToString() => base.ToString();

    private static string HintOf(string pattern) => pattern.Replace("{", "").Replace("}", "");

    /// <summary>
    /// The entry [text] fills the pattern with, null when there is nothing to fill it with. [text] can
    /// be the name alone or the whole entry being typed : the fixed text is not written twice.
    /// </summary>
    public DataManualValue? Fill(string? text)
    {
        // Still typing the fixed text, which the hint already shows.
        if (string.IsNullOrEmpty(text) || Prefix.StartsWith(text, StringComparison.Ordinal))
            return null;

        string name = text.StartsWith(Prefix, StringComparison.Ordinal) ? text[Prefix.Length..] : text;
        if (Suffix.Length > 0 && name.EndsWith(Suffix, StringComparison.Ordinal))
            name = name[..^Suffix.Length];
        if (name.Length == 0)
            return null;

        return new DataManualValue(Type, $"{Prefix}{name}{Suffix}");
    }

    /// <summary>Whether [value] is an entry of the pattern, a name filling it.</summary>
    public bool Matches(object? value)
        => value is string text
            && text.Length > Prefix.Length + Suffix.Length
            && text.StartsWith(Prefix, StringComparison.Ordinal)
            && text.EndsWith(Suffix, StringComparison.Ordinal);
}


public record DataUndefined : DataManualValue
{
    public DataUndefined() : base(null, null)
    {}

    public override string ToString() => $"undefined";
}