using PagingControl = Joufflu.Navigation.Controls.Paging;

namespace Joufflu.Navigation.Tests.Paging;

/// <summary>
/// What only the control does : coercing its page and raising <see cref="PagingControl.PagingChange"/>. WPF
/// controls are created and driven from a single-threaded apartment only.
/// </summary>
[Apartment(ApartmentState.STA)]
public class PagingTests
{
    [Test]
    public void A_larger_capacity_brings_the_page_back_within_the_total()
    {
        var paging = new PagingControl { Total = 100, PageNumber = 10 };

        paging.Capacity = 50;

        Assert.That(paging.PageNumber, Is.EqualTo(2));
    }

    [Test]
    public void A_capacity_change_moving_the_page_raises_PagingChange_once()
    {
        var paging = new PagingControl { Total = 100, PageNumber = 10 };
        var changes = new List<(int Page, int Capacity)>();
        paging.PagingChange += (page, capacity) => changes.Add((page, capacity));

        paging.Capacity = 50;

        Assert.That(changes, Is.EqualTo(new[] { (2, 50) }));
    }

    [Test]
    public void A_capacity_change_keeping_the_page_raises_PagingChange_once()
    {
        var paging = new PagingControl { Total = 100, PageNumber = 1 };
        var changes = new List<(int Page, int Capacity)>();
        paging.PagingChange += (page, capacity) => changes.Add((page, capacity));

        paging.Capacity = 25;

        Assert.That(changes, Is.EqualTo(new[] { (1, 25) }));
    }

    [Test]
    public void Next_is_unavailable_on_the_last_page()
    {
        var paging = new PagingControl { Total = 30, PageNumber = 3 };

        Assert.That(paging.NextCommand.CanExecute(null), Is.False);
    }
}
