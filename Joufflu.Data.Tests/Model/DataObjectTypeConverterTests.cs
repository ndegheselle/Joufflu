using System.ComponentModel;
using Joufflu.Data.Model;
using Newtonsoft.Json.Linq;

namespace Joufflu.Data.Tests.Model;

/// <summary>
/// A JToken given where a <see cref="DataObject"/> is expected, as a binding to a node property does.
/// </summary>
public class DataObjectTypeConverterTests
{
    private static readonly TypeConverter Converter = TypeDescriptor.GetConverter(typeof(DataObject));

    [Test]
    public void JObjectConvertsToItsNode()
    {
        Assert.That(Converter.CanConvertFrom(typeof(JObject)));

        var node = (DataObject)Converter.ConvertFrom(JObject.Parse("""{"a":1,"b":"x"}"""))!;

        Assert.That(node.Properties.Select(property => property.Key), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void OtherTokensAreTheOnlyPropertyOfTheRoot()
    {
        var node = (DataObject)Converter.ConvertFrom(JArray.Parse("[1]"))!;

        Assert.That(node.Properties.Select(property => property.Key), Is.EqualTo(new[] { "value" }));
        Assert.That(node.Properties[0], Is.InstanceOf<DataArray>());
    }
}
