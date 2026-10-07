using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Tests.Format;

/// <summary>
/// The format the samples use: three unpadded, non-nullable numbers rendered "0h 0m 0s".
/// </summary>
public class FormatTextBoxTests
{
    private const string Format = "{max:23}h {max:59}m {max:59}s";

    private FormatInputHost<FormatTextBox>? _host;

    [TearDown]
    public void TearDown() => _host?.Dispose();

    private FormatTextBox Show(FormatTextBox box)
    {
        _host = new FormatInputHost<FormatTextBox>(box);
        return box;
    }

    [Test]
    public void Format_set_before_GlobalFormat_is_parsed_once_loaded()
    {
        // XAML sets attributes in the order they are written, Format first here.
        FormatTextBox box = Show(new FormatTextBox { Format = Format, GlobalFormat = "numeric" });

        Assert.That(box.Text, Is.EqualTo("0h 0m 0s"));
    }

    [Test]
    public void Values_set_before_loading_show_once_loaded()
    {
        var values = new List<object?> { 1, 2, 3 };
        FormatTextBox box = Show(new FormatTextBox { Format = Format, GlobalFormat = "numeric", Values = values });

        Assert.That(box.Text, Is.EqualTo("1h 2m 3s"));
    }

    [Test]
    public void Values_set_from_outside_show_in_the_text()
    {
        FormatTextBox box = Show(new FormatTextBox { Format = Format, GlobalFormat = "numeric" });
        box.Values = new List<object?> { 4L, 5L, 6L };

        Assert.That(box.Text, Is.EqualTo("4h 5m 6s"));
    }

    [Test]
    public void Typing_updates_Values()
    {
        FormatTextBox box = Show(new FormatTextBox { Format = Format, GlobalFormat = "numeric" });
        var raised = new List<List<object?>>();
        box.ValuesChanged += (_, values) => raised.Add(values);

        _host!.Click(0);
        _host.Type("5");

        Assert.That(box.Text, Is.EqualTo("5h 0m 0s"));
        Assert.That(box.Values, Is.EqualTo(new object?[] { 5L, 0L, 0L }));
        Assert.That(raised, Has.Count.EqualTo(1));
    }

    [Test]
    public void Changing_the_format_once_loaded_parses_it_again()
    {
        FormatTextBox box = Show(new FormatTextBox { Format = Format, GlobalFormat = "numeric" });
        box.Format = "{max:9}x";

        Assert.That(box.Text, Is.EqualTo("0x"));
    }

    [Test]
    public void An_unknown_option_is_refused()
    {
        FormatTextBox box = Show(new FormatTextBox { Format = Format, GlobalFormat = "numeric" });

        Assert.Throws<ArgumentException>(() => box.Format = "{bogus}");
    }

    [Test]
    public void A_group_without_a_type_is_refused()
    {
        FormatTextBox box = Show(new FormatTextBox());

        Assert.Throws<ArgumentException>(() => box.Format = "{max:9}x");
    }

    [Test]
    public void A_group_after_literal_text_edits_its_own_text_only()
    {
        FormatTextBox box = Show(new FormatTextBox { Format = "x{numeric|length:2|noGlobalSelection}" });
        box.Values = new List<object?> { 1L };
        _host!.Click(2);
        _host.Type("2");

        Assert.That(box.Text, Is.EqualTo("x12"));
        Assert.That(box.CaretIndex, Is.EqualTo(3));
    }

    [Test]
    public void A_group_before_literal_text_edits_its_own_text_only()
    {
        FormatTextBox box = Show(new FormatTextBox { Format = "{numeric|length:2|noGlobalSelection}h" });
        box.Values = new List<object?> { 1L };
        _host!.Click(1);
        _host.Type("2");

        Assert.That(box.Text, Is.EqualTo("12h"));
    }

    [Test]
    public void Loading_again_keeps_what_was_typed()
    {
        FormatTextBox box = Show(new FormatTextBox { Format = Format, GlobalFormat = "numeric" });
        _host!.Click(0);
        _host.Type("5");

        _host.Reload();

        Assert.That(box.Text, Is.EqualTo("5h 0m 0s"));
        Assert.That(box.Values, Is.EqualTo(new object?[] { 5L, 0L, 0L }));
    }
}
