using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Tests.Format;

/// <summary>
/// The parts of a format: its groups, their options and the literal text between them.
/// </summary>
public class FormatEditorFormatTests
{
    private static void Type(FormatEditor editor, string text)
    {
        foreach (char character in text)
            editor.Type(character.ToString());
    }

    [Test]
    public void Literal_text_shows_between_the_groups()
    {
        var editor = new FormatEditor(
        [
            new IntegerGroup { Max = 23 },
            new FormatLiteral { Text = "h " },
            new IntegerGroup { Max = 59 },
            new FormatLiteral { Text = "m" },
        ]);

        Assert.That(editor.Text, Is.EqualTo("0h 0m"));
    }

    [Test]
    public void StringFormat_pads_the_number_with_zeros()
    {
        var editor = new FormatEditor([new IntegerGroup { StringFormat = "000" }]);

        Assert.That(editor.Text, Is.EqualTo("000"));
    }

    [Test]
    [SetCulture("en-US")]
    public void StringFormat_separates_the_thousands()
    {
        var editor = new FormatEditor([new IntegerGroup { StringFormat = "N0" }]);
        editor.Load(new object?[] { 1234L });

        Assert.That(editor.Text, Is.EqualTo("1,234"));
    }

    [Test]
    public void An_empty_group_shows_its_prompt_char_for_each_character()
    {
        var editor = new FormatEditor([new IntegerGroup { IsNullable = true, PromptChar = '_', Max = 99 }]);

        Assert.That(editor.Text, Is.EqualTo("__"));
    }

    [Test]
    public void Step_sets_what_up_adds()
    {
        var editor = new FormatEditor([new IntegerGroup { Step = 5 }]);
        editor.Spin(1);

        Assert.That(editor.GetValues(), Is.EqualTo(new object?[] { 5L }));
    }

    [Test]
    public void A_non_nullable_group_starts_within_its_min()
    {
        var editor = new FormatEditor([new DecimalGroup { Min = 1 }]);

        Assert.That(editor.Text, Is.EqualTo("1"));
    }

    [Test]
    public void A_group_after_literal_text_edits_its_own_text_only()
    {
        var editor = new FormatEditor(
        [
            new FormatLiteral { Text = "x" },
            new IntegerGroup { Max = 99, SelectsWhole = false },
        ]);
        editor.Load(new object?[] { 1L });
        editor.Select(2, 0);
        Type(editor, "2");

        Assert.That(editor.Text, Is.EqualTo("x12"));
        Assert.That(editor.SelectionStart, Is.EqualTo(3));
    }

    [Test]
    public void A_group_before_literal_text_edits_its_own_text_only()
    {
        var editor = new FormatEditor(
        [
            new IntegerGroup { Max = 99, SelectsWhole = false },
            new FormatLiteral { Text = "h" },
        ]);
        editor.Load(new object?[] { 1L });
        editor.Select(1, 0);
        Type(editor, "2");

        Assert.That(editor.Text, Is.EqualTo("12h"));
    }

    [Test]
    public void A_full_group_moves_on_to_the_group_right_after_it()
    {
        // The caret at the end of the first group is also at the start of the second.
        var editor = new FormatEditor([new IntegerGroup { Max = 9 }, new IntegerGroup { Max = 9 }]);
        editor.Select(0, 0);
        Type(editor, "12");

        Assert.That(editor.Text, Is.EqualTo("12"));
    }
}
