using System.Globalization;
using System.Text.RegularExpressions;

namespace Joufflu.Data.Model;

/// <summary>How a string is typed in a <see cref="Controls.DataForm"/>, read from the schema (see <see cref="DataInputs"/>).</summary>
public enum EnumDataInput
{
    /// <summary>A line of text.</summary>
    Text,

    /// <summary>A text of several lines.</summary>
    Multiline,

    /// <summary>A path to a file, typed or picked with the open dialog of the system.</summary>
    File,
}

/// <summary>
/// The keys of a schema telling how a string is typed, which JSON Schema has no word for. Written
/// in the schema of the string, next to <c>type</c> :
/// <code>{"type":"string","x-input":"file","x-file-filter":"CSV files|*.csv|All files|*.*"}</code>
/// </summary>
public static class DataInputs
{
    /// <summary>The input, <see cref="File"/> or <see cref="Multiline"/>, a line of text otherwise.</summary>
    public const string SchemaKey = "x-input";

    /// <summary>The file types the open dialog of a <see cref="File"/> offers, as <c>Microsoft.Win32.OpenFileDialog.Filter</c> reads them.</summary>
    public const string FileFilterSchemaKey = "x-file-filter";

    public const string File = "file";
    public const string Multiline = "multiline";

    /// <summary>The input [value], the value of <see cref="SchemaKey"/>, stands for.</summary>
    public static EnumDataInput InputOf(object? value) => value switch
    {
        File => EnumDataInput.File,
        Multiline => EnumDataInput.Multiline,
        _ => EnumDataInput.Text,
    };
}

/// <summary>
/// What a value of a schema has to be on top of its type : the range of a number, the length and
/// the pattern of a string. Checked as the value is filled in, see <see cref="DataValue.Error"/>.
/// </summary>
public sealed record DataRules(
    decimal? Minimum = null,
    decimal? Maximum = null,
    int? MinLength = null,
    int? MaxLength = null,
    string? Pattern = null)
{
    public static readonly DataRules None = new();

    /// <summary>Why [value] breaks the rules, null when it doesn't. Null breaks none : whether it is taken is the type's business.</summary>
    public string? ErrorOf(object? value) => value switch
    {
        string text => ErrorOfText(text),
        long or int or short or byte or decimal or double or float => ErrorOfNumber(Convert.ToDecimal(value, CultureInfo.InvariantCulture)),
        _ => null,
    };

    private string? ErrorOfNumber(decimal number)
    {
        if (number < Minimum)
            return $"At least {Text(Minimum.Value)}.";
        if (number > Maximum)
            return $"At most {Text(Maximum.Value)}.";
        return null;
    }

    private string? ErrorOfText(string text)
    {
        if (text.Length < MinLength)
            // A minimum length of one is what a required string amounts to.
            return MinLength == 1 ? "Required." : $"At least {MinLength} characters.";
        if (text.Length > MaxLength)
            return $"At most {MaxLength} characters.";
        if (Pattern != null && !Regex.IsMatch(text, Pattern))
            return $"Doesn't match the expected format ({Pattern}).";
        return null;
    }

    /// <summary>[number] without the trailing zeros a schema read as a double leaves.</summary>
    private static string Text(decimal number) => number.ToString("G29", CultureInfo.CurrentCulture);
}
