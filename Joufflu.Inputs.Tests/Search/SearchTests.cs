using System.Diagnostics;
using System.Windows.Threading;

namespace Joufflu.Inputs.Tests.Search;

/// <summary>The debounced <see cref="Controls.Search.SearchText"/> and the box text staying in step.</summary>
[Apartment(ApartmentState.STA)]
public class SearchTests
{
    [Test]
    public void Setting_the_search_text_shows_it_in_the_box()
    {
        var search = new Controls.Search { SearchText = "Tata" };

        Assert.That(search.Text, Is.EqualTo("Tata"));
    }

    [Test]
    public void Setting_the_search_text_does_not_search_again()
    {
        var search = new Controls.Search();
        int searches = 0;
        search.SearchChanged += _ => searches++;

        search.SearchText = "Tata";
        PumpFor(TimeSpan.FromMilliseconds(500));

        Assert.That(searches, Is.Zero);
    }

    [Test]
    public void Typed_text_reaches_the_search_text_once_settled()
    {
        var search = new Controls.Search();

        search.Text = "Toto";
        PumpFor(TimeSpan.FromMilliseconds(500));

        Assert.That(search.SearchText, Is.EqualTo("Toto"));
    }

    /// <summary>Runs the dispatcher long enough for the debounce timer to tick.</summary>
    private static void PumpFor(TimeSpan duration)
    {
        var watch = Stopwatch.StartNew();
        while (watch.Elapsed < duration)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            Thread.Sleep(10);
        }
    }
}
