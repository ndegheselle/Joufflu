using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Joufflu.Samples.Views.Natives.DataInput;

public class ListBoxSamplesViewModel : ObservableObject
{
    public ObservableCollection<string> Items { get; } =
        new() { "Apple", "Banana", "Cherry", "Date", "Elderberry", "Fig", "Grape" };

    public string Code => "<ListBox ItemsSource=\"{Binding Items}\" />";

    public string MultiCode =>
        "<ListBox ItemsSource=\"{Binding Items}\"\n" +
        "         SelectionMode=\"Extended\" />";
}
