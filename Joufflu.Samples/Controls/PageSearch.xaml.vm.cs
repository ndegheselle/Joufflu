using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Navigation;

namespace Joufflu.Samples.Controls;

/// <summary>A gallery page, as the search lists it.</summary>
/// <param name="Title">
/// The menu label, prefixed with its menu group when it has one ("File explorer / List"): a short label such as
/// "List" would otherwise be ambiguous in the results.
/// </param>
/// <param name="Summary">One sentence telling what the page shows.</param>
public record PageEntry(Type PageType, string Title, string Summary);

/// <summary>
/// The page search overlay: lists the gallery pages matching what is typed, all of them while nothing is, and closes
/// with the page picked as its result. The caller navigates once the overlay is closed.
/// </summary>
public partial class PageSearchViewModel : ObservableObject, IOverlayContent<PageEntry>
{
    private readonly IReadOnlyList<PageEntry> _pages;
    private readonly IOverlayer _overlays;

    public OverlayOptions Options { get; } = new() { Title = "Search a page" };

    /// <summary>The debounced text of the search box.</summary>
    [ObservableProperty]
    public partial string? Query { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<PageEntry> Results { get; set; }

    /// <summary>The page picked, set when the overlay is validated.</summary>
    public PageEntry? Result { get; private set; }

    public PageSearchViewModel(IReadOnlyList<PageEntry> pages, IOverlayer overlays)
    {
        _pages = pages;
        _overlays = overlays;
        Results = pages;
    }

    partial void OnQueryChanged(string? value)
    {
        Results = Find(_pages, value);
    }

    public void Open(PageEntry page)
    {
        Result = page;
        _overlays.Validate(this);
    }

    /// <summary>
    /// The pages matching [query], best matches first: a title word starting with the query, then the query anywhere
    /// in the title, then in the summary. Pages matching equally keep their order in [pages]; every page matches an
    /// empty query.
    /// </summary>
    public static IReadOnlyList<PageEntry> Find(IEnumerable<PageEntry> pages, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return pages.ToList();

        string search = query.Trim();
        // OrderBy is a stable sort, which is what keeps the given order between pages of the same rank.
        return pages
            .Select(page => (Page: page, Match: MatchOf(page, search)))
            .Where(result => result.Match != Match.None)
            .OrderBy(result => result.Match)
            .Select(result => result.Page)
            .ToList();
    }

    /// <summary>How well a page matches a search, best first.</summary>
    private enum Match
    {
        TitleWord,
        Title,
        Summary,
        None,
    }

    private static Match MatchOf(PageEntry page, string search)
    {
        if (HasWordStartingWith(page.Title, search))
            return Match.TitleWord;
        if (page.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            return Match.Title;
        if (page.Summary.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            return Match.Summary;
        return Match.None;
    }

    /// <summary>Whether [search] appears in [title] at the start of a word, "box" in "Combo box" for instance.</summary>
    private static bool HasWordStartingWith(string title, string search)
    {
        for (int i = 0; i < title.Length; i++)
        {
            bool isWordStart = i == 0 || title[i - 1] is ' ' or '/';
            if (!isWordStart)
                continue;
            if (title.AsSpan(i).StartsWith(search, StringComparison.CurrentCultureIgnoreCase))
                return true;
        }
        return false;
    }
}
