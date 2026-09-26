using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

/// <summary>An <see cref="ObservableValidator"/> so the validation sample has errors to report.</summary>
public class FilePickerSamplesViewModel : ObservableValidator
{
    private string? _filePath;
    private string? _requiredFilePath;

    public FilePickerSamplesViewModel() => ValidateAllProperties();

    public string? FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }

    [Required(ErrorMessage = "Choose a file.")]
    public string? RequiredFilePath { get => _requiredFilePath; set => SetProperty(ref _requiredFilePath, value, validate: true); }

    public string FilePickerCode =>
        "<inputs:FilePicker FilePath=\"{Binding FilePath, Mode=TwoWay}\" />";

    public string ValidationCode =>
        "<inputs:FilePicker FilePath=\"{Binding RequiredFilePath}\" />\n\n" +
        "<!-- Opt out of the template: only the input's own red border remains -->\n" +
        "<inputs:FilePicker Validation.ErrorTemplate=\"{x:Null}\" FilePath=\"{Binding RequiredFilePath}\" />";
}
