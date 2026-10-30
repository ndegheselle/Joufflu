using System.Windows;
using System.Windows.Media;

namespace Joufflu;

/// <summary>
/// A colour that is optional in a theme: <see cref="Color"/> when the theme defines it, <see cref="Fallback"/>
/// otherwise. Declared as a resource and read through a binding, it lets a category colour follow the global one
/// until a theme overrides it:
/// <code>
/// &lt;joufflu:ColorFallback x:Key="InputBaseFallback"
///     Color="{DynamicResource {x:Static joufflu:Colors.InputBaseColor}}"
///     Fallback="{DynamicResource {x:Static joufflu:Colors.Base1Color}}" /&gt;
/// &lt;SolidColorBrush Color="{Binding Result, Source={StaticResource InputBaseFallback}}" /&gt;
/// </code>
/// <para>
/// <see cref="Color"/> is an <see cref="object"/> rather than a <see cref="System.Windows.Media.Color"/>: a
/// <c>DynamicResource</c> whose key is not found leaves it at <c>null</c>, which tells "not defined" apart from any
/// colour, transparent included.
/// </para>
/// </summary>
public sealed class ColorFallback : Freezable
{
    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
        nameof(Color), typeof(object), typeof(ColorFallback), new PropertyMetadata(null, OnInputChanged));

    public static readonly DependencyProperty FallbackProperty = DependencyProperty.Register(
        nameof(Fallback), typeof(Color), typeof(ColorFallback), new PropertyMetadata(default(Color), OnInputChanged));

    private static readonly DependencyPropertyKey ResultPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Result), typeof(Color), typeof(ColorFallback), new PropertyMetadata(default(Color)));

    public static readonly DependencyProperty ResultProperty = ResultPropertyKey.DependencyProperty;

    /// <summary>The optional colour, <c>null</c> when the theme does not define it.</summary>
    public object? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>The colour used while <see cref="Color"/> is not defined.</summary>
    public Color Fallback
    {
        get => (Color)GetValue(FallbackProperty);
        set => SetValue(FallbackProperty, value);
    }

    /// <summary><see cref="Color"/> if it is a colour, <see cref="Fallback"/> otherwise.</summary>
    public Color Result => (Color)GetValue(ResultProperty);

    protected override Freezable CreateInstanceCore() => new ColorFallback();

    private static void OnInputChanged(DependencyObject fallback, DependencyPropertyChangedEventArgs e)
    {
        var self = (ColorFallback)fallback;
        self.SetValue(ResultPropertyKey, self.Color is Color color ? color : self.Fallback);
    }
}
