using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Joufflu.Inputs
{
    /// <summary>
    /// Colour picker: an editable hex text box you can paste a colour into, with a button
    /// that opens an RGBA slider popup to pick a colour manually.
    /// Two-way bindable through <see cref="Color"/>.
    /// </summary>
    public class ColorPicker : Control
    {
        // Guards against feedback loops while syncing Color <-> hex text <-> channels.
        private bool _isUpdating;

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public ColorPicker()
        {
            SyncFromColor(Color);
        }

        #region Color
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(System.Windows.Media.Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            if (picker._isUpdating)
                return;
            picker.SyncFromColor((Color)e.NewValue);
        }
        #endregion

        #region SwatchBrush (read-only)
        private static readonly DependencyPropertyKey SwatchBrushPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SwatchBrush),
            typeof(Brush),
            typeof(ColorPicker),
            new PropertyMetadata(System.Windows.Media.Brushes.Black));

        public static readonly DependencyProperty SwatchBrushProperty = SwatchBrushPropertyKey.DependencyProperty;

        /// <summary>Brush mirror of <see cref="Color"/> for the swatch previews.</summary>
        public Brush SwatchBrush
        {
            get => (Brush)GetValue(SwatchBrushProperty);
            private set => SetValue(SwatchBrushPropertyKey, value);
        }
        #endregion

        #region HexText
        public static readonly DependencyProperty HexTextProperty = DependencyProperty.Register(
            nameof(HexText),
            typeof(string),
            typeof(ColorPicker),
            new PropertyMetadata("#FF000000", OnHexTextChanged));

        /// <summary>The colour as an editable <c>#AARRGGBB</c> hex string.</summary>
        public string HexText
        {
            get => (string)GetValue(HexTextProperty);
            set => SetValue(HexTextProperty, value);
        }

        private static void OnHexTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            if (picker._isUpdating)
                return;

            if (TryParse((string)e.NewValue, out Color color))
                picker.Color = color;
            else
                picker.SetHexText(ToHex(picker.Color)); // revert invalid input
        }
        #endregion

        #region R / G / B / A channels
        public static readonly DependencyProperty RProperty = RegisterChannel(nameof(R));
        public static readonly DependencyProperty GProperty = RegisterChannel(nameof(G));
        public static readonly DependencyProperty BProperty = RegisterChannel(nameof(B));
        public static readonly DependencyProperty AProperty = RegisterChannel(nameof(A));

        public double R { get => (double)GetValue(RProperty); set => SetValue(RProperty, value); }
        public double G { get => (double)GetValue(GProperty); set => SetValue(GProperty, value); }
        public double B { get => (double)GetValue(BProperty); set => SetValue(BProperty, value); }
        public double A { get => (double)GetValue(AProperty); set => SetValue(AProperty, value); }

        private static DependencyProperty RegisterChannel(string name) => DependencyProperty.Register(
            name,
            typeof(double),
            typeof(ColorPicker),
            new PropertyMetadata(0d, OnChannelChanged));

        private static void OnChannelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            if (picker._isUpdating)
                return;
            picker.SyncFromChannels();
        }
        #endregion

        /// <summary>Pushes the current colour into the swatch, hex text and channels.</summary>
        private void SyncFromColor(Color color)
        {
            _isUpdating = true;
            try
            {
                SwatchBrush = new SolidColorBrush(color);
                HexText = ToHex(color);
                R = color.R;
                G = color.G;
                B = color.B;
                A = color.A;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>Rebuilds the colour from the R/G/B/A channels.</summary>
        private void SyncFromChannels()
        {
            var color = Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B);
            _isUpdating = true;
            try
            {
                SwatchBrush = new SolidColorBrush(color);
                HexText = ToHex(color);
                Color = color;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void SetHexText(string text)
        {
            _isUpdating = true;
            try
            {
                HexText = text;
            }
            finally
            {
                _isUpdating = false;
            }
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
}
