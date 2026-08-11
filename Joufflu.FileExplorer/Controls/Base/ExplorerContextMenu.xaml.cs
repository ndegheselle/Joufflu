using System.Windows.Markup;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.FileExplorer.Controls.Base
{
    public enum MenuScope { Single, Multiple, None }

    /// <summary>
    /// Resource key of the context menu template of a data type.
    /// Inherits <see cref="TypeExtension"/> because the XAML compiler only accepts String, TypeExtension and
    /// StaticExtension as x:Key markup extensions.
    /// </summary>
    public sealed class ContextMenuTemplateKey : TypeExtension, IEquatable<ContextMenuTemplateKey>
    {
        public ContextMenuTemplateKey() { }

        public ContextMenuTemplateKey(Type dataType) : base(dataType) { }

        public ContextMenuTemplateKey(string typeName) : base(typeName) { }

        public Type DataType => Type;

        public MenuScope Scope { get; set; } = MenuScope.Single;

        public override object ProvideValue(IServiceProvider sp)
        {
            // Resolves the type name declared in XAML
            Type ??= (Type)base.ProvideValue(sp);
            return this;
        }

        public bool Equals(ContextMenuTemplateKey? o) => o is not null && o.Type == Type && o.Scope == Scope;
        public override bool Equals(object? o) => Equals(o as ContextMenuTemplateKey);
        public override int GetHashCode() => HashCode.Combine(Type, Scope);
    }

    /// <summary>
    /// Data context of the context menus of an <see cref="ExplorerList"/>, gives access to the commands of the
    /// loader and to the nodes the menu was opened on.
    /// </summary>
    public class ExplorerMenuContext
    {
        public IExplorerSource? Source { get; }

        /// <summary>
        /// Every selected node, the menu was opened on the first one.
        /// </summary>
        public IReadOnlyList<IExplorerNode> Nodes { get; }

        /// <summary>
        /// Node the menu was opened on, null when multiple nodes are selected.
        /// </summary>
        public IExplorerNode? Node => Nodes.Count == 1 ? Nodes[0] : null;

        public ExplorerMenuContext(IExplorerSource? source, IReadOnlyList<IExplorerNode> nodes)
        {
            Source = source;
            Nodes = nodes;
        }
    }
}
