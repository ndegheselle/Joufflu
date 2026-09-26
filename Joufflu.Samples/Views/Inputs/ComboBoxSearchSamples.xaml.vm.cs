using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

/// <summary>An <see cref="ObservableValidator"/> so the validation sample has errors to report.</summary>
public class ComboBoxSearchSamplesViewModel : ObservableValidator
{
    private string? _selectedCountry = "France";
    private string? _requiredCountry;

    public ComboBoxSearchSamplesViewModel() => ValidateAllProperties();

    public ObservableCollection<string> Countries { get; } = new()
    {
        "Belgium", "Canada", "Denmark", "France", "Germany",
        "Italy", "Japan", "Norway", "Portugal", "Spain", "Sweden",
    };

    public string? SelectedCountry { get => _selectedCountry; set => SetProperty(ref _selectedCountry, value); }

    [Required(ErrorMessage = "Pick a country.")]
    public string? RequiredCountry { get => _requiredCountry; set => SetProperty(ref _requiredCountry, value, validate: true); }

    public string ComboSearchCode =>
        "<inputs:ComboBoxSearch ItemsSource=\"{Binding Countries}\"\n" +
        "                       SelectedItem=\"{Binding SelectedCountry}\" />";

    public string ValidationCode =>
        "<inputs:ComboBoxSearch ItemsSource=\"{Binding Countries}\"\n" +
        "                       SelectedItem=\"{Binding RequiredCountry}\" />\n\n" +
        "<!-- Opt out of the template: only the input's own red border remains -->\n" +
        "<inputs:ComboBoxSearch Validation.ErrorTemplate=\"{x:Null}\" ... />";
}
