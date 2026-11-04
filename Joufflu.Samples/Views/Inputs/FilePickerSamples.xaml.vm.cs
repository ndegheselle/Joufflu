using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Inputs.Controls;

namespace Joufflu.Samples.Views.Inputs;

/// <summary>An <see cref="ObservableValidator"/> so the validation sample has errors to report.</summary>
public class FilePickerSamplesViewModel : ObservableValidator
{
    private string? _filePath;
    private string? _imagePath;
    private string? _requiredFilePath;

    public FilePickerSamplesViewModel() => ValidateAllProperties();

    public string? FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }

    public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }

    public FilePickerOptions ImageOptions { get; } = new()
    {
        Filter = "Images (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*",
        DefaultExtension = ".png"
    };

    [Required(ErrorMessage = "Choose a file.")]
    public string? RequiredFilePath { get => _requiredFilePath; set => SetProperty(ref _requiredFilePath, value, validate: true); }

    public string FilePickerCode =>
        "<inputs:FilePicker FilePath=\"{Binding FilePath, Mode=TwoWay}\" />";

    public string FileTypesCode =>
        "<inputs:FilePicker FilePath=\"{Binding ImagePath}\" Options=\"{Binding ImageOptions}\" />\n\n" +
        "public FilePickerOptions ImageOptions { get; } = new()\n" +
        "{\n" +
        "    Filter = \"Images (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*\",\n" +
        "    DefaultExtension = \".png\"\n" +
        "};";

    public string ValidationCode =>
        "<inputs:FilePicker FilePath=\"{Binding RequiredFilePath}\" />\n\n" +
        "<!-- Opt out of the template: only the input's own red border remains -->\n" +
        "<inputs:FilePicker Validation.ErrorTemplate=\"{x:Null}\" FilePath=\"{Binding RequiredFilePath}\" />";
}
