using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;

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

    public IList SelectedCountries { get; } = new ObservableCollection<object> { "France", "Japan" };

    public string SearchCode =>
        "<inputs:Search />\n" +
        "// code-behind: search.SearchChanged += text => Filter(text);";

    public string ComboSearchCode =>
        "<inputs:ComboBoxSearch ItemsSource=\"{Binding Countries}\"\n" +
        "                       SelectedItem=\"{Binding SelectedCountry}\" />";

    public string TagsCode =>
        "<inputs:ComboBoxTags AllowAdd=\"True\"\n" +
        "                     ItemsSource=\"{Binding Countries}\"\n" +
        "                     SelectedItems=\"{Binding SelectedCountries}\" />";
}
