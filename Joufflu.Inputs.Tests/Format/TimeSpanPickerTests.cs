using System.Windows.Input;
using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Format;

/// <summary>
/// Groups are days / hours / minutes / seconds, rendered "000d 00h 00m 00s":
/// days start at 0, hours at 5, minutes at 9, seconds at 13.
/// </summary>
[Apartment(ApartmentState.STA)]
public class TimeSpanPickerTests
{
    private const int Days = 0;
    private const int Hours = 5;
    private const int Minutes = 9;

    private FormatInputHost<TimeSpanPicker> _host = null!;
    private TimeSpanPicker Box => _host.Box;

    [SetUp]
    public void SetUp() => _host = new FormatInputHost<TimeSpanPicker>(new TimeSpanPicker());

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void A_value_set_from_outside_fills_each_group()
    {
        Box.Value = new TimeSpan(1, 2, 3, 4);

        Assert.That(Box.Text, Is.EqualTo("001d 02h 03m 04s"));
        Assert.That(Box.Values, Is.EqualTo(new object?[] { 1L, 2L, 3L, 4L }));
    }

    [Test]
    public void A_value_set_from_outside_is_not_clamped()
    {
        Box.Value = new TimeSpan(400, 0, 0, 0);

        Assert.That(Box.Text, Is.EqualTo("400d 00h 00m 00s"));
        Assert.That(Box.Value, Is.EqualTo(new TimeSpan(400, 0, 0, 0)));
    }

    [Test]
    public void Clicking_a_group_selects_it_whole()
    {
        Box.Value = new TimeSpan(1, 2, 3, 4);
        _host.Click(Minutes + 1);

        Assert.That(Box.SelectionStart, Is.EqualTo(Minutes));
        Assert.That(Box.SelectionLength, Is.EqualTo(2));
    }

    [Test]
    public void Typing_every_group_gives_the_time_span()
    {
        _host.Click(Days);
        _host.Type("100");
        _host.Type("2");
        _host.Press(Key.Tab);
        _host.Type("30");
        _host.Type("45");

        Assert.That(Box.Value, Is.EqualTo(new TimeSpan(100, 2, 30, 45)));
        Assert.That(Box.Text, Is.EqualTo("100d 02h 30m 45s"));
    }

    [Test]
    public void Clearing_one_group_keeps_the_others()
    {
        Box.Value = new TimeSpan(1, 2, 3, 4);
        _host.Click(Hours);
        _host.Press(Key.Back);

        Assert.That(Box.Value, Is.Null);
        Assert.That(Box.Text, Is.EqualTo("001d --h 03m 04s"));
        Assert.That(Box.Values, Is.EqualTo(new object?[] { 1L, null, 3L, 4L }));
    }

    [Test]
    public void Arrows_select_the_neighbouring_group_whole()
    {
        Box.Value = new TimeSpan(1, 2, 3, 4);
        _host.Click(Days);
        _host.Press(Key.Right);

        Assert.That(Box.SelectionStart, Is.EqualTo(Hours));
        Assert.That(Box.SelectionLength, Is.EqualTo(2));

        _host.Press(Key.Left);

        Assert.That(Box.SelectionStart, Is.EqualTo(Days));
        Assert.That(Box.SelectionLength, Is.EqualTo(3));
    }
}
