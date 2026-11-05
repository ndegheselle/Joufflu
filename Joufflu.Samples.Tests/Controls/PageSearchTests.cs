using Joufflu.Navigation;
using Joufflu.Samples.Controls;

namespace Joufflu.Samples.Tests.Controls;

/// <summary>How the page search finds and orders the gallery pages, and hands the one picked back.</summary>
public class PageSearchTests
{
    private static readonly PageEntry ListBox = new(typeof(int), "Native controls / List box", "Lists with single or multiple selection.");
    private static readonly PageEntry ExplorerList = new(typeof(long), "File explorer / List", "The content of a folder as a list.");
    private static readonly PageEntry Playlist = new(typeof(short), "Playlist", "Tracks to play.");
    private static readonly PageEntry Spinner = new(typeof(byte), "Feedback / Spinner", "An indeterminate loading indicator.");

    private static IReadOnlyList<string> Titles(IEnumerable<PageEntry> pages) => pages.Select(page => page.Title).ToList();

    [Test]
    public void An_empty_query_lists_every_page_in_order()
    {
        IReadOnlyList<PageEntry> results = PageSearchViewModel.Find([Spinner, ListBox], "  ");

        Assert.That(Titles(results), Is.EqualTo(new[] { Spinner.Title, ListBox.Title }));
    }

    [Test]
    public void The_search_opens_on_every_page()
    {
        var search = new PageSearchViewModel([ListBox, Spinner], new Overlayer());

        Assert.That(Titles(search.Results), Is.EqualTo(new[] { ListBox.Title, Spinner.Title }));
    }

    [Test]
    public async Task Opening_a_page_closes_the_overlay_with_that_page()
    {
        var overlays = new Overlayer();
        var search = new PageSearchViewModel([ListBox, Spinner], overlays);
        Task<PageEntry?> shown = overlays.ShowAsync(search);

        search.Open(Spinner);
        PageEntry? picked = await shown;

        Assert.Multiple(() =>
        {
            Assert.That(picked, Is.EqualTo(Spinner));
            Assert.That(overlays.HasOverlays, Is.False, "closed by the time the caller navigates");
        });
    }

    [Test]
    public void The_query_is_matched_ignoring_case()
    {
        IReadOnlyList<PageEntry> results = PageSearchViewModel.Find([ListBox, Spinner], "SPIN");

        Assert.That(Titles(results), Is.EqualTo(new[] { Spinner.Title }));
    }

    [Test]
    public void The_menu_group_in_the_title_is_searched()
    {
        IReadOnlyList<PageEntry> results = PageSearchViewModel.Find([ListBox, ExplorerList, Spinner], "file explorer");

        Assert.That(Titles(results), Is.EqualTo(new[] { ExplorerList.Title }));
    }

    [Test]
    public void A_page_matching_only_by_its_summary_is_found()
    {
        IReadOnlyList<PageEntry> results = PageSearchViewModel.Find([ListBox, Spinner], "loading");

        Assert.That(Titles(results), Is.EqualTo(new[] { Spinner.Title }));
    }

    [Test]
    public void A_title_word_starting_with_the_query_comes_before_the_query_inside_a_word()
    {
        IReadOnlyList<PageEntry> results = PageSearchViewModel.Find([Playlist, ListBox], "list");

        Assert.That(Titles(results), Is.EqualTo(new[] { ListBox.Title, Playlist.Title }));
    }

    [Test]
    public void A_title_match_comes_before_a_summary_match()
    {
        var folders = new PageEntry(typeof(char), "Folders", "Directories on the disk.");

        // "folder" is only in the summary of the explorer list.
        IReadOnlyList<PageEntry> results = PageSearchViewModel.Find([ExplorerList, folders], "folder");

        Assert.That(Titles(results), Is.EqualTo(new[] { folders.Title, ExplorerList.Title }));
    }

    [Test]
    public void Equally_good_matches_keep_the_given_order()
    {
        IReadOnlyList<PageEntry> results = PageSearchViewModel.Find([ExplorerList, ListBox], "list");

        Assert.That(Titles(results), Is.EqualTo(new[] { ExplorerList.Title, ListBox.Title }));
    }
}
