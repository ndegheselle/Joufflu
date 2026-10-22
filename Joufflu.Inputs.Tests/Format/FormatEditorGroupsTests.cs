using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Tests.Format;

/// <summary>
/// Several groups selected whole, the format of <c>TimeSpanPicker</c>: days / hours / minutes /
/// seconds rendered "000d 00h 00m 00s", days starting at 0, hours at 5, minutes at 9, seconds
/// at 13.
/// </summary>
public class FormatEditorGroupsTests
{
    private const int Days = 0;
    private const int Hours = 5;
    private const int Minutes = 9;
    private const int Seconds = 13;

    private FormatEditor _editor = null!;

    [SetUp]
    public void SetUp() => _editor = new FormatEditor(
    [
        TimeGroup(max: 365, stringFormat: "000"),
        new FormatLiteral { Text = "d " },
        TimeGroup(max: 23, stringFormat: "00"),
        new FormatLiteral { Text = "h " },
        TimeGroup(max: 59, stringFormat: "00"),
        new FormatLiteral { Text = "m " },
        TimeGroup(max: 59, stringFormat: "00"),
        new FormatLiteral { Text = "s" },
    ]);

    private static IntegerGroup TimeGroup(long max, string stringFormat)
        => new IntegerGroup { Min = 0, Max = max, StringFormat = stringFormat, IsNullable = true };

    private void Type(string text)
    {
        foreach (char character in text)
            _editor.Type(character.ToString());
    }

    [Test]
    public void Starts_with_placeholders()
    {
        Assert.That(_editor.Text, Is.EqualTo("---d --h --m --s"));
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { null, null, null, null }));
    }

    [Test]
    public void Loaded_values_fill_each_group()
    {
        _editor.Load(new object?[] { 1, 2, 3, 4 });

        Assert.That(_editor.Text, Is.EqualTo("001d 02h 03m 04s"));
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { 1L, 2L, 3L, 4L }));
    }

    [Test]
    public void Loaded_values_are_not_clamped()
    {
        _editor.Load(new object?[] { 400L, 0L, 0L, 0L });

        Assert.That(_editor.Text, Is.EqualTo("400d 00h 00m 00s"));
    }

    [Test]
    public void Values_not_answering_the_format_are_ignored()
    {
        _editor.Load(new object?[] { 1L, 2L });

        Assert.That(_editor.Text, Is.EqualTo("---d --h --m --s"));
    }

    [Test]
    public void Clicking_a_group_selects_it_whole()
    {
        _editor.Load(new object?[] { 1L, 2L, 3L, 4L });
        _editor.Select(Minutes + 1, 0);

        Assert.That(_editor.SelectedGroupIndex, Is.EqualTo(2));
        Assert.That(_editor.SelectionStart, Is.EqualTo(Minutes));
        Assert.That(_editor.SelectionLength, Is.EqualTo(2));
    }

    [Test]
    public void Clicking_between_groups_selects_none()
    {
        _editor.Select(Days + 4, 0);

        Assert.That(_editor.SelectedGroupIndex, Is.EqualTo(-1));
    }

    [Test]
    public void A_full_group_moves_on_to_the_next()
    {
        _editor.Select(Days, 0);
        Type("123");

        Assert.That(_editor.Text, Is.EqualTo("123d --h --m --s"));
        Assert.That(_editor.SelectionStart, Is.EqualTo(Hours));
        Assert.That(_editor.SelectionLength, Is.EqualTo(2));
    }

    [Test]
    public void Typing_past_the_max_clamps_and_moves_on()
    {
        _editor.Select(Hours, 0);
        Type("59");

        Assert.That(_editor.Text, Is.EqualTo("---d 23h --m --s"));
        Assert.That(_editor.SelectionStart, Is.EqualTo(Minutes));
    }

    [Test]
    public void Typing_every_group()
    {
        _editor.Select(Days, 0);
        Type("100");
        Type("2");
        _editor.MoveToGroup(1);
        Type("30");
        Type("45");

        Assert.That(_editor.Text, Is.EqualTo("100d 02h 30m 45s"));
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { 100L, 2L, 30L, 45L }));
    }

    [Test]
    public void Typing_into_a_full_group_starts_it_over()
    {
        _editor.Load(new object?[] { 1L, 2L, 3L, 45L });
        _editor.Select(Seconds, 0);
        Type("6");

        Assert.That(_editor.Text, Is.EqualTo("001d 02h 03m 06s"));
    }

    [Test]
    public void Clearing_one_group_keeps_the_others()
    {
        _editor.Load(new object?[] { 1L, 2L, 3L, 4L });
        _editor.Select(Hours, 0);
        _editor.Delete(backwards: true);

        Assert.That(_editor.Text, Is.EqualTo("001d --h 03m 04s"));
        Assert.That(_editor.GetValues(), Is.EqualTo(new object?[] { 1L, null, 3L, 4L }));
    }

    [Test]
    public void Arrows_select_the_neighbouring_group_whole()
    {
        _editor.Load(new object?[] { 1L, 2L, 3L, 4L });
        _editor.Select(Days, 0);

        Assert.That(_editor.MoveCaret(+1), Is.True);
        Assert.That(_editor.SelectionStart, Is.EqualTo(Hours));
        Assert.That(_editor.SelectionLength, Is.EqualTo(2));

        _editor.MoveCaret(-1);

        Assert.That(_editor.SelectionStart, Is.EqualTo(Days));
        Assert.That(_editor.SelectionLength, Is.EqualTo(3));
    }

    [Test]
    public void There_is_no_group_past_the_last()
    {
        _editor.Select(Seconds, 0);

        Assert.That(_editor.MoveToGroup(1), Is.False);
        Assert.That(_editor.SelectedGroupIndex, Is.EqualTo(3));
    }

    [Test]
    public void Up_counts_the_selected_group()
    {
        _editor.Load(new object?[] { 1L, 2L, 3L, 4L });
        _editor.Select(Minutes, 0);
        _editor.Spin(1);

        Assert.That(_editor.Text, Is.EqualTo("001d 02h 04m 04s"));
        Assert.That(_editor.SelectionStart, Is.EqualTo(Minutes));
        Assert.That(_editor.SelectionLength, Is.EqualTo(2));
    }

    [Test]
    public void Up_stops_at_the_max()
    {
        _editor.Load(new object?[] { 0L, 23L, 0L, 0L });
        _editor.Select(Hours, 0);
        _editor.Spin(1);

        Assert.That(_editor.Text, Is.EqualTo("000d 23h 00m 00s"));
    }
}
