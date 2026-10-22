using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Tests.Format;

/// <summary>
/// Hosts a format input in a real, off-screen window and drives it the way the keyboard does,
/// for what only WPF does: bindings, initialization, Loaded and the keyboard reaching the
/// editor. A window is needed: the control creates its editor once initialized, and key events
/// need a presentation source. Fixtures using it run in a single-threaded apartment, the only
/// one WPF controls can be created and driven from.
/// </summary>
internal sealed class FormatInputHost<TBox> : IDisposable where TBox : FormatTextBox
{
    private readonly Window _window;

    public TBox Box { get; }

    public FormatInputHost(TBox box)
    {
        Box = box;
        _window = new Window
        {
            Content = box,
            Width = 400,
            Height = 100,
            Left = -10000,
            Top = -10000,
            WindowStyle = WindowStyle.None,
            ShowActivated = false,
            ShowInTaskbar = false,
        };
        _window.Show();
        DoEvents();
    }

    /// <summary>
    /// Type [text] one character at a time.
    /// </summary>
    public void Type(string text)
    {
        foreach (char character in text)
        {
            var composition = new TextComposition(InputManager.Current, Box, character.ToString());
            var args = new TextCompositionEventArgs(Keyboard.PrimaryDevice, composition)
            {
                RoutedEvent = TextCompositionManager.PreviewTextInputEvent,
            };
            Box.RaiseEvent(args);
        }
    }

    /// <summary>
    /// Press [key], returning whether the box handled it: a key it leaves goes on to WPF, Tab
    /// moving the focus to the next control for instance.
    /// </summary>
    public bool Press(Key key)
    {
        PresentationSource source = PresentationSource.FromVisual(_window);
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent,
        };
        Box.RaiseEvent(args);
        return args.Handled;
    }

    /// <summary>
    /// Put the caret at [index], as a click there would.
    /// </summary>
    public void Click(int index) => Box.Select(index, 0);

    /// <summary>
    /// Take the box out of the window and put it back, which raises Loaded a second time.
    /// </summary>
    public void Reload()
    {
        _window.Content = null;
        DoEvents();
        _window.Content = Box;
        DoEvents();
    }

    public void Dispose() => _window.Close();

    // Let the dispatcher run what was queued (Loaded is raised asynchronously).
    private static void DoEvents()
        => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
}
