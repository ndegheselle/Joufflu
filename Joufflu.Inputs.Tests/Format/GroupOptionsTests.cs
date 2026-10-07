using System.Windows.Input;
using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Tests.Format;

/// <summary>
/// The options a group is written with, seen through what the box shows.
/// </summary>
public class GroupOptionsTests
{
    private FormatInputHost<FormatTextBox>? _host;

    [TearDown]
    public void TearDown() => _host?.Dispose();

    private FormatTextBox Show(string format, string? globalFormat = null)
    {
        var box = new FormatTextBox { Format = format, GlobalFormat = globalFormat };
        _host = new FormatInputHost<FormatTextBox>(box);
        return box;
    }

    [Test]
    public void Length_and_padded_fill_the_group_with_zeros()
    {
        FormatTextBox box = Show("{numeric|length:3|padded}");

        Assert.That(box.Text, Is.EqualTo("000"));
    }

    [Test]
    public void A_nullable_group_shows_its_nullable_char_for_each_character()
    {
        FormatTextBox box = Show("{numeric|nullable|nullableChar:_|length:2}");

        Assert.That(box.Text, Is.EqualTo("__"));
    }

    [Test]
    [SetCulture("en-US")]
    public void Format_applies_a_string_format()
    {
        FormatTextBox box = Show("{numeric|format::N0}");
        box.Values = new List<object?> { 1234L };

        Assert.That(box.Text, Is.EqualTo("1,234"));
    }

    [Test]
    public void IncrementDelta_sets_the_step()
    {
        FormatTextBox box = Show("{numeric|incrementDelta:5}");
        _host!.Click(0);
        _host.Press(Key.Up);

        Assert.That(box.Values, Is.EqualTo(new object?[] { 5L }));
    }

    [Test]
    public void A_non_nullable_group_starts_within_its_min()
    {
        FormatTextBox box = Show("{decimal|min:1}");

        Assert.That(box.Text, Is.EqualTo("1"));
    }

    [Test]
    public void The_group_options_override_the_global_ones()
    {
        FormatTextBox box = Show("{max:99}", "numeric|max:9");
        _host!.Click(0);
        _host.Type("50");

        Assert.That(box.Values, Is.EqualTo(new object?[] { 50L }));
    }
}
