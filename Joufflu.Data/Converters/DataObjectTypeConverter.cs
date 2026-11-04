using System.ComponentModel;
using System.Globalization;
using Joufflu.Data.Model;
using Newtonsoft.Json.Linq;

namespace Joufflu.Data.Converters;

/// <summary>
/// Lets a <see cref="JToken"/> be given where a <see cref="DataNode"/> is expected, a binding to
/// <see cref="Controls.DataDisplay.Node"/> for one: the token is read as the node of
/// <c>ToDataNode</c>, types taken from its values.
/// </summary>
public class DataNodeTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => typeof(JToken).IsAssignableFrom(sourceType) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => value is JToken token ? token.ToDataNode() : base.ConvertFrom(context, culture, value);
}

/// <summary>
/// Same for a <see cref="DataObject"/>, which the editors take as root: a token that is not an
/// object (an array or a value) has no root object of its own, so it is shown as the only
/// property of one, under <see cref="RootKey"/>.
/// </summary>
public class DataObjectTypeConverter : DataNodeTypeConverter
{
    /// <summary>The key of a token that is not an object, as the only property of the root.</summary>
    public const string RootKey = "value";

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not JToken token)
            return base.ConvertFrom(context, culture, value);

        return token.ToDataNode() as DataObject
            ?? new DataObject(null) { Properties = [token.ToDataNode(RootKey)] };
    }
}
