using System.Windows.Input;
using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Format;

// The decimal separator follows the current culture, so it is pinned.
[SetCulture("fr-FR")]
public class DecimalUpDownTests
{
    private FormatInputHost<DecimalUpDown> _host = null!;
    private DecimalUpDown Box => _host.Box;

    [SetUp]
    public void SetUp() => _host = new FormatInputHost<DecimalUpDown>(new DecimalUpDown());

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void A_separator_waits_for_its_fraction()
    {
        _host.Click(0);
        _host.Type("3,");

        Assert.That(Box.Text, Is.EqualTo("3,"));
        Assert.That(Box.CaretIndex, Is.EqualTo(2));

        _host.Type("5");

        Assert.That(Box.Value, Is.EqualTo(3.5m));
        Assert.That(Box.Text, Is.EqualTo("3,5"));
    }

    [Test]
    public void A_point_is_typed_as_the_culture_separator()
    {
        _host.Click(0);
        _host.Type("3.5");

        Assert.That(Box.Value, Is.EqualTo(3.5m));
        Assert.That(Box.Text, Is.EqualTo("3,5"));
    }

    [Test]
    [SetCulture("en-US")]
    public void A_comma_is_typed_as_the_culture_separator()
    {
        _host.Click(0);
        _host.Type("3,5");

        Assert.That(Box.Value, Is.EqualTo(3.5m));
        Assert.That(Box.Text, Is.EqualTo("3.5"));
    }

    [Test]
    public void Up_counts_by_a_tenth()
    {
        _host.Click(0);
        _host.Press(Key.Up);
        _host.Press(Key.Up);

        Assert.That(Box.Value, Is.EqualTo(0.1m));
        Assert.That(Box.Text, Is.EqualTo("0,1"));
    }

    [Test]
    public void A_value_set_from_outside_shows_in_the_text()
    {
        Box.Value = 2.25m;

        Assert.That(Box.Text, Is.EqualTo("2,25"));
    }
}
