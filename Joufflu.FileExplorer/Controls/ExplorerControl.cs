using System.Windows;
using System.Windows.Controls;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Base of the explorer controls : the loader whose content they display and through which they navigate. Several
    /// controls share the same loader, so they all show the same opened directory.
    /// </summary>
    public abstract class ExplorerControl : Control
    {
        #region Dependency Property
        public static readonly DependencyProperty LoaderProperty = DependencyProperty.Register(
            nameof(Loader),
            typeof(IExplorerLoader),
            typeof(ExplorerControl),
            new PropertyMetadata(null));
        #endregion

        public IExplorerLoader? Loader
        {
            get => (IExplorerLoader?)GetValue(LoaderProperty);
            set => SetValue(LoaderProperty, value);
        }
    }
}
