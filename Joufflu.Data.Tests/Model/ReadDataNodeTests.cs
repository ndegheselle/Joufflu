using System.IO;
using Joufflu.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Joufflu.Data.Tests.Model;

/// <summary>A node read straight from a <see cref="JsonReader"/>, as a large file is.</summary>
public class ReadDataNodeTests
{
    private static DataNode Read(string json)
    {
        using var reader = new JsonTextReader(new StringReader(json));
        return reader.ReadDataNode();
    }

    [Test]
    public void Reads_the_node_the_parsed_token_gives()
    {
        const string json = """
            {
              "text": "a", "integer": 12, "number": 1.5, "flag": true, "date": "2026-01-02T03:04:05",
              "none": null, "items": [1, "b", { "nested": [] }], "empty": {}
            }
            """;

        JToken? read = Read(json).ToToken();
        JToken? parsed = JToken.Parse(json).ToDataNode().ToToken();

        Assert.That(JToken.DeepEquals(read, parsed), Is.True, $"{read}");
    }

    [Test]
    public void A_key_given_twice_keeps_its_last_value()
    {
        var node = (DataObject)Read("""{ "a": 1, "b": 2, "a": 3 }""");

        Assert.That(node.ToToken()!.ToString(Formatting.None), Is.EqualTo("""{"a":3,"b":2}"""));
        Assert.That(node.Properties.Any(property => property.HasErrors), Is.False);
    }

    [Test]
    public void Comments_are_skipped()
    {
        DataNode node = Read("""/* head */ [1, /* between */ 2] // tail""");

        Assert.That(node.ToToken()!.ToString(Formatting.None), Is.EqualTo("[1,2]"));
    }

    [Test]
    public void A_reader_with_no_value_is_refused()
    {
        Assert.Throws<JsonReaderException>(() => Read("  // only a comment"));
    }
}
