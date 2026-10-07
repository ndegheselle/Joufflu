using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Tests.Format;

/// <summary>
/// The format string: its groups, their options and the literal text between them.
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
        var editor = new FormatEditor("{max:23}h {max:59}m {max:59}s", "numeric");

        Assert.That(editor.Text, Is.EqualTo("0h 0m 0s"));
    }

    [Test]
    public void An_unknown_option_is_refused()
    {
        Assert.Throws<ArgumentException>(() => new FormatEditor("{bogus}", "numeric"));
    }

    [Test]
    public void A_group_without_a_type_is_refused()
    {
        Assert.Throws<ArgumentException>(() => new FormatEditor("{max:9}x", null));
    }

    [Test]
    public void Length_and_padded_fill_the_group_with_zeros()
    {
        var editor = new FormatEditor("{numeric|length:3|padded}", null);

        Assert.That(editor.Text, Is.EqualTo("000"));
    }

    [Test]
    public void A_nullable_group_shows_its_nullable_char_for_each_character()
    {
        var editor = new FormatEditor("{numeric|nullable|nullableChar:_|length:2}", null);

        Assert.That(editor.Text, Is.EqualTo("__"));
    }

    [Test]
    [SetCulture("en-US")]
    public void Format_applies_a_string_format()
    {
        var editor = new FormatEditor("{numeric|format::N0}", null);
        editor.Load(new object?[] { 1234L });

        Assert.That(editor.Text, Is.EqualTo("1,234"));
    }

    [Test]
    public void IncrementDelta_sets_the_step()
    {
        var editor = new FormatEditor("{numeric|incrementDelta:5}", null);
        editor.Spin(1);

        Assert.That(editor.GetValues(), Is.EqualTo(new object?[] { 5L }));
    }

    [Test]
    public void A_non_nullable_group_starts_within_its_min()
    {
        var editor = new FormatEditor("{decimal|min:1}", null);

        Assert.That(editor.Text, Is.EqualTo("1"));
    }

    [Test]
    public void The_group_options_override_the_global_ones()
    {
        var editor = new FormatEditor("{max:99}", "numeric|max:9");
        editor.Select(0, 0);
        Type(editor, "50");

        Assert.That(editor.GetValues(), Is.EqualTo(new object?[] { 50L }));
    }

    [Test]
    public void A_group_after_literal_text_edits_its_own_text_only()
    {
        var editor = new FormatEditor("x{numeric|length:2|noGlobalSelection}", null);
        editor.Load(new object?[] { 1L });
        editor.Select(2, 0);
        Type(editor, "2");

        Assert.That(editor.Text, Is.EqualTo("x12"));
        Assert.That(editor.SelectionStart, Is.EqualTo(3));
    }

    [Test]
    public void A_group_before_literal_text_edits_its_own_text_only()
    {
        var editor = new FormatEditor("{numeric|length:2|noGlobalSelection}h", null);
        editor.Load(new object?[] { 1L });
        editor.Select(1, 0);
        Type(editor, "2");

        Assert.That(editor.Text, Is.EqualTo("12h"));
    }

    [Test]
    public void A_full_group_moves_on_to_the_group_right_after_it()
    {
        // The caret at the end of the first group is also at the start of the second.
        var editor = new FormatEditor("{max:9}{max:9}", "numeric");
        editor.Select(0, 0);
        Type(editor, "12");

        Assert.That(editor.Text, Is.EqualTo("12"));
    }
}
