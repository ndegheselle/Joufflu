using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Tests.Format;

/// <summary>
/// A lone number keeping its own caret, the format of <c>NumericUpDown</c>.
/// </summary>
public class FormatEditorNumberTests
{
    private FormatEditor _editor = null!;

    [SetUp]
    public void SetUp() => _editor = new FormatEditor([new IntegerGroup { IsNullable = true, SelectsWhole = false }]);

    private void Type(string text)
    {
        foreach (char character in text)
            _editor.Type(character.ToString());
    }

    [Test]
    public void Starts_empty()
    {
        Assert.That(_editor.Text, Is.Empty);
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { null }));
    }

    [Test]
    public void Typing_digits_builds_the_number()
    {
        _editor.Select(0, 0);
        Type("42");

        Assert.That(_editor.Text, Is.EqualTo("42"));
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { 42L }));
        Assert.That(_editor.SelectionStart, Is.EqualTo(2));
    }

    [Test]
    public void Typing_with_no_group_selected_starts_in_the_first()
    {
        Type("4");

        Assert.That(_editor.Text, Is.EqualTo("4"));
    }

    [Test]
    public void Typing_inserts_at_the_caret()
    {
        Type("12");
        _editor.Select(1, 0);
        Type("5");

        Assert.That(_editor.Text, Is.EqualTo("152"));
        Assert.That(_editor.SelectionStart, Is.EqualTo(2));
    }

    [Test]
    public void Typing_replaces_the_selection()
    {
        Type("123");
        _editor.Select(1, 1);
        Type("9");

        Assert.That(_editor.Text, Is.EqualTo("193"));
        Assert.That(_editor.SelectionStart, Is.EqualTo(2));
    }

    [Test]
    public void A_minus_waits_for_its_digits()
    {
        Type("-");

        Assert.That(_editor.Text, Is.EqualTo("-"));
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { null }));

        Type("4");

        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { -4L }));
    }

    [Test]
    public void Letters_are_ignored()
    {
        Type("4a");

        Assert.That(_editor.Text, Is.EqualTo("4"));
    }

    [Test]
    public void Backspace_removes_the_character_before_the_caret()
    {
        Type("42");
        _editor.Delete(backwards: true);

        Assert.That(_editor.Text, Is.EqualTo("4"));
        Assert.That(_editor.SelectionStart, Is.EqualTo(1));
    }

    [Test]
    public void Delete_removes_the_character_after_the_caret()
    {
        Type("42");
        _editor.Select(0, 0);
        _editor.Delete(backwards: false);

        Assert.That(_editor.Text, Is.EqualTo("2"));
        Assert.That(_editor.SelectionStart, Is.EqualTo(0));
    }

    [Test]
    public void Removing_the_last_digit_leaves_no_value()
    {
        Type("4");
        _editor.Delete(backwards: true);

        Assert.That(_editor.Text, Is.Empty);
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { null }));
    }

    [Test]
    public void Up_starts_an_empty_number_at_zero_then_counts()
    {
        _editor.Spin(1);

        Assert.That(_editor.Text, Is.EqualTo("0"));

        _editor.Spin(1);
        _editor.Spin(1);
        _editor.Spin(-1);

        Assert.That(_editor.Text, Is.EqualTo("1"));
    }

    [Test]
    public void Up_puts_the_caret_at_the_end_of_the_number()
    {
        Type("99");
        _editor.Select(0, 0);
        _editor.Spin(1);

        Assert.That(_editor.Text, Is.EqualTo("100"));
        Assert.That(_editor.SelectionStart, Is.EqualTo(3));
    }

    [Test]
    public void Clear_empties_the_number()
    {
        Type("42");
        _editor.Clear();

        Assert.That(_editor.Text, Is.Empty);
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { null }));
    }
}

/// <summary>
/// A lone decimal keeping its own caret, the format of <c>DecimalUpDown</c>. The decimal
/// separator follows the current culture, so it is pinned.
/// </summary>
[SetCulture("fr-FR")]
public class FormatEditorDecimalTests
{
    private FormatEditor _editor = null!;

    [SetUp]
    public void SetUp() => _editor = new FormatEditor([new DecimalGroup { IsNullable = true, SelectsWhole = false }]);

    private void Type(string text)
    {
        foreach (char character in text)
            _editor.Type(character.ToString());
    }

    [Test]
    public void A_separator_waits_for_its_fraction()
    {
        Type("3,");

        Assert.That(_editor.Text, Is.EqualTo("3,"));
        Assert.That(_editor.SelectionStart, Is.EqualTo(2));

        Type("5");

        Assert.That(_editor.Text, Is.EqualTo("3,5"));
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { 3.5m }));
    }

    [Test]
    public void A_point_is_typed_as_the_culture_separator()
    {
        Type("3.5");

        Assert.That(_editor.Text, Is.EqualTo("3,5"));
    }

    [Test]
    [SetCulture("en-US")]
    public void A_comma_is_typed_as_the_culture_separator()
    {
        Type("3,5");

        Assert.That(_editor.Text, Is.EqualTo("3.5"));
    }

    [Test]
    public void Up_counts_by_a_tenth()
    {
        _editor.Spin(1);
        _editor.Spin(1);

        Assert.That(_editor.Text, Is.EqualTo("0,1"));
    }

    [Test]
    public void A_loaded_value_is_shown_in_the_culture()
    {
        _editor.Load(new object?[] { 2.25m });

        Assert.That(_editor.Text, Is.EqualTo("2,25"));
    }
}
