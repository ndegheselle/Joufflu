using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class ComboBoxTagsSamplesViewModel : ObservableObject
{
    public ObservableCollection<string> Countries { get; } = new()
    {
        "Belgium", "Canada", "Denmark", "France", "Germany",
        "Italy", "Japan", "Norway", "Portugal", "Spain", "Sweden",
    };

    public IList SelectedCountries { get; } = new ObservableCollection<object> { "France", "Japan" };

    public string TagsCode =>
        "<inputs:ComboBoxTags AllowAdd=\"True\"\n" +
        "                     ItemsSource=\"{Binding Countries}\"\n" +
        "                     SelectedItems=\"{Binding SelectedCountries}\" />";
}
