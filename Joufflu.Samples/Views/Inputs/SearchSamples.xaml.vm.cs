using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class SearchSamplesViewModel : ObservableObject
{
    public string SearchCode =>
        "<!-- MVVM: SearchText is two-way by default and only updates once typing settles -->\n" +
        "<inputs:Search SearchText=\"{Binding Query}\" />\n" +
        "<!-- or a command, executed with the text -->\n" +
        "<inputs:Search SearchCommand=\"{Binding SearchCommand}\" />\n\n" +
        "// code-behind: search.SearchChanged += text => Filter(text);";
}
