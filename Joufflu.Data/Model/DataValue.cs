using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;

namespace Joufflu.Data.Model;

/// <summary>One choice of a closed list: the value that is filled in, under the name it is read by.</summary>
public record DataChoiceOption(string Name, object? Value)
{ }

public partial class DataValue : DataNode
{
    /// <summary>What has been filled in, in the CLR type the schema's editor works in.</summary>
    [ObservableProperty]
    private object? _value;

    /// <summary>
    /// Picking a manual value sets <see cref="Value"/>, which stays the single thing a host
    /// reads: leaving manual mode keeps whatever was set.
    /// </summary>
    protected override void OnManualValuePicked(DataManualValue? manualValue)
    {
        if (manualValue is not null)
            Value = manualValue.Value;
    }

    /// <summary>The options a <see cref="EnumDataType.Choice"/> offers.</summary>
    public ObservableCollection<DataChoiceOption> Options { get; }

    /// <summary>[isNullable] is set here as the default value depends on it.</summary>
    public DataValue(EnumDataType type, string? key, IEnumerable<DataChoiceOption> options, bool isNullable = false) : base(type, key)
    {
        Options = [.. options];
        Options.CollectionChanged += (_, _) => OnChanged();
        IsNullable = isNullable;
        Value = Default();
    }

    /// <summary>A manual value the editor can't hold, a reference in a number say, falls back to the default.</summary>
    protected override void OnManualModeChanged(bool isManual)
    {
        if (!isManual && !Holds(Value))
            Value = Default();
    }

    /// <summary>
    /// Adds a string option, named by its value. The first one also becomes the value when it can't be null.
    /// Refuses an empty value and one already offered, returning false.
    /// </summary>
    public bool AddOption(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        if (Options.Any(option => Equals(option.Value, value)))
            return false;

        Options.Add(new DataChoiceOption(value, value));
        if (Value is null && !IsNullable)
            Value = value;
        return true;
    }

    /// <summary>Removes [option], the value falls back to its default if it was the one picked.</summary>
    public void RemoveOption(DataChoiceOption option)
    {
        if (Options.Remove(option) && Equals(Value, option.Value))
            Value = Default();
    }

    public override JToken? ToToken()
    {
        if (IsManual)
            return ManualToken();

        if (Value is null)
            return JValue.CreateNull();

        return Type switch
        {
            EnumDataType.String => Convert.ToString(Value, CultureInfo.InvariantCulture) ?? string.Empty,
            EnumDataType.Boolean => new JValue(Convert.ToBoolean(Value, CultureInfo.InvariantCulture)),
            EnumDataType.Integer => new JValue(Convert.ToInt64(Value, CultureInfo.InvariantCulture)),
            EnumDataType.Number => new JValue(Convert.ToDecimal(Value, CultureInfo.InvariantCulture)),
            EnumDataType.DateTime => new JValue(((DateTime)Value).ToString("O", CultureInfo.InvariantCulture)),
            EnumDataType.TimeSpan => new JValue(((TimeSpan)Value).ToString("c", CultureInfo.InvariantCulture)),
            _ => TokenOf(Value)
        };
    }

    /// <summary>
    /// The options (copied in a new list) and the manual value are records, shared as they are. [ManualValue] goes in
    /// before [Value], which it would otherwise overwrite.
    /// </summary>
    public override DataValue Clone() => new(Type, Key, Options, IsNullable)
    {
        Description = Description,
        IsManual = IsManual,
        ManualValue = ManualValue,
        Value = Value is JToken token ? token.DeepClone() : Value,
        IsRequired = IsRequired,
    };

    /// <summary>Whether [value] is one the editor of [Type] holds.</summary>
    private bool Holds(object? value) => value is null ? IsNullable : Type switch
    {
        EnumDataType.String => value is string,
        EnumDataType.Integer => value is long or int or short or byte,
        EnumDataType.Number => value is decimal or double or float or long or int,
        EnumDataType.Boolean => value is bool,
        EnumDataType.DateTime => value is DateTime,
        EnumDataType.TimeSpan => value is TimeSpan,
        EnumDataType.Choice => Options.Any(option => Equals(option.Value, value)),
        _ => true
    };

    /// <summary>
    /// The value a new node of [Type] starts with.
    /// </summary>
    private object? Default()
    {
        if (IsNullable)
            return null;

        return Type switch
        {
            EnumDataType.String => "",
            EnumDataType.Integer => 0L,
            EnumDataType.Number => 0m,
            EnumDataType.Boolean => false,
            EnumDataType.Choice => Options.FirstOrDefault()?.Value,
            EnumDataType.TimeSpan => TimeSpan.Zero,
            EnumDataType.DateTime => DateTime.Today,
            _ => throw new Exception($"Can't set a default value for the type [{Type}]")
        };
    }

    /// <summary> [value] as the JSON its own type amounts to, whatever the schema says it should have been. </summary>
    internal static JToken TokenOf(object? value) => value switch
    {
        null => JValue.CreateNull(),
        JToken token => token,
        _ => JToken.FromObject(value)
    };
}
