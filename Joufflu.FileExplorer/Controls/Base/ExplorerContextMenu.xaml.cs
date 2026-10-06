using System.Windows;
﻿using System.Windows.Input;
using System.Windows.Markup;
using Joufflu.FileExplorer.Nodes;

namespace Joufflu.FileExplorer.Controls.Base
{
    public enum MenuScope { Single, Multiple, None }

    /// <summary>
    /// Resource key of the context menu template of a data type.
    /// Inherits <see cref="ComponentResourceKey"/> because the templates live in the theme dictionary of the library
    /// (Themes/Generic.xaml) : a resource lookup only reaches a theme dictionary for a Type or a ComponentResourceKey
    /// key, any other key would only be found by a consumer merging the dictionaries into its own resources.
    /// </summary>
    public sealed class ContextMenuTemplateKey : ComponentResourceKey, IEquatable<ContextMenuTemplateKey>
    {
        /// <summary>Type name declared in XAML, resolved in <see cref="ProvideValue"/>.</summary>
        private readonly string? _typeName;

        public ContextMenuTemplateKey()
        {
            // Points the lookup at the theme dictionary of this assembly.
            TypeInTargetAssembly = typeof(ContextMenuTemplateKey);
        }

        public ContextMenuTemplateKey(Type dataType) : this() { DataType = dataType; }

        public ContextMenuTemplateKey(string typeName) : this() { _typeName = typeName; }

        public Type? DataType { get; private set; }

        public MenuScope Scope { get; set; } = MenuScope.Single;

        public override object ProvideValue(IServiceProvider sp)
        {
            // Resolves the type name declared in XAML
            if (DataType == null && _typeName != null)
                DataType = (sp.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver)?.Resolve(_typeName);

            return this;
        }

        public bool Equals(ContextMenuTemplateKey? o) => o is not null && o.DataType == DataType && o.Scope == Scope;
        public override bool Equals(object? o) => Equals(o as ContextMenuTemplateKey);
        public override int GetHashCode() => HashCode.Combine(DataType, Scope);
    }

    /// <summary>
    /// Data context of the context menus of an <see cref="ExplorerNodesControl"/>, gives access to the commands of
    /// the source and to the nodes the menu was opened on.
    /// </summary>
    public class ExplorerMenuContext
    {
        public IExplorerSource? Source { get; }

        /// <summary>
        /// Starts renaming the node given as a parameter in the control the menu was opened in : the name is typed
        /// where the node is displayed, see <see cref="ExplorerNodesControl.BeginRenameCommand"/>.
        /// </summary>
        public ICommand BeginRenameCommand { get; }

        /// <summary>
        /// Every selected node, the menu was opened on the first one.
        /// </summary>
        public IReadOnlyList<IExplorerNode> Nodes { get; }

        /// <summary>
        /// Node the menu was opened on, null when multiple nodes are selected.
        /// </summary>
        public IExplorerNode? Node => Nodes.Count == 1 ? Nodes[0] : null;

        public ExplorerMenuContext(IExplorerSource? source, ICommand beginRenameCommand, IReadOnlyList<IExplorerNode> nodes)
        {
            BeginRenameCommand = beginRenameCommand;
            Source = source;
            Nodes = nodes;
        }
    }
}
