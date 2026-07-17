using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Controls;

/// <summary>
/// An indeterminate, continuously spinning loading indicator. Its color comes from
/// <see cref="Control.Foreground"/> (accent by default) and its diameter from
/// <see cref="ControlProperties.Size"/>.
/// </summary>
public class Spinner : Control
{
    static Spinner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Spinner),
            new FrameworkPropertyMetadata(typeof(Spinner)));
    }
}
