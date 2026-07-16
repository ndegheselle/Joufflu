using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class FilePickerSamplesViewModel : ObservableObject
{
    private string? _filePath;

    public string? FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }

    public string FilePickerCode =>
        "<inputs:FilePicker FilePath=\"{Binding FilePath, Mode=TwoWay}\" />";
}
