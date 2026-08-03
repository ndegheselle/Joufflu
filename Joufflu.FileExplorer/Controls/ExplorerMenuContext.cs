using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Data context of the context menus of an <see cref="ExplorerList"/>, gives access to the commands of the
    /// loader and to the nodes the menu was opened on.
    /// </summary>
    public class ExplorerMenuContext
    {
        public IExplorerLoader? Loader { get; }

        /// <summary>
        /// Every selected node, the menu was opened on the first one.
        /// </summary>
        public IReadOnlyList<IExplorerNode> Nodes { get; }

        /// <summary>
        /// Node the menu was opened on, null when multiple nodes are selected.
        /// </summary>
        public IExplorerNode? Node => Nodes.Count == 1 ? Nodes[0] : null;

        public ExplorerMenuContext(IExplorerLoader? loader, IReadOnlyList<IExplorerNode> nodes)
        {
            Loader = loader;
            Nodes = nodes;
        }
    }
}
