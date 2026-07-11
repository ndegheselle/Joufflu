using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Joufflu.Samples.Controls;

/// <summary>
/// Compact colour picker: a swatch + hex text box, with an RGBA slider popup.
/// Two-way bindable through <see cref="Color"/>.
/// </summary>
public partial class ColorEditor : UserControl
{
    // Guards against feedback loops while syncing Color <-> hex box <-> sliders.
    private bool _isUpdating;

    public ColorEditor()
    {
        InitializeComponent();
        SyncFromColor(Color);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
        nameof(Color),
        typeof(Color),
        typeof(ColorEditor),
        new FrameworkPropertyMetadata(
            System.Windows.Media.Colors.Black,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnColorChanged));

    /// <summary>Brush mirror of <see cref="Color"/> for the swatch previews.</summary>
    public Brush SwatchBrush
    {
        get => (Brush)GetValue(SwatchBrushProperty);
        private set => SetValue(SwatchBrushPropertyKey, value);
    }

    private static readonly DependencyPropertyKey SwatchBrushPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(SwatchBrush),
        typeof(Brush),
        typeof(ColorEditor),
        new PropertyMetadata(System.Windows.Media.Brushes.Black));

    public static readonly DependencyProperty SwatchBrushProperty = SwatchBrushPropertyKey.DependencyProperty;

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (ColorEditor)d;
        if (editor._isUpdating)
            return;
        editor.SyncFromColor((Color)e.NewValue);
    }

    /// <summary>Pushes the current colour into the swatch, hex box and sliders.</summary>
    private void SyncFromColor(Color color)
    {
        _isUpdating = true;
        try
        {
            SwatchBrush = new SolidColorBrush(color);
            HexBox.Text = ToHex(color);
            SliderR.Value = color.R;
            SliderG.Value = color.G;
            SliderB.Value = color.B;
            SliderA.Value = color.A;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void Channel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdating)
            return;

        var color = Color.FromArgb((byte)SliderA.Value, (byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
        _isUpdating = true;
        try
        {
            SwatchBrush = new SolidColorBrush(color);
            HexBox.Text = ToHex(color);
            Color = color;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void HexBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_isUpdating)
            return;

        if (TryParse(HexBox.Text, out Color color))
            Color = color;
        else
            HexBox.Text = ToHex(Color); // revert invalid input
    }

    private static string ToHex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static bool TryParse(string? text, out Color color)
    {
        color = System.Windows.Media.Colors.Black;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        try
        {
            object? parsed = ColorConverter.ConvertFromString(text.Trim());
            if (parsed is Color c)
            {
                color = c;
                return true;
            }
        }
        catch
        {
            // fall through to false
        }
        return false;
    }
}
