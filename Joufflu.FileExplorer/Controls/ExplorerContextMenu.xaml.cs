using Joufflu.FileExplorer.Loaders;
using System.Windows.Markup;

namespace Joufflu.FileExplorer.Controls
{
    public enum MenuScope { Single, Multiple }

    public sealed class ContextMenuTemplateKey : MarkupExtension, IEquatable<ContextMenuTemplateKey>
    {
        public ContextMenuTemplateKey(Type dataType) => DataType = dataType;

        [ConstructorArgument("dataType")]
        public Type DataType { get; set; }

        public MenuScope Scope { get; set; } = MenuScope.Single;
        public override object ProvideValue(IServiceProvider sp) => this;

        public bool Equals(ContextMenuTemplateKey? o) => o is not null && o.DataType == DataType;
        public override bool Equals(object? o) => Equals(o as ContextMenuTemplateKey);
        public override int GetHashCode() => DataType?.GetHashCode() ?? 0;
    }

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
