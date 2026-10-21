using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using Joufflu.FileExplorer.Controls.Base;
using Joufflu.FileExplorer.Nodes;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Shows where the source has navigated : a breadcrumb of the path leading to the opened directory, each of its
    /// directories reopened by a click, and a button going up to the parent of the opened one.
    /// </summary>
    /// <remarks>
    /// Everything comes from the <see cref="ExplorerControl.Source"/>, so a bar and any other control sharing that
    /// source stay in sync whichever one navigates.
    /// </remarks>
    [TemplatePart(Name = PartBreadcrumb, Type = typeof(ScrollViewer))]
    public partial class ExplorerControlBar : ExplorerControl
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

        /// <summary>Opens a directory of the breadcrumb, or the parent of the opened one.</summary>
        [RelayCommand]
        private async Task Open(IExplorerNode? directory)
        {
            if (Source != null && directory != null)
                await Source.Open(directory);
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
