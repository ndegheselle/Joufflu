using System.Windows;

namespace Joufflu.FileExplorer.DragAndDrop;

/// <summary>
/// Wraps the dragged object so that it always travels under a single, known clipboard format.
/// </summary>
/// <remarks>
/// The payload is never serialized, so it only crosses <see cref="DragDrop.DoDragDrop"/> inside the current process :
/// dragging out of the application carries nothing. Reading data coming from the outside still works, see
/// <see cref="DragDropData.TryGetFilePaths"/>.
/// </remarks>
internal sealed class DragPayload
{
    public object Data { get; }

    public DragPayload(object data) { Data = data; }
}

/// <summary>
/// Reads and writes the data carried by a drag and drop operation.
/// </summary>
public static class DragDropData
{
    /// <summary>
    /// Format under which <see cref="DragPayload"/> is stored, so a single lookup finds the dragged object whatever
    /// its type.
    /// </summary>
    internal const string PayloadFormat = "Joufflu.DragAndDrop.Payload";

    /// <summary>
    /// Packs <paramref name="data"/> in a <see cref="DataObject"/> ready to be passed to
    /// <see cref="DragDrop.DoDragDrop"/>.
    /// </summary>
    internal static DataObject Pack(object data) => new DataObject(PayloadFormat, new DragPayload(data));

    /// <summary>
    /// Gets the dragged object if it is a <typeparamref name="TData"/>.
    /// </summary>
    /// <remarks>
    /// Only the <see cref="PayloadFormat"/> is looked up, never the first available format : data coming from the
    /// Windows explorer exposes many formats and the first one is not the dropped object.
    /// </remarks>
    public static bool TryGetData<TData>(IDataObject? dataObject, out TData? data) where TData : class
    {
        data = GetData<TData>(dataObject);
        return data != null;
    }

    /// <inheritdoc cref="TryGetData{TData}(IDataObject?, out TData?)"/>
    public static TData? GetData<TData>(IDataObject? dataObject) where TData : class
    {
        if (dataObject?.GetDataPresent(PayloadFormat) != true)
            return null;

        return (dataObject.GetData(PayloadFormat) as DragPayload)?.Data as TData;
    }

    /// <summary>
    /// Gets the paths of the files dropped from outside of the application (the Windows explorer for example).
    /// </summary>
    public static bool TryGetFilePaths(IDataObject? dataObject, out IReadOnlyList<string> paths)
    {
        paths = GetFilePaths(dataObject);
        return paths.Count > 0;
    }

    /// <inheritdoc cref="TryGetFilePaths"/>
    public static IReadOnlyList<string> GetFilePaths(IDataObject? dataObject)
    {
        if (dataObject?.GetDataPresent(DataFormats.FileDrop) != true)
            return [];

        return dataObject.GetData(DataFormats.FileDrop) as string[] ?? [];
    }
}
