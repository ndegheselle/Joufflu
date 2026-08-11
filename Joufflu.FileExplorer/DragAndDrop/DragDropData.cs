using System.Windows;

namespace Joufflu.FileExplorer.DragAndDrop;

/// <summary>
/// Reads and writes the data carried by a drag and drop operation.
/// </summary>
public static class DragDropData
{
    /// <summary>
    /// Format under which the dragged object is stored, so a single lookup finds it whatever its type.
    /// </summary>
    internal const string PayloadFormat = "Joufflu.DragAndDrop.Payload";

    /// <summary>
    /// Packs <paramref name="data"/> in a <see cref="DataObject"/> ready to be passed to
    /// <see cref="DragDrop.DoDragDrop"/>.
    /// </summary>
    /// <remarks>
    /// The payload is never serialized, so it only crosses <see cref="DragDrop.DoDragDrop"/> inside the current
    /// process : dragging out of the application carries nothing. Reading data coming from the outside still works,
    /// see <see cref="GetFilePaths"/>.
    /// </remarks>
    internal static DataObject Pack(object data) => new DataObject(PayloadFormat, data);

    /// <summary>
    /// Gets the dragged object if it is a <typeparamref name="TData"/>.
    /// </summary>
    /// <remarks>
    /// Only the <see cref="PayloadFormat"/> is looked up, never the first available format : data coming from the
    /// Windows explorer exposes many formats and the first one is not the dropped object.
    /// </remarks>
    public static TData? GetData<TData>(IDataObject? dataObject) where TData : class
        => dataObject?.GetDataPresent(PayloadFormat) == true
            ? dataObject.GetData(PayloadFormat) as TData
            : null;

    /// <summary>
    /// Gets the paths of the files dropped from outside of the application (the Windows explorer for example).
    /// </summary>
    public static IReadOnlyList<string> GetFilePaths(IDataObject? dataObject)
        => dataObject?.GetDataPresent(DataFormats.FileDrop) == true
            ? dataObject.GetData(DataFormats.FileDrop) as string[] ?? []
            : [];
}
