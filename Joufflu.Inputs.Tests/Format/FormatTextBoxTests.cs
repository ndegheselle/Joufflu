using System.Windows.Markup;
using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Tests.Format;

/// <summary>
/// The format the samples use: three unpadded, non-nullable numbers rendered "0h 0m 0s".
/// </summary>
[Apartment(ApartmentState.STA)]
public class FormatTextBoxTests
{
    private FormatInputHost<FormatTextBox>? _host;

    [TearDown]
    public void TearDown() => _host?.Dispose();

    private FormatTextBox Show(FormatTextBox box)
    {
        _host = new FormatInputHost<FormatTextBox>(box);
        return box;
    }

    private static FormatTextBox HoursMinutesSeconds()
    {
        var box = new FormatTextBox();
        box.Parts.Add(new IntegerGroup { Max = 23 });
        box.Parts.Add(new FormatLiteral { Text = "h " });
        box.Parts.Add(new IntegerGroup { Max = 59 });
        box.Parts.Add(new FormatLiteral { Text = "m " });
        box.Parts.Add(new IntegerGroup { Max = 59 });
        box.Parts.Add(new FormatLiteral { Text = "s" });
        return box;
    }

    [Test]
    public void Parts_declared_in_xaml_are_shown()
    {
        const string xaml = "<FormatTextBox xmlns=\"clr-namespace:Joufflu.Inputs.Controls.Format;assembly=Joufflu.Inputs\">"
            + "<IntegerGroup Max=\"23\" StringFormat=\"00\" />"
            + "<FormatLiteral Text=\"h\" />"
            + "</FormatTextBox>";
        FormatTextBox box = Show((FormatTextBox)XamlReader.Parse(xaml));

        Assert.That(box.Text, Is.EqualTo("00h"));
    }

    [Test]
    public void Values_set_before_initialization_show_once_initialized()
    {
        FormatTextBox box = HoursMinutesSeconds();
        box.Values = new List<object?> { 1, 2, 3 };
        Show(box);

        Assert.That(box.Text, Is.EqualTo("1h 2m 3s"));
    }

    [Test]
    public void Values_set_from_outside_show_in_the_text()
    {
        FormatTextBox box = Show(HoursMinutesSeconds());
        box.Values = new List<object?> { 4L, 5L, 6L };

        Assert.That(box.Text, Is.EqualTo("4h 5m 6s"));
    }

    [Test]
    public void Typing_updates_Values()
    {
        FormatTextBox box = Show(HoursMinutesSeconds());
        var raised = new List<List<object?>>();
        box.ValuesChanged += (_, values) => raised.Add(values);

        _host!.Click(0);
        _host.Type("5");

        Assert.That(box.Text, Is.EqualTo("5h 0m 0s"));
        Assert.That(box.Values, Is.EqualTo(new object?[] { 5L, 0L, 0L }));
        Assert.That(raised, Has.Count.EqualTo(1));
    }

    [Test]
    public void Changing_the_parts_once_initialized_shows_them()
    {
        FormatTextBox box = Show(HoursMinutesSeconds());
        box.Parts.Clear();
        box.Parts.Add(new IntegerGroup { Max = 9 });
        box.Parts.Add(new FormatLiteral { Text = "x" });

        Assert.That(box.Text, Is.EqualTo("0x"));
    }

    [Test]
    public void Loading_again_keeps_what_was_typed()
    {
        FormatTextBox box = Show(HoursMinutesSeconds());
        _host!.Click(0);
        _host.Type("5");

        _host.Reload();

        Assert.That(box.Text, Is.EqualTo("5h 0m 0s"));
        Assert.That(box.Values, Is.EqualTo(new object?[] { 5L, 0L, 0L }));
    }
}
