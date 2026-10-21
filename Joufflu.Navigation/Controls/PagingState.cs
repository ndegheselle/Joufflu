namespace Joufflu.Navigation.Controls
{
    /// <summary>
    /// What a <see cref="Paging"/> shows for a total, a page size and a current page. Free of WPF so the paging
    /// rules are tested without a control.
    /// </summary>
    internal sealed record PagingState(int Total, int Capacity, int PageNumber)
    {
        // Past this many pages the buttons can't show them all : they show the first, the last and the ones around
        // the current page, gaps standing for the others.
        private const int MaxPageButtons = 7;

        /// <summary>
        /// A negative <see cref="Total"/> leaves the size of the set unknown, while 0 is an empty set.
        /// </summary>
        public bool IsTotalKnown => Total >= 0;

        public int PageMax
        {
            get
            {
                // An unknown total has no last page : one past the current one keeps Next available.
                if (!IsTotalKnown)
                    return PageNumber + 1;
                int max = (int)Math.Ceiling(Total / (double)Capacity);
                return Math.Max(1, max);
            }
        }

        // An empty set shows "0-0 of 0" rather than starting at its first item.
        public int IntervalMin => Total == 0 ? 0 : Capacity * (PageNumber - 1) + 1;

        public int IntervalMax => Math.Min(IntervalMin + Capacity - 1, Total);

        public bool CanGoBack => PageNumber > 1;

        public bool CanGoForward => PageNumber < PageMax;

        /// <summary>
        /// [page] brought back between the first and the last page, the last one being unbounded while the total is
        /// unknown.
        /// </summary>
        public int Clamp(int page)
        {
            if (!IsTotalKnown)
                return Math.Max(1, page);
            return Math.Clamp(page, 1, PageMax);
        }

        /// <summary>
        /// The page numbers to show as buttons, in order, <c>null</c> standing for a gap.
        /// </summary>
        public IReadOnlyList<int?> VisiblePages()
        {
            if (PageMax <= MaxPageButtons)
                return Enumerable.Range(1, PageMax).Select(page => (int?)page).ToList();

            // The three pages around the current one, kept clear of the first and the last two.
            int middle = Math.Clamp(PageNumber, 4, PageMax - 3);
            int? second = PageNumber > 3 ? null : 2;
            int? beforeLast = PageNumber < PageMax - 3 ? null : PageMax - 1;
            return [1, second, middle - 1, middle, middle + 1, beforeLast, PageMax];
        }
    }
}
