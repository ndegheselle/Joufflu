using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class SelectionInputsSamplesViewModel : ObservableObject
{
    private string? _selectedCountry = "France";

    public ObservableCollection<string> Countries { get; } = new()
    {
        "Belgium", "Canada", "Denmark", "France", "Germany",
        "Italy", "Japan", "Norway", "Portugal", "Spain", "Sweden",
    };

    public string? SelectedCountry { get => _selectedCountry; set => SetProperty(ref _selectedCountry, value); }

    public string SearchCode =>
        "<inputs:Search />\n" +
        "// code-behind: search.SearchChanged += text => Filter(text);";

    public string ComboSearchCode =>
        "<inputs:ComboBoxSearch ItemsSource=\"{Binding Countries}\"\n" +
        "                       SelectedItem=\"{Binding SelectedCountry}\" />";
}
