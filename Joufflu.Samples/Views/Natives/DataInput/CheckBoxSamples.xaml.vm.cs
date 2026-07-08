using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.DataInput;

public class CheckBoxSamplesViewModel : ObservableObject
{
    private bool _isChecked = true;

    public bool IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value); }

    public string StatesCode =>
        "<CheckBox Content=\"Bound\" IsChecked=\"{Binding IsChecked}\" />\n" +
        "<CheckBox Content=\"Unchecked\" />\n" +
        "<CheckBox Content=\"Indeterminate\" IsThreeState=\"True\" IsChecked=\"{x:Null}\" />\n" +
        "<CheckBox Content=\"Disabled\" IsEnabled=\"False\" />";
}
