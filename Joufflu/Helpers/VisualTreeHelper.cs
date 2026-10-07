using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Joufflu.Helpers
{
    public static class MoreVisualTreeHelper
    {
        /// <summary>
        /// Parent of <paramref name="element"/> in the visual tree, falling back to the logical one for the elements
        /// that aren't part of it (the inline content of a text, ...).
        /// </summary>
        public static DependencyObject? GetParent(DependencyObject element)
            => element is Visual or Visual3D
                ? VisualTreeHelper.GetParent(element)
                : LogicalTreeHelper.GetParent(element);

        /// <summary>
        /// First element of <paramref name="type"/> at or above <paramref name="origin"/>, null when there is none.
        /// </summary>
        public static DependencyObject? FindSelfOrParent(DependencyObject? origin, Type type)
        {
            for (DependencyObject? current = origin; current != null; current = GetParent(current))
            {
                if (type.IsInstanceOfType(current))
                    return current;
            }

            return null;
        }

        public static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
        {
            if (child == null) return null;
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T? parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }

        /// <summary>
        /// The visual children of <paramref name="element"/>, and theirs too, depth first, when
        /// <paramref name="recursive"/>.
        /// </summary>
        public static IEnumerable<DependencyObject> GetChildren(DependencyObject element, bool recursive)
        {
            if (element == null)
                yield break;

            // Counted at each step: the caller may change the tree between two children.
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(element, i);
                if (child == null)
                    continue;

                yield return child;
                if (!recursive)
                    continue;

                foreach (DependencyObject descendant in GetChildren(child, recursive: true))
                    yield return descendant;
            }
        }

        public static IEnumerable<T> GetChildren<T>(DependencyObject element, bool recursive) where T : DependencyObject
        {
            IEnumerable<DependencyObject> children = GetChildren(element, recursive);
            return children.OfType<T>();
        }

        public static T? GetChild<T>(DependencyObject element, bool recursive) where T : DependencyObject
        {
            IEnumerable<DependencyObject> children = GetChildren(element, recursive);
            return children.OfType<T>().FirstOrDefault();
        }
    }
}
