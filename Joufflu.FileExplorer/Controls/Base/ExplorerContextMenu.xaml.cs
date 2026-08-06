using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;
using System.Windows.Markup;

namespace Joufflu.FileExplorer.Controls.Base
{
    /// <summary>
    /// What a context menu was opened on, which decides the menu shown.
    /// </summary>
    public enum MenuScope
    {
        /// <summary>A single node.</summary>
        Single,
        /// <summary>Several selected nodes.</summary>
        Multiple,
        /// <summary>
        /// The empty space of the control, the menu then acting on the opened directory : where "New folder" and
        /// "Paste" belong, neither of them having a node to act on.
        /// </summary>
        Background
    }

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
    /// Data context of the context menus of the explorer controls : the session and its commands, the nodes the menu
    /// was opened on, and the control itself for the commands it owns.
    /// </summary>
    public class ExplorerMenuContext
    {
        public ExplorerMenuContext(
            ExplorerNodesControl owner,
            IReadOnlyList<IExplorerNode> nodes,
            IExplorerDirectory? directory)
        {
            Owner = owner;
            Nodes = nodes;
            Directory = directory;
        }

        /// <summary>
        /// Control the menu was opened on.
        /// </summary>
        /// <remarks>
        /// A context menu is not in the visual tree of the control that owns it, so a RelativeSource cannot reach it :
        /// the commands needing the interface rather than the session, renaming and creating a folder, are bound
        /// through here.
        /// </remarks>
        public ExplorerNodesControl Owner { get; }

        public ExplorerSession? Session => Owner.Session;

        /// <summary>
        /// Every selected node, the menu was opened on the first one. Empty for a
        /// <see cref="MenuScope.Background"/> menu.
        /// </summary>
        public IReadOnlyList<IExplorerNode> Nodes { get; }

        /// <summary>
        /// Node the menu was opened on, null when several nodes are selected or when the menu was opened on the empty
        /// space of the control.
        /// </summary>
        public IExplorerNode? Node => Nodes.Count == 1 ? Nodes[0] : null;

        /// <summary>
        /// Directory the menu acts in : the clicked node when it is one, its parent otherwise, and the opened
        /// directory when the click was on the empty space. Target of "New folder" and "Paste".
        /// </summary>
        public IExplorerDirectory? Directory { get; }
    }
}
