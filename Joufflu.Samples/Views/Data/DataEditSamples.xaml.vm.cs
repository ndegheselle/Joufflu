using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Samples.Views.Data;

public partial class DataEditSamplesViewModel : ObservableObject
{
    /// <summary>The object built from scratch.</summary>
    [ObservableProperty]
    private DataObject _node = new("");

    /// <summary>The JSON last generated from [Node].</summary>
    [ObservableProperty]
    private string? _json;

    public string EditCode =>
        "<jata:DataEdit Node=\"{Binding Node}\" />\n\n" +
        "Json = Node.ToToken()?.ToString();";

    /// <summary>Starts over from an empty root object.</summary>
    [RelayCommand]
    private void Clear()
    {
        Node = new DataObject("");
        Json = null;
    }

    [RelayCommand]
    private void GenerateJson() => Json = Node.ToToken()?.ToString();
}
