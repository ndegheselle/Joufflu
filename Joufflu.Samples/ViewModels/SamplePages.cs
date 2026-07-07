using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Joufflu.Samples.ViewModels;

public class ButtonsViewModel : ObservableObject
{
    public string VariantsCode =>
        "<Button>Default</Button>\n" +
        "<Button Style=\"{StaticResource Primary}\">Primary</Button>\n" +
        "<Button Style=\"{StaticResource Secondary}\">Secondary</Button>\n" +
        "<Button Style=\"{StaticResource Ghost}\">Ghost</Button>\n" +
        "<Button Style=\"{StaticResource Success}\">Success</Button>\n" +
        "<Button Style=\"{StaticResource Danger}\">Danger</Button>";

    public string SizesCode =>
        "<Button joufflu:ControlProperties.Size=\"xs\">XS</Button>\n" +
        "<Button joufflu:ControlProperties.Size=\"sm\">SM</Button>\n" +
        "<Button joufflu:ControlProperties.Size=\"md\">MD</Button>\n" +
        "<Button joufflu:ControlProperties.Size=\"lg\">LG</Button>";

    public string IconCode =>
        "<Button joufflu:ControlProperties.IsSquare=\"True\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Plus}\" />\n" +
        "</Button>";
}

public class InputsViewModel : ObservableObject
{
    private string _text = "Hello";
    private bool _isChecked = true;
    private bool _isToggled;
    private string? _selectedOption = "Two";
    private DateTime? _selectedDate = DateTime.Today;
    private double _sliderValue = 40;

    public string Text { get => _text; set => SetProperty(ref _text, value); }

    public bool IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value); }

    public bool IsToggled { get => _isToggled; set => SetProperty(ref _isToggled, value); }

    public ObservableCollection<string> Options { get; } = new() { "One", "Two", "Three", "Four" };

    public string? SelectedOption { get => _selectedOption; set => SetProperty(ref _selectedOption, value); }

    public DateTime? SelectedDate { get => _selectedDate; set => SetProperty(ref _selectedDate, value); }

    public double SliderValue { get => _sliderValue; set => SetProperty(ref _sliderValue, value); }

    public string TextBoxCode => "<TextBox Text=\"{Binding Text}\" />";

    public string ComboCode => "<ComboBox ItemsSource=\"{Binding Options}\"\n          SelectedItem=\"{Binding SelectedOption}\" />";

    public string CheckCode =>
        "<CheckBox Content=\"Enabled\" IsChecked=\"{Binding IsChecked}\" />\n" +
        "<RadioButton Content=\"Choice\" />";
}

public class TreeNode
{
    public string Name { get; set; } = "";

    public ObservableCollection<TreeNode> Children { get; } = new();
}

public class DisplayViewModel : ObservableObject
{
    private double _progress = 65;

    public ObservableCollection<string> ListItems { get; } = new() { "Apple", "Banana", "Cherry", "Date", "Elderberry" };

    public ObservableCollection<TreeNode> Tree { get; } = new();

    public double Progress { get => _progress; set => SetProperty(ref _progress, value); }

    public DisplayViewModel()
    {
        var fruits = new TreeNode { Name = "Fruits" };
        fruits.Children.Add(new TreeNode { Name = "Apple" });
        fruits.Children.Add(new TreeNode { Name = "Banana" });

        var veggies = new TreeNode { Name = "Vegetables" };
        veggies.Children.Add(new TreeNode { Name = "Carrot" });
        veggies.Children.Add(new TreeNode { Name = "Potato" });

        Tree.Add(fruits);
        Tree.Add(veggies);
    }

    public string ListCode => "<ListBox ItemsSource=\"{Binding ListItems}\" />";

    public string TabCode =>
        "<TabControl>\n" +
        "    <TabItem Header=\"First\">...</TabItem>\n" +
        "    <TabItem Header=\"Second\">...</TabItem>\n" +
        "</TabControl>";
}

public class HelpersViewModel : ObservableObject
{
    public string SizeCode =>
        "<!-- Attached property drives height, font size and padding -->\n" +
        "<Button joufflu:ControlProperties.Size=\"xs\" />\n" +
        "<Button joufflu:ControlProperties.Size=\"sm\" />\n" +
        "<Button joufflu:ControlProperties.Size=\"md\" />  <!-- default -->\n" +
        "<Button joufflu:ControlProperties.Size=\"lg\" />\n\n" +
        "<!-- Size is inherited, so a panel sets it for every child -->\n" +
        "<StackPanel joufflu:ControlProperties.Size=\"lg\">\n" +
        "    <TextBox /> <ComboBox /> <Button>OK</Button>\n" +
        "</StackPanel>";

    public string SquareCode =>
        "<Button joufflu:ControlProperties.IsSquare=\"True\"\n" +
        "        joufflu:ControlProperties.Size=\"lg\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "</Button>";

    public string GapCode =>
        "<!-- Gap adds spacing between children of any panel -->\n" +
        "<!-- StackPanel: gap along the Orientation axis -->\n" +
        "<StackPanel Orientation=\"Horizontal\" joufflu:Spacing.Gap=\"8\">\n" +
        "    <Button>One</Button> <Button>Two</Button> <Button>Three</Button>\n" +
        "</StackPanel>\n\n" +
        "<!-- WrapPanel: gap on both axes, or per axis -->\n" +
        "<WrapPanel joufflu:Spacing.HorizontalGap=\"8\"\n" +
        "           joufflu:Spacing.VerticalGap=\"12\">\n" +
        "    ...\n" +
        "</WrapPanel>\n\n" +
        "<!-- Grid: gap sits between rows and columns -->\n" +
        "<Grid joufflu:Spacing.Gap=\"12\">\n" +
        "    ...\n" +
        "</Grid>";
}
