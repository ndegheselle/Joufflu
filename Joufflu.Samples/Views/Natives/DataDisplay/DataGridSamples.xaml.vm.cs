using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.DataDisplay;

public class DataGridSamplesViewModel : ObservableObject
{
    public ObservableCollection<Person> People { get; } = new()
    {
        new Person { Name = "Ada Lovelace", Role = "Engineer", Age = 36, IsActive = true },
        new Person { Name = "Alan Turing", Role = "Researcher", Age = 41 },
        new Person { Name = "Grace Hopper", Role = "Admiral", Age = 79, IsActive = true },
        new Person { Name = "Katherine Johnson", Role = "Mathematician", Age = 52, IsActive = true },
        new Person { Name = "Edsger Dijkstra", Role = "Engineer", Age = 72 },
    };

    /// <summary>
    /// Choices of the role combo box column. Static because DataGrid columns are not in the visual
    /// tree, so their ItemsSource cannot bind to the DataContext.
    /// </summary>
    public static IReadOnlyList<string> Roles { get; } =
        ["Admiral", "Engineer", "Mathematician", "Researcher"];

    public string Code =>
        "<DataGrid ItemsSource=\"{Binding People}\" AutoGenerateColumns=\"False\">\n" +
        "    <DataGrid.Columns>\n" +
        "        <DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\" />\n" +
        "        <DataGridTextColumn Header=\"Role\" Binding=\"{Binding Role}\" />\n" +
        "    </DataGrid.Columns>\n" +
        "</DataGrid>";

    public string CompactCode =>
        "<DataGrid toolkit:Sizing.Size=\"xs\"\n" +
        "          ItemsSource=\"{Binding People}\" AutoGenerateColumns=\"False\">\n" +
        "    <DataGrid.Columns>\n" +
        "        <DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\" />\n" +
        "        <DataGridTextColumn Header=\"Role\" Binding=\"{Binding Role}\" />\n" +
        "    </DataGrid.Columns>\n" +
        "</DataGrid>";

    public string ColumnTypesCode =>
        "<DataGrid ItemsSource=\"{Binding People}\" AutoGenerateColumns=\"False\">\n" +
        "    <DataGrid.Columns>\n" +
        "        <DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\" />\n" +
        "        <DataGridComboBoxColumn Header=\"Role\"\n" +
        "                                ItemsSource=\"{x:Static vm:DataGridSamplesViewModel.Roles}\"\n" +
        "                                SelectedItemBinding=\"{Binding Role}\" />\n" +
        "        <DataGridCheckBoxColumn Header=\"Active\" Binding=\"{Binding IsActive}\" />\n" +
        "    </DataGrid.Columns>\n" +
        "</DataGrid>";

    public string RowDetailsCode =>
        "<DataGrid ItemsSource=\"{Binding People}\" AutoGenerateColumns=\"False\"\n" +
        "          RowDetailsVisibilityMode=\"VisibleWhenSelected\">\n" +
        "    <DataGrid.RowDetailsTemplate>\n" +
        "        <DataTemplate>\n" +
        "            <TextBlock Text=\"{Binding Role}\" />\n" +
        "        </DataTemplate>\n" +
        "    </DataGrid.RowDetailsTemplate>\n" +
        "</DataGrid>";
}
