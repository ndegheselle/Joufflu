using Joufflu.Navigation.Controls;

namespace Joufflu.Navigation.Tests.Paging;

public class PagingStateTests
{
    [Test]
    public void Shows_every_page_when_there_are_seven_or_fewer()
    {
        var state = new PagingState(Total: 70, Capacity: 10, PageNumber: 1);

        Assert.That(state.VisiblePages(), Is.EqualTo(new int?[] { 1, 2, 3, 4, 5, 6, 7 }));
    }

    [Test]
    public void Puts_a_gap_on_both_sides_of_a_middle_page()
    {
        var state = new PagingState(Total: 200, Capacity: 10, PageNumber: 10);

        Assert.That(state.VisiblePages(), Is.EqualTo(new int?[] { 1, null, 9, 10, 11, null, 20 }));
    }

    [Test]
    public void Shows_the_first_pages_without_a_gap_near_the_start()
    {
        var state = new PagingState(Total: 200, Capacity: 10, PageNumber: 2);

        Assert.That(state.VisiblePages(), Is.EqualTo(new int?[] { 1, 2, 3, 4, 5, null, 20 }));
    }

    [Test]
    public void Shows_the_last_pages_without_a_gap_near_the_end()
    {
        var state = new PagingState(Total: 200, Capacity: 10, PageNumber: 19);

        Assert.That(state.VisiblePages(), Is.EqualTo(new int?[] { 1, null, 16, 17, 18, 19, 20 }));
    }

    [Test]
    public void An_empty_set_is_a_single_page_showing_zero_to_zero()
    {
        var state = new PagingState(Total: 0, Capacity: 10, PageNumber: 1);

        Assert.That(state.PageMax, Is.EqualTo(1));
        Assert.That((state.IntervalMin, state.IntervalMax), Is.EqualTo((0, 0)));
        Assert.That(state.CanGoForward, Is.False);
    }

    [Test]
    public void An_unknown_total_keeps_next_available()
    {
        var state = new PagingState(Total: -1, Capacity: 10, PageNumber: 3);

        Assert.That(state.CanGoForward, Is.True);
    }

    [Test]
    public void An_unknown_total_does_not_bound_the_page()
    {
        var state = new PagingState(Total: -1, Capacity: 10, PageNumber: 1);

        Assert.That(state.Clamp(50), Is.EqualTo(50));
    }

    [Test]
    public void Clamps_a_page_past_the_last_one_to_the_last()
    {
        var state = new PagingState(Total: 95, Capacity: 10, PageNumber: 1);

        Assert.That(state.Clamp(42), Is.EqualTo(10));
    }

    [Test]
    public void The_last_page_interval_stops_at_the_total()
    {
        var state = new PagingState(Total: 95, Capacity: 10, PageNumber: 10);

        Assert.That((state.IntervalMin, state.IntervalMax), Is.EqualTo((91, 95)));
    }
}
