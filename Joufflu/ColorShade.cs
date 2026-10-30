using System.Windows;
using System.Windows.Media;

namespace Joufflu;

/// <summary>
/// A colour derived from another by mixing it toward a second one: <see cref="Color"/> blended with
/// <see cref="Toward"/> by <see cref="Amount"/>. Declared as a resource and read through a binding, it lets a theme
/// define a few colours and have their hover and selected variants follow them:
/// <code>
/// &lt;joufflu:ColorShade x:Key="HoverShade" Amount="0.1"
///     Color="{DynamicResource {x:Static joufflu:Colors.PrimaryColor}}"
///     Toward="{DynamicResource {x:Static joufflu:Colors.PrimaryContentColor}}" /&gt;
/// &lt;SolidColorBrush Color="{Binding Result, Source={StaticResource HoverShade}}" /&gt;
/// </code>
/// <para>
/// Mixing toward the colour drawn on top of the fill, rather than darkening it, gives the direction a theme would
/// choose by hand: a dark fill with light content lightens, a pale fill with dark content darkens, in a light and a
/// dark theme alike.
/// </para>
/// <para>
/// A <see cref="Freezable"/> rather than a converter: it is evaluated once per instance and again only when one of its
/// inputs changes, not each time the colour is used, and no theme code has to run for it.
/// </para>
/// </summary>
public sealed class ColorShade : Freezable
{
    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
        nameof(Color), typeof(Color), typeof(ColorShade), new PropertyMetadata(default(Color), OnInputChanged));

    public static readonly DependencyProperty TowardProperty = DependencyProperty.Register(
        nameof(Toward), typeof(Color), typeof(ColorShade), new PropertyMetadata(default(Color), OnInputChanged));

    public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(
        nameof(Amount), typeof(double), typeof(ColorShade), new PropertyMetadata(0.0, OnInputChanged));

    private static readonly DependencyPropertyKey ResultPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Result), typeof(Color), typeof(ColorShade), new PropertyMetadata(default(Color)));

    public static readonly DependencyProperty ResultProperty = ResultPropertyKey.DependencyProperty;

    /// <summary>The colour the shade is derived from.</summary>
    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>The colour <see cref="Color"/> is mixed toward: the one drawn on top of it.</summary>
    public Color Toward
    {
        get => (Color)GetValue(TowardProperty);
        set => SetValue(TowardProperty, value);
    }

    /// <summary>How far to mix, from 0 (<see cref="Color"/> as it is) to 1 (<see cref="Toward"/>).</summary>
    public double Amount
    {
        get => (double)GetValue(AmountProperty);
        set => SetValue(AmountProperty, value);
    }

    /// <summary>The derived colour, recomputed when <see cref="Color"/>, <see cref="Toward"/> or <see cref="Amount"/> change.</summary>
    public Color Result => (Color)GetValue(ResultProperty);

    protected override Freezable CreateInstanceCore() => new ColorShade();

    private static void OnInputChanged(DependencyObject shade, DependencyPropertyChangedEventArgs e)
    {
        var self = (ColorShade)shade;
        self.SetValue(ResultPropertyKey, Mix(self.Color, self.Toward, self.Amount));
    }

    /// <summary>The colour blended <paramref name="amount"/> of the way toward <paramref name="toward"/>, its alpha kept.</summary>
    public static Color Mix(Color color, Color toward, double amount)
    {
        double t = Math.Clamp(amount, 0, 1);
        return Color.FromArgb(
            color.A,
            Channel(color.R, toward.R, t),
            Channel(color.G, toward.G, t),
            Channel(color.B, toward.B, t));
    }

    private static byte Channel(byte from, byte to, double t) => (byte)Math.Round(from + (to - from) * t);
}
