using System.Globalization;
using Joufflu.Data.Converters;
using Joufflu.Data.Model;

namespace Joufflu.Data.Tests.Controls;

/// <summary>The text a value reads by in a row of <see cref="Joufflu.Data.Controls.DataDisplay"/>, one line high.</summary>
public class DataValueTextConverterTests
{
    private static string TextOf(string value)
    {
        var node = new DataValue(EnumDataType.String, "text", []) { Value = value };
        return (string)new DataValueTextConverter().Convert([node], typeof(string), null!, CultureInfo.InvariantCulture);
    }

    [Test]
    public void A_line_break_shows_as_a_mark_on_the_same_line()
    {
        Assert.That(TextOf("one\ntwo"), Is.EqualTo("one↵two"));
    }

    [Test]
    public void A_long_text_is_cut_with_an_ellipsis()
    {
        string text = TextOf(new string('a', 5000));

        Assert.That(text, Has.Length.EqualTo(1001));
        Assert.That(text, Does.EndWith("…"));
    }
}
