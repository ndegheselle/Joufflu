using System.Collections;
using System.Globalization;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Orders nodes the way a file browser does : folders first, then names compared naturally
    /// so that "file2" comes before "file10".
    /// </summary>
    public class ExplorerNodeComparer : IComparer<IExplorerNode>, IComparer
    {
        public static readonly ExplorerNodeComparer Default = new ExplorerNodeComparer();

        public int Compare(IExplorerNode? x, IExplorerNode? y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            bool isFolderX = x is IExplorerFolder;
            bool isFolderY = y is IExplorerFolder;
            if (isFolderX != isFolderY)
                return isFolderX ? -1 : 1;

            return CompareNatural(x.Name, y.Name);
        }

        /// <summary>
        /// Non generic entry point, this is the one a <see cref="System.Windows.Data.ListCollectionView"/> uses.
        /// </summary>
        int IComparer.Compare(object? x, object? y) => Compare(x as IExplorerNode, y as IExplorerNode);

        /// <summary>
        /// Compare two names chunk by chunk : digit runs are compared as numbers, the rest is
        /// compared as text so that the ordering follows what a human would expect.
        /// </summary>
        private static int CompareNatural(string left, string right)
        {
            int i = 0, j = 0;
            while (i < left.Length && j < right.Length)
            {
                int startLeft = i, startRight = j;
                if (char.IsDigit(left[i]) && char.IsDigit(right[j]))
                {
                    while (i < left.Length && char.IsDigit(left[i]))
                        i++;
                    while (j < right.Length && char.IsDigit(right[j]))
                        j++;

                    // Leading zeros carry no value : "007" and "7" are the same number.
                    ReadOnlySpan<char> numberLeft = left.AsSpan(startLeft, i - startLeft).TrimStart('0');
                    ReadOnlySpan<char> numberRight = right.AsSpan(startRight, j - startRight).TrimStart('0');

                    if (numberLeft.Length != numberRight.Length)
                        return numberLeft.Length - numberRight.Length;

                    // Same amount of digits, comparing them one by one is comparing the numbers.
                    int numbers = numberLeft.SequenceCompareTo(numberRight);
                    if (numbers != 0)
                        return numbers;
                }
                else
                {
                    while (i < left.Length && !char.IsDigit(left[i]))
                        i++;
                    while (j < right.Length && !char.IsDigit(right[j]))
                        j++;

                    int text = CultureInfo.CurrentCulture.CompareInfo.Compare(
                        left,
                        startLeft,
                        i - startLeft,
                        right,
                        startRight,
                        j - startRight,
                        CompareOptions.IgnoreCase);
                    if (text != 0)
                        return text;
                }
            }

            // One of the names is a prefix of the other, the shortest comes first.
            int remaining = (left.Length - i) - (right.Length - j);
            if (remaining != 0)
                return remaining;

            // Names only differing by their case or their leading zeros, keep a stable order.
            return string.CompareOrdinal(left, right);
        }
    }
}
