using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// Colour picker: a swatch button that opens a popup with a saturation/brightness square,
    /// a hue slider, an alpha slider and an editable hex value.
    /// Two-way bindable through <see cref="Color"/>.
    /// </summary>
    [TemplatePart(Name = PartSaturationArea, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PartSaturationSelector, Type = typeof(FrameworkElement))]
    public class ColorPicker : Control
    {
        private const string PartSaturationArea = "PART_SaturationArea";
        private const string PartSaturationSelector = "PART_SaturationSelector";

        // Guards against feedback loops while syncing Color <-> hex text <-> HSB <-> alpha.
        private bool _isUpdating;

        private FrameworkElement? _saturationArea;
        private FrameworkElement? _saturationSelector;
        private TranslateTransform? _selectorTransform;

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

        #region HueBrush (read-only)
        private static readonly DependencyPropertyKey HueBrushPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(HueBrush),
            typeof(Brush),
            typeof(ColorPicker),
            new PropertyMetadata(System.Windows.Media.Brushes.Red));

        public static readonly DependencyProperty HueBrushProperty = HueBrushPropertyKey.DependencyProperty;

        /// <summary>Fully saturated/bright colour for the current <see cref="Hue"/>; base fill of the saturation square.</summary>
        public Brush HueBrush
        {
            get => (Brush)GetValue(HueBrushProperty);
            private set => SetValue(HueBrushPropertyKey, value);
        }
        #endregion

        #region OpaqueBrush (read-only)
        private static readonly DependencyPropertyKey OpaqueBrushPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(OpaqueBrush),
            typeof(Brush),
            typeof(ColorPicker),
            new PropertyMetadata(System.Windows.Media.Brushes.Black));

        public static readonly DependencyProperty OpaqueBrushProperty = OpaqueBrushPropertyKey.DependencyProperty;

        /// <summary>The current colour at full opacity; used as the opaque end of the alpha slider gradient.</summary>
        public Brush OpaqueBrush
        {
            get => (Brush)GetValue(OpaqueBrushProperty);
            private set => SetValue(OpaqueBrushPropertyKey, value);
        }
        #endregion

        #region HexText
        public static readonly DependencyProperty HexTextProperty = DependencyProperty.Register(
            nameof(HexText),
            typeof(string),
            typeof(ColorPicker),
            new PropertyMetadata("#000000", OnHexTextChanged));

        /// <summary>The colour as an editable <c>#RRGGBB</c> hex string (paste also accepts <c>#AARRGGBB</c>).</summary>
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
                // Keep the current alpha: the hex box only edits RGB.
                picker.Color = Color.FromArgb(picker.Color.A, color.R, color.G, color.B);
            else
                picker.SetGuarded(HexTextProperty, ToHex(picker.Color)); // revert invalid input
        }
        #endregion

        #region Hue / Saturation / Brightness
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register(
            nameof(Hue), typeof(double), typeof(ColorPicker), new PropertyMetadata(0d, OnHsbChanged));

        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            nameof(Saturation), typeof(double), typeof(ColorPicker), new PropertyMetadata(0d, OnHsbChanged));

        public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register(
            nameof(Brightness), typeof(double), typeof(ColorPicker), new PropertyMetadata(0d, OnHsbChanged));

        /// <summary>Hue in degrees (0-360).</summary>
        public double Hue { get => (double)GetValue(HueProperty); set => SetValue(HueProperty, value); }
        /// <summary>Saturation (0-1).</summary>
        public double Saturation { get => (double)GetValue(SaturationProperty); set => SetValue(SaturationProperty, value); }
        /// <summary>Brightness / value (0-1).</summary>
        public double Brightness { get => (double)GetValue(BrightnessProperty); set => SetValue(BrightnessProperty, value); }

        private static void OnHsbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            picker.UpdateSelectorPosition();
            if (picker._isUpdating)
                return;
            picker.SyncFromHsb();
        }
        #endregion

        #region Alpha / AlphaPercent
        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register(
            nameof(Alpha), typeof(double), typeof(ColorPicker), new PropertyMetadata(1d, OnAlphaChanged));

        public static readonly DependencyProperty AlphaPercentProperty = DependencyProperty.Register(
            nameof(AlphaPercent), typeof(double), typeof(ColorPicker), new PropertyMetadata(100d, OnAlphaPercentChanged));

        /// <summary>Opacity (0-1) driving the alpha slider.</summary>
        public double Alpha { get => (double)GetValue(AlphaProperty); set => SetValue(AlphaProperty, value); }
        /// <summary>Opacity as a percentage (0-100) for the editable percent box.</summary>
        public double AlphaPercent { get => (double)GetValue(AlphaPercentProperty); set => SetValue(AlphaPercentProperty, value); }

        private static void OnAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            if (picker._isUpdating)
                return;
            picker.SetGuarded(AlphaPercentProperty, Math.Round((double)e.NewValue * 100));
            picker.SyncFromHsb();
        }

        private static void OnAlphaPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            if (picker._isUpdating)
                return;
            double percent = Math.Clamp((double)e.NewValue, 0, 100);
            picker.Alpha = percent / 100d; // drives OnAlphaChanged
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_saturationArea != null)
            {
                _saturationArea.MouseLeftButtonDown -= OnSaturationMouseDown;
                _saturationArea.MouseLeftButtonUp -= OnSaturationMouseUp;
                _saturationArea.MouseMove -= OnSaturationMouseMove;
                _saturationArea.SizeChanged -= OnSaturationSizeChanged;
            }

            _saturationArea = GetTemplateChild(PartSaturationArea) as FrameworkElement;
            _saturationSelector = GetTemplateChild(PartSaturationSelector) as FrameworkElement;

            _selectorTransform = new TranslateTransform();
            if (_saturationSelector != null)
                _saturationSelector.RenderTransform = _selectorTransform;

            if (_saturationArea != null)
            {
                _saturationArea.MouseLeftButtonDown += OnSaturationMouseDown;
                _saturationArea.MouseLeftButtonUp += OnSaturationMouseUp;
                _saturationArea.MouseMove += OnSaturationMouseMove;
                _saturationArea.SizeChanged += OnSaturationSizeChanged;
            }

            UpdateSelectorPosition();
        }

        #region Saturation square interaction
        private void OnSaturationSizeChanged(object sender, SizeChangedEventArgs e) => UpdateSelectorPosition();

        private void OnSaturationMouseDown(object sender, MouseButtonEventArgs e)
        {
            _saturationArea?.CaptureMouse();
            SetFromSaturationPoint(e.GetPosition(_saturationArea));
        }

        private void OnSaturationMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _saturationArea?.IsMouseCaptured == true)
                SetFromSaturationPoint(e.GetPosition(_saturationArea));
        }

        private void OnSaturationMouseUp(object sender, MouseButtonEventArgs e) => _saturationArea?.ReleaseMouseCapture();

        private void SetFromSaturationPoint(Point point)
        {
            if (_saturationArea == null || _saturationArea.ActualWidth <= 0 || _saturationArea.ActualHeight <= 0)
                return;

            double x = Math.Clamp(point.X / _saturationArea.ActualWidth, 0, 1);
            double y = Math.Clamp(point.Y / _saturationArea.ActualHeight, 0, 1);
            Saturation = x;
            Brightness = 1 - y;
        }

        private void UpdateSelectorPosition()
        {
            if (_saturationArea == null || _selectorTransform == null)
                return;

            _selectorTransform.X = Saturation * _saturationArea.ActualWidth;
            _selectorTransform.Y = (1 - Brightness) * _saturationArea.ActualHeight;
        }
        #endregion

        /// <summary>Pushes an externally-set colour into every derived value (hex, HSB, alpha, brushes).</summary>
        private void SyncFromColor(Color color)
        {
            _isUpdating = true;
            try
            {
                RgbToHsv(color, out double h, out double s, out double v);
                // Hue is undefined for greys/black; keep the slider where it was so it doesn't jump.
                if (s > 0)
                    Hue = h;
                Saturation = s;
                Brightness = v;

                Alpha = color.A / 255d;
                AlphaPercent = Math.Round(Alpha * 100);
                UpdateDerived(color);
            }
            finally
            {
                _isUpdating = false;
            }
            UpdateSelectorPosition();
        }

        /// <summary>Rebuilds the colour from the current hue/saturation/brightness and alpha.</summary>
        private void SyncFromHsb()
        {
            Color rgb = HsvToRgb(Hue, Saturation, Brightness);
            var color = Color.FromArgb((byte)Math.Round(Math.Clamp(Alpha, 0, 1) * 255), rgb.R, rgb.G, rgb.B);
            _isUpdating = true;
            try
            {
                Color = color;
                UpdateDerived(color);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>Refreshes the read-only brushes and hex text from a colour (caller owns the update guard).</summary>
        private void UpdateDerived(Color color)
        {
            SwatchBrush = new SolidColorBrush(color);
            OpaqueBrush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
            HueBrush = new SolidColorBrush(HsvToRgb(Hue, 1, 1));
            HexText = ToHex(color);
        }

        /// <summary>Sets a dependency property without re-triggering the sync logic.</summary>
        private void SetGuarded(DependencyProperty property, object value)
        {
            _isUpdating = true;
            try
            {
                SetValue(property, value);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

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

        #region HSV <-> RGB
        private static void RgbToHsv(Color color, out double h, out double s, out double v)
        {
            double r = color.R / 255d, g = color.G / 255d, b = color.B / 255d;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            v = max;
            s = max <= 0 ? 0 : delta / max;

            if (delta <= 0)
            {
                h = 0;
                return;
            }

            if (max == r)
                h = 60 * (((g - b) / delta) % 6);
            else if (max == g)
                h = 60 * (((b - r) / delta) + 2);
            else
                h = 60 * (((r - g) / delta) + 4);

            if (h < 0)
                h += 360;
        }

        private static Color HsvToRgb(double h, double s, double v)
        {
            h = ((h % 360) + 360) % 360;
            s = Math.Clamp(s, 0, 1);
            v = Math.Clamp(v, 0, 1);

            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60 % 2) - 1));
            double m = v - c;

            double r, g, b;
            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromRgb(
                (byte)Math.Round((r + m) * 255),
                (byte)Math.Round((g + m) * 255),
                (byte)Math.Round((b + m) * 255));
        }
        #endregion
    }
}
