using System.Windows;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Controls.Base;

/// <summary>
/// Arguments of <see cref="ExplorerNodesControl.NodeActivated"/> : the node the user opened.
/// </summary>
public class ExplorerNodeEventArgs : RoutedEventArgs
{
    public ExplorerNodeEventArgs(RoutedEvent routedEvent, IExplorerNode node) : base(routedEvent)
    {
        Node = node;
    }

    public IExplorerNode Node { get; }
}
