using Joufflu.FileExplorer.Sources;
using System.Windows;
using System.Windows.Controls;

namespace Joufflu.FileExplorer.Controls.Base;

/// <summary>
/// Base of the explorer controls : the source whose content they display and through which they navigate. Several
/// controls share the same source, so they all show the same opened directory.
/// </summary>
public abstract class ExplorerControl : Control
{
    #region Dependency Properties

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source),
        typeof(IExplorerSource),
        typeof(ExplorerControl),
        new PropertyMetadata(
            null,
            (d, e) => ((ExplorerControl)d).OnSourceChanged(
                e.OldValue as IExplorerSource,
                e.NewValue as IExplorerSource)));

    #endregion

    /// <summary>
    /// Source of the explorer
    /// </summary>
    public IExplorerSource Source
    {
        get => (IExplorerSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// The control is given another source ; a derived control overrides it to track the directory it opens.
    /// </summary>
    protected virtual void OnSourceChanged(IExplorerSource? previous, IExplorerSource? source) { }
}
