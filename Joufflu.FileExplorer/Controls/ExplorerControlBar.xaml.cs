using System.Windows;
using System.Windows.Controls;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Drives the navigation of a loader : going back and forward through the visited directories, going up to the
    /// parent of the opened one and a breadcrumb of the path leading to it, each of its directories reopened by a
    /// click.
    /// </summary>
    /// <remarks>
    /// Everything comes from the navigation commands of the loader, so a bar and any other control sharing that loader
    /// stay in sync whichever one navigates.
    /// </remarks>
    [TemplatePart(Name = PartBreadcrumb, Type = typeof(ScrollViewer))]
    public class ExplorerControlBar : ExplorerControl
    {
        private const string PartBreadcrumb = "PART_Breadcrumb";

        private ScrollViewer? _breadcrumb;

        static ExplorerControlBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExplorerControlBar),
                new FrameworkPropertyMetadata(typeof(ExplorerControlBar)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_breadcrumb != null)
                _breadcrumb.ScrollChanged -= OnBreadcrumbScrollChanged;

            _breadcrumb = GetTemplateChild(PartBreadcrumb) as ScrollViewer;

            if (_breadcrumb != null)
                _breadcrumb.ScrollChanged += OnBreadcrumbScrollChanged;
        }

        /// <summary>
        /// Keeps the opened directory, at the end of the breadcrumb, in sight : a path too long to be shown entirely
        /// would otherwise be scrolled on its first directories.
        /// </summary>
        private static void OnBreadcrumbScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentWidthChange != 0)
                ((ScrollViewer)sender).ScrollToRightEnd();
        }
    }
}
