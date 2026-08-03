using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Resource key of a context menu associated with a node type, works like <see cref="DataTemplateKey"/> :
    /// <c>&lt;ContextMenu x:Key="{local:ExplorerMenuKey local:PhysicalFile}"&gt;</c>.
    /// </summary>
    [MarkupExtensionReturnType(typeof(ExplorerMenuKey))]
    public class ExplorerMenuKey : ResourceKey
    {
        public Type? DataType { get; set; }

        public ExplorerMenuKey() { }

        public ExplorerMenuKey(Type? dataType) { DataType = dataType; }

        public override Assembly Assembly => DataType?.Assembly ?? typeof(ExplorerMenuKey).Assembly;

        public override bool Equals(object? obj) => obj is ExplorerMenuKey key && key.DataType == DataType;

        public override int GetHashCode() => DataType?.GetHashCode() ?? 0;

        public override string ToString() => $"{nameof(ExplorerMenuKey)}({DataType?.Name})";
    }
}
