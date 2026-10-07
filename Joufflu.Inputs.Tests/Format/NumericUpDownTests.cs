using System.Windows.Input;
using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Format;

public class NumericUpDownTests
{
    private FormatInputHost<NumericUpDown> _host = null!;
    private NumericUpDown Box => _host.Box;

    [SetUp]
    public void SetUp() => _host = new FormatInputHost<NumericUpDown>(new NumericUpDown());

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Starts_empty()
    {
        Assert.That(Box.Value, Is.Null);
        Assert.That(Box.Text, Is.Empty);
    }

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
    public void Typing_inserts_at_the_caret()
    {
        _host.Click(0);
        _host.Type("12");
        _host.Click(1);
        _host.Type("5");

        Assert.That(Box.Value, Is.EqualTo(152));
        Assert.That(Box.CaretIndex, Is.EqualTo(2));
    }

    [Test]
    public void A_minus_waits_for_its_digits()
    {
        _host.Click(0);
        _host.Type("-");

        Assert.That(Box.Value, Is.Null);
        Assert.That(Box.Text, Is.EqualTo("-"));

        _host.Type("4");

        Assert.That(Box.Value, Is.EqualTo(-4));
        Assert.That(Box.Text, Is.EqualTo("-4"));
    }

    [Test]
    public void Letters_are_ignored()
    {
        _host.Click(0);
        _host.Type("4a");

        Assert.That(Box.Value, Is.EqualTo(4));
        Assert.That(Box.Text, Is.EqualTo("4"));
    }

    [Test]
    public void Backspace_removes_the_character_before_the_caret()
    {
        _host.Click(0);
        _host.Type("42");
        _host.Press(Key.Back);

        Assert.That(Box.Value, Is.EqualTo(4));
        Assert.That(Box.CaretIndex, Is.EqualTo(1));
    }

    [Test]
    public void Delete_removes_the_character_after_the_caret()
    {
        _host.Click(0);
        _host.Type("42");
        _host.Click(0);
        _host.Press(Key.Delete);

        Assert.That(Box.Value, Is.EqualTo(2));
        Assert.That(Box.CaretIndex, Is.EqualTo(0));
    }

    [Test]
    public void Removing_the_last_digit_leaves_no_value()
    {
        _host.Click(0);
        _host.Type("4");
        _host.Press(Key.Back);

        Assert.That(Box.Value, Is.Null);
        Assert.That(Box.Text, Is.Empty);
    }

    [Test]
    public void Up_starts_an_empty_number_at_zero_then_counts()
    {
        _host.Click(0);
        _host.Press(Key.Up);

        Assert.That(Box.Value, Is.EqualTo(0));

        _host.Press(Key.Up);
        _host.Press(Key.Up);
        _host.Press(Key.Down);

        Assert.That(Box.Value, Is.EqualTo(1));
        Assert.That(Box.Text, Is.EqualTo("1"));
    }

    [Test]
    public void Up_puts_the_caret_at_the_end_of_the_number()
    {
        _host.Click(0);
        _host.Type("99");
        _host.Click(0);
        _host.Press(Key.Up);

        Assert.That(Box.Text, Is.EqualTo("100"));
        Assert.That(Box.CaretIndex, Is.EqualTo(3));
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
