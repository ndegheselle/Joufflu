using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Data.Model;
using Joufflu.Inputs.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Samples.Views.Data;

public partial class DataDisplaySamplesViewModel : ObservableObject
{
    /// <summary>The JSON shown, with a forced value and a null among the others.</summary>
    public string Json { get; } = """
        {
          "Customer": "TBD",
          "PlacedOn": "2026-09-27T00:00:00",
          "Delivery": 1,
          "IsPaid": null,
          "Lines": [
            { "Reference": "A-102", "Quantity": 2, "UnitPrice": 12.5 },
            { "Reference": "B-7", "Quantity": 1, "UnitPrice": 99.9 }
          ]
        }
        """;

    /// <summary>The <see cref="Order"/> schema's tree, filled with [Json].</summary>
    public DataObject Node { get; }

    public string DisplayCode =>
        "Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();\n" +
        "Node.Load(JToken.Parse(json), ManualValues);\n\n" +
        "<data:DataDisplay Node=\"{Binding Node}\" />";

    /// <summary>A token given as is: <see cref="Node"/> reads it as a node.</summary>
    public JToken Token { get; } = new JValue("Hello");

    public string TokenCode =>
        "public JToken Token { get; } = new JValue(\"Hello\");\n\n" +
        "<data:DataDisplay Node=\"{Binding Token}\" />";

    /// <summary>The node of the last file opened, null until one is.</summary>
    [ObservableProperty]
    private DataNode? _fileNode;

    [ObservableProperty]
    private string _fileStatus = "No file opened.";

    public string FileCode =>
        "// When FilePath changes\n" +
        "using var reader = new JsonTextReader(File.OpenText(FilePath));\n" +
        "Node = await Task.Run(() => reader.ReadDataNode());\n\n" +
        "<inputs:FilePicker FilePath=\"{Binding FilePath}\" Options=\"{Binding JsonFileOptions}\" />\n" +
        "<data:DataDisplay Node=\"{Binding Node}\" />";

    public DataDisplaySamplesViewModel()
    {
        Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();
        Node.Load(JToken.Parse(Json), [new DataManualValue(EnumDataType.String, "TBD")]);
    }

    /// <summary>The file picked, read as soon as it changes. Cleared, it lets the shown file go.</summary>
    [ObservableProperty]
    private string? _filePath;

    public FilePickerOptions JsonFileOptions { get; } = new()
    {
        Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
        DefaultExtension = ".json"
    };

    partial void OnFilePathChanged(string? value) => _ = OpenFileAsync(value);

    private async Task OpenFileAsync(string? path)
    {
        // Let the previous file go before reading the next one: both could not fit at once.
        FileNode = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            FileStatus = "No file opened.";
            return;
        }

        string name = Path.GetFileName(path);
        FileStatus = $"Reading {name}...";

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Read off the UI thread: the node is only bound once whole, so it never changes under the tree.
            DataNode node = await Task.Run(() => ReadFile(path));
            // Another file picked while this one was read: that one is shown instead.
            if (path != FilePath)
                return;

            FileNode = node;
            double megabytes = new FileInfo(path).Length / 1024d / 1024d;
            FileStatus = $"{name}, {megabytes:0.#} MB read in {stopwatch.Elapsed.TotalSeconds:0.00} s.";
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            if (path == FilePath)
                FileStatus = $"{name} could not be read: {exception.Message}";
        }
    }

    private static DataNode ReadFile(string path)
    {
        using var reader = new JsonTextReader(File.OpenText(path));
        return reader.ReadDataNode();
    }
}
