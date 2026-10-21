using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Search;

/// <summary>
/// The box lives in a real, off-screen window, as key events need a presentation source, and fixtures run in a
/// single-threaded apartment, the only one WPF controls can be created and driven from.
/// </summary>
[Apartment(ApartmentState.STA)]
public class ComboBoxTagsTests
{
    private Window _window = null!;
    private ComboBoxTags _box = null!;

    [SetUp]
    public void SetUp()
    {
        _box = new ComboBoxTags
        {
            ItemsSource = new List<string> { "Paris", "Lyon", "Marseille" },
            SelectedItems = new List<string> { "Paris", "Lyon" },
        };
        _window = new Window
        {
            Content = _box,
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
    public void Backspace_with_no_text_removes_only_the_last_tag()
    {
        Press(Key.Back);

        Assert.That(_box.SelectedItems, Is.EqualTo(new[] { "Paris" }));
    }

    // Raise the key as WPF routes it to the box itself : the preview, then, when nothing handled it, the key down.
    private void Press(Key key)
    {
        PresentationSource source = PresentationSource.FromVisual(_window);
        var preview = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
        _box.RaiseEvent(preview);
        if (!preview.Handled)
        {
            var keyDown = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.KeyDownEvent };
            _box.RaiseEvent(keyDown);
        }
        DoEvents();
    }

    private static void DoEvents()
        => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
}
