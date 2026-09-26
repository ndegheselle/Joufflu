using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

/// <summary>An <see cref="ObservableValidator"/> so the validation sample has errors to report.</summary>
public class ComboBoxTagsSamplesViewModel : ObservableValidator
{
    public ComboBoxTagsSamplesViewModel()
    {
        // The control edits the list in place, never replaces it: validate on each change.
        ((ObservableCollection<object>)RequiredCountries).CollectionChanged +=
            (_, _) => ValidateProperty(RequiredCountries, nameof(RequiredCountries));
        ValidateAllProperties();
    }

    public ObservableCollection<string> Countries { get; } = new()
    {
        "Belgium", "Canada", "Denmark", "France", "Germany",
        "Italy", "Japan", "Norway", "Portugal", "Spain", "Sweden",
    };

    public IList SelectedCountries { get; } = new ObservableCollection<object> { "France", "Japan" };

    [MinLength(2, ErrorMessage = "Pick at least 2 countries.")]
    public IList RequiredCountries { get; } = new ObservableCollection<object> { "France" };

    public string TagsCode =>
        "<inputs:ComboBoxTags AllowAdd=\"True\"\n" +
        "                     ItemsSource=\"{Binding Countries}\"\n" +
        "                     SelectedItems=\"{Binding SelectedCountries}\" />";

    public string ValidationCode =>
        "<inputs:ComboBoxTags ItemsSource=\"{Binding Countries}\"\n" +
        "                     SelectedItems=\"{Binding RequiredCountries}\" />\n\n" +
        "<!-- Opt out of the template: only the input's own red border remains -->\n" +
        "<inputs:ComboBoxTags Validation.ErrorTemplate=\"{x:Null}\" ... />";
}
