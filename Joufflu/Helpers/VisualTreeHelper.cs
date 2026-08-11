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

        public static IEnumerable<DependencyObject> GetChildren(DependencyObject pElement, bool pRecursif)
        {
            if (pElement != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(pElement); i++)
                {
                    DependencyObject lChild = VisualTreeHelper.GetChild(pElement, i);
                    if (lChild != null)
                    {
                        yield return lChild;

                        if (pRecursif)
                        {
                            foreach (DependencyObject lChildOfChild in GetChildren(lChild, true))
                                yield return lChildOfChild;
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> GetChildren<T>(DependencyObject pElement, bool pRecursif) where T : DependencyObject
        {
            IEnumerable<DependencyObject> lList = GetChildren(pElement, pRecursif);
            return lList.OfType<T>();
        }

        public static T? GetChild<T>(DependencyObject pElement, bool pRecursif) where T : DependencyObject
        {
            IEnumerable<DependencyObject> lList = GetChildren(pElement, pRecursif);
            var lReturn = lList.OfType<T>().FirstOrDefault();
            return lReturn;
        }
    }
}
