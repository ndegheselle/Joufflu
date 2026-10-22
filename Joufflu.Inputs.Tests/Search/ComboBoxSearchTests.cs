using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Search;

/// <summary>
/// What only WPF does : the filter reaching the list a box shows. The boxes live in a real, off-screen window, as
/// the filter is attached once loaded, and fixtures run in a single-threaded apartment, the only one WPF controls
/// can be created and driven from.
/// </summary>
[Apartment(ApartmentState.STA)]
public class ComboBoxSearchTests
{
    private readonly List<string> _cities = ["Paris", "Lyon", "Marseille"];
    private Window _window = null!;
    private ComboBoxSearch _box = null!;
    private ComboBoxSearch _otherBox = null!;

    [SetUp]
    public void SetUp()
    {
        _box = new ComboBoxSearch { ItemsSource = _cities };
        _otherBox = new ComboBoxSearch { ItemsSource = _cities };
        var panel = new StackPanel();
        panel.Children.Add(_box);
        panel.Children.Add(_otherBox);
        _window = new Window
        {
            Content = panel,
            Width = 400,
            Height = 200,
            Left = -10000,
            Top = -10000,
            WindowStyle = WindowStyle.None,
            ShowActivated = false,
            ShowInTaskbar = false,
        };
        _window.Show();
        DoEvents();
    }

    [TearDown]
    public void TearDown() => _window.Close();

    [Test]
    public void Typing_shows_only_the_items_containing_the_text()
    {
        Type(_box, "ar");

        Assert.That(_box.Items, Is.EqualTo(new[] { "Paris", "Marseille" }));
    }

    [Test]
    public void Typing_in_one_box_leaves_another_box_on_the_same_items_whole()
    {
        Type(_box, "ar");

        Assert.That(_otherBox.Items, Is.EqualTo(_cities));
    }

    [Test]
    public void Typing_an_item_name_selects_it()
    {
        Type(_box, "Lyon");

        Assert.That(_box.SelectedItem, Is.EqualTo("Lyon"));
    }

    [Test]
    public void Opening_the_drop_down_after_a_pick_shows_every_item()
    {
        Type(_box, "Lyon");
        _box.IsDropDownOpen = false;
        DoEvents();

        _box.IsDropDownOpen = true;
        DoEvents();

        Assert.That(_box.Items, Is.EqualTo(_cities));
    }

    private static void Type(ComboBoxSearch box, string text)
    {
        box.Text = text;
        DoEvents();
    }

    // Let the dispatcher run what was queued (Loaded and the drop down are handled asynchronously).
    private static void DoEvents()
        => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
}
