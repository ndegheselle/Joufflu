using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Joufflu.Samples.Views.Natives.DataDisplay;

/// <summary>Simple row model shared by the ListView and DataGrid samples.</summary>
public class Person
{
    public string Name { get; set; } = "";

    public string Role { get; set; } = "";

    public int Age { get; set; }
}

public class ListViewSamplesViewModel : ObservableObject
{
    public ObservableCollection<Person> People { get; } = new()
    {
        new Person { Name = "Ada Lovelace", Role = "Engineer", Age = 36 },
        new Person { Name = "Alan Turing", Role = "Researcher", Age = 41 },
        new Person { Name = "Grace Hopper", Role = "Admiral", Age = 79 },
        new Person { Name = "Katherine Johnson", Role = "Mathematician", Age = 52 },
    };

    public string Code =>
        "<ListView ItemsSource=\"{Binding People}\">\n" +
        "    <ListView.View>\n" +
        "        <GridView>\n" +
        "            <GridViewColumn Header=\"Name\" DisplayMemberBinding=\"{Binding Name}\" />\n" +
        "            <GridViewColumn Header=\"Role\" DisplayMemberBinding=\"{Binding Role}\" />\n" +
        "        </GridView>\n" +
        "    </ListView.View>\n" +
        "</ListView>";
}
