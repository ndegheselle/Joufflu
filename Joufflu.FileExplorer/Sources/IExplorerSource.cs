
using Joufflu.FileExplorer.Data;
using System.ComponentModel;

namespace Joufflu.FileExplorer.Sources;

public interface IExplorerSource : INotifyPropertyChanged
{
    public IExplorerDirectory? Root { get; }
    public IExplorerDirectory? Current { get; }

    /// <summary>
    /// Open the root element and makes it the <see cref="Current"/> one.
    /// </summary>
    public Task Open();

    /// <summary>
    /// Open a node, if it's a directory loads the children of a directory and makes it the <see cref="Current"/> one.
    /// </summary>
    public Task Open(IExplorerNode node);
}
