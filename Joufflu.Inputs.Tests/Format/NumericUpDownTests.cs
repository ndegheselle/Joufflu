using System.Windows.Input;
using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Format;

[Apartment(ApartmentState.STA)]
public class NumericUpDownTests
{
    private FormatInputHost<NumericUpDown> _host = null!;
    private NumericUpDown Box => _host.Box;

    [SetUp]
    public void SetUp() => _host = new FormatInputHost<NumericUpDown>(new NumericUpDown());

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Typing_digits_builds_the_number()
    {
        _host.Click(0);
        _host.Type("42");

        Assert.That(Box.Value, Is.EqualTo(42));
        Assert.That(Box.Text, Is.EqualTo("42"));
        Assert.That(Box.CaretIndex, Is.EqualTo(2));
    }

    [Test]
    public void Up_starts_an_empty_number_at_zero_then_counts()
    {
        _host.Click(0);
        _host.Press(Key.Up);
        _host.Press(Key.Up);

        Assert.That(Box.Value, Is.EqualTo(1));
    }

    [Test]
    public void A_value_set_from_outside_shows_in_the_text()
    {
        Box.Value = 12;

        Assert.That(Box.Text, Is.EqualTo("12"));
        Assert.That(Box.Values, Is.EqualTo(new object?[] { 12L }));

        Box.Value = null;

        Assert.That(Box.Text, Is.Empty);
    }

    [Test]
    public void ValueChanged_is_raised_once_per_new_value()
    {
        var raised = new List<long?>();
        Box.ValueChanged += (_, value) => raised.Add(value);

        _host.Click(0);
        _host.Type("42");

        Assert.That(raised, Is.EqualTo(new long?[] { 4, 42 }));
    }
}
