using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;

namespace Joufflu.Data.Model;

public enum EnumDataType
{
    String,
    Integer,
    Number,
    Boolean,
    TimeSpan,
    DateTime,
    Choice,
    Array,
    Object
}

public abstract partial class DataNode : ObservableObject, INotifyDataErrorInfo
{
    [ObservableProperty]
    private string? _key;

    /// <summary>Why [Key] is refused, null when it is fine.</summary>
    private string? _keyError;

    partial void OnKeyChanged(string? value)
    {
        if (Parent is not null)
            ValidateKeys(Parent.Children);
    }

    /// <summary>For DataNode with children and to prevent binding errors.</summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    public EnumDataType Type { get; private set; }
    /// <summary>Whether the schema takes null on top of the type it calls for.</summary>
    public bool IsNullable { get; set; }
    /// <summary>Whether the parent object requires the node: it then cannot be undefined.</summary>
    public bool IsRequired { get; set; }
    public bool CanEditKey { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsArrayItem))]
    private IDataParent? _parent;

    /// <summary>Whether the node is an array item, the only kind <see cref="Controls.DataFill"/> removes.</summary>
    public bool IsArrayItem => Parent is DataArray;
    public string? Description { get; set; }

    /// <summary> Whether the node is forced from the manual list, an object or an array then leaving its children out. </summary>
    [ObservableProperty]
    private bool _isManual;

    /// <summary>The entry picked while in manual mode.</summary>
    [ObservableProperty]
    private DataManualValue? _manualEntry;

    partial void OnIsManualChanged(bool value) => OnManualModeChanged(value);
    partial void OnManualEntryChanged(DataManualValue? value) => OnManualEntryPicked(value);

    /// <summary>Called when [IsManual] changes.</summary>
    protected virtual void OnManualModeChanged(bool isManual) { }

    /// <summary>Called when [ManualEntry] changes.</summary>
    protected virtual void OnManualEntryPicked(DataManualValue? entry) { }

    public DataNode(EnumDataType type, string? key)
    {
        Type = type;
        Key = key;
    }

    public abstract JToken? ToToken();

    /// <summary>
    /// What manual mode writes: nothing when nothing is picked, undefined included, as it writes
    /// what it is pointed at and no more.
    /// </summary>
    protected JToken? ManualToken() => ManualEntry is null or DataUndefined
        ? null
        : DataValue.TokenOf(ManualEntry.Value);

    /// <summary>Raised when the node or anything under it changes, whether it is expanded aside.</summary>
    public event EventHandler? Changed;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName != nameof(IsExpanded))
            OnChanged();
    }

    /// <summary>Raises <see cref="Changed"/> on the node and on every parent up to the root.</summary>
    protected void OnChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
        (Parent as DataNode)?.OnChanged();
    }

    #region Errors

    public bool HasErrors => _keyError is not null;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// The key error belongs to [Key] alone: reporting it as an error of the whole node (a null or
    /// empty name) would flag every <c>{Binding}</c> on the node, the tree's row presenters included.
    /// </summary>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (_keyError is not null && propertyName == nameof(Key))
            return new[] { _keyError };
        return Array.Empty<string>();
    }

    /// <summary>
    /// Flags every node of [siblings] whose key another one already uses. JSON keys are case
    /// sensitive, so is the comparison; a node without a key writes nothing and clashes with none.
    /// </summary>
    internal static void ValidateKeys(IEnumerable<DataNode> siblings)
    {
        List<DataNode> nodes = [.. siblings];
        HashSet<string> duplicates = [.. nodes
            .Where(node => node.Key is not null)
            .GroupBy(node => node.Key!, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)];

        foreach (DataNode node in nodes)
            node.SetKeyError(node.Key is not null && duplicates.Contains(node.Key)
                ? $"The key [{node.Key}] is already used."
                : null);
    }

    /// <summary>
    /// Lets the node go once its parent removed it: it no longer raises <see cref="Changed"/> on
    /// that parent, and out of any parent it has no sibling to clash with.
    /// </summary>
    internal void Detach()
    {
        Parent = null;
        SetKeyError(null);
    }

    private void SetKeyError(string? error)
    {
        if (_keyError == error)
            return;

        _keyError = error;
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Key)));
        OnPropertyChanged(nameof(HasErrors));
    }

    #endregion

    /// <summary>
    /// A deep copy of the node, values included. The copy stands on its own: it belongs to no
    /// <see cref="Parent"/> until one takes it.
    /// </summary>
    public abstract DataNode Clone();

    /// <summary>An empty node of [type]: an object without properties, an array without template, a value at its default.</summary>
    public static DataNode Create(EnumDataType type, string key) => type switch
    {
        EnumDataType.Object => new DataObject(key),
        EnumDataType.Array => new DataArray(key, null),
        _ => new DataValue(type, key, [])
    };
}

public interface IDataParent
{
    /// <summary>The nodes whose keys share this parent's scope.</summary>
    IEnumerable<DataNode> Children { get; }

    void Remove(DataNode node);
}

/// <summary>
/// The children of an object or an array. Clearing removes them one by one: the reset a plain
/// clear raises does not tell which nodes left, and their parent has to detach each of them.
/// </summary>
public class DataNodeCollection : ObservableCollection<DataNode>
{
    protected override void ClearItems()
    {
        while (Count > 0)
            RemoveAt(Count - 1);
    }
}
