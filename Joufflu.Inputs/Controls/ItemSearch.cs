using System.Collections;
using System.Reflection;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// How <see cref="ComboBoxSearch"/> reads its items as text and matches them against what the user typed. Free
    /// of WPF so the search rules are tested without a control.
    /// </summary>
    internal static class ItemSearch
    {
        /// <summary>
        /// The text [item] shows : the value at [displayMemberPath], dotted paths such as <c>Address.City</c>
        /// included, or the item itself when there is no path.
        /// </summary>
        public static string TextOf(object? item, string? displayMemberPath)
        {
            if (item is null)
                return string.Empty;
            if (string.IsNullOrEmpty(displayMemberPath))
                return item.ToString() ?? string.Empty;

            // DisplayMemberPath is a string by WPF's design, so it can only be followed by reflection.
            object? value = item;
            foreach (string propertyName in displayMemberPath.Split('.'))
            {
                if (value is null)
                    return string.Empty;
                PropertyInfo? property = value.GetType().GetProperty(propertyName);
                if (property is null)
                    return item.ToString() ?? string.Empty;
                value = property.GetValue(value);
            }
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Whether [item] stays in the list while the user searches for [search].
        /// </summary>
        public static bool Passes(object? item, string? search, object? selectedItem, string? displayMemberPath)
        {
            if (item is null)
                return false;
            if (string.IsNullOrEmpty(search))
                return true;
            // Text left by a selection is not something the user searched for : the list stays whole so that the
            // other choices remain reachable once an item has been picked.
            if (selectedItem is not null && search == TextOf(selectedItem, displayMemberPath))
                return true;

            string itemText = TextOf(item, displayMemberPath);
            return itemText.Contains(search, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// The index of the first item whose text is exactly [text], or -1.
        /// </summary>
        public static int IndexOfExactMatch(IEnumerable items, string? text, string? displayMemberPath)
        {
            if (string.IsNullOrEmpty(text))
                return -1;

            int index = 0;
            foreach (object? item in items)
            {
                if (text == TextOf(item, displayMemberPath))
                    return index;
                index++;
            }
            return -1;
        }
    }
}
