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

        // Every edit goes to the model, then the model is shown back : the guard keeps showing it from counting as
        // an edit.
        private readonly ColorModel _model = new();
        private bool _isShowingModel;

        private FrameworkElement? _saturationArea;
        private FrameworkElement? _saturationSelector;
        private TranslateTransform? _selectorTransform;

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public ColorPicker()
        {
            _model.SetColor(Color);
            ShowModel();
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
            if (picker._isShowingModel)
                return;
            picker._model.SetColor((Color)e.NewValue);
            picker.ShowModel();
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

        private static readonly DependencyPropertyKey AlphaBrushPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(AlphaBrush), typeof(Brush), typeof(ColorPicker), new PropertyMetadata(null));

        public static readonly DependencyProperty AlphaBrushProperty = AlphaBrushPropertyKey.DependencyProperty;

        /// <summary>Transparent -> current colour ramp used as the alpha slider track.</summary>
        public Brush AlphaBrush
        {
            get => (Brush)GetValue(AlphaBrushProperty);
            private set => SetValue(AlphaBrushPropertyKey, value);
        }
        #endregion

        #region HexText
        public static readonly DependencyProperty HexTextProperty = DependencyProperty.Register(
            nameof(HexText),
            typeof(string),
            typeof(ColorPicker),
            new PropertyMetadata("#000000", OnHexTextChanged));

        /// <summary>
        /// The colour as an editable <c>#RRGGBB</c> hex string. Pasting <c>#AARRGGBB</c> also sets the alpha, while
        /// <c>#RRGGBB</c> keeps it.
        /// </summary>
        public string HexText
        {
            get => (string)GetValue(HexTextProperty);
            set => SetValue(HexTextProperty, value);
        }

        private static void OnHexTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            if (picker._isShowingModel)
                return;
            // Text spelling no colour leaves the model as it was, so showing it reverts the box.
            picker._model.TrySetHex((string)e.NewValue);
            picker.ShowModel();
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
            if (picker._isShowingModel)
                return;
            picker._model.SetHsb(picker.Hue, picker.Saturation, picker.Brightness);
            picker.ShowModel();
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
            if (picker._isShowingModel)
                return;
            picker._model.SetAlpha((double)e.NewValue);
            picker.ShowModel();
        }

        private static void OnAlphaPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (ColorPicker)d;
            if (picker._isShowingModel)
                return;
            picker._model.SetAlphaPercent((double)e.NewValue);
            picker.ShowModel();
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

        /// <summary>
        /// Shows the model in every property : the colour, its hex, HSB and alpha, and the brushes previewing it.
        /// </summary>
        private void ShowModel()
        {
            _isShowingModel = true;
            try
            {
                // SetCurrentValue keeps the bindings a host or the template set on these properties.
                SetCurrentValue(ColorProperty, _model.Color);
                SetCurrentValue(HexTextProperty, _model.Hex);
                SetCurrentValue(HueProperty, _model.Hue);
                SetCurrentValue(SaturationProperty, _model.Saturation);
                SetCurrentValue(BrightnessProperty, _model.Brightness);
                SetCurrentValue(AlphaProperty, _model.Alpha);
                SetCurrentValue(AlphaPercentProperty, _model.AlphaPercent);
                ShowBrushes();
            }
            finally
            {
                _isShowingModel = false;
            }
            UpdateSelectorPosition();
        }

        private void ShowBrushes()
        {
            Color color = _model.Color;
            SwatchBrush = new SolidColorBrush(color);
            Color opaque = Color.FromRgb(color.R, color.G, color.B);

            OpaqueBrush = new SolidColorBrush(opaque);
            AlphaBrush = new LinearGradientBrush(
                new GradientStopCollection
                {
                    // Same RGB with alpha 0, not Colors.Transparent (#00FFFFFF),
                    // otherwise the ramp washes through white.
                    new GradientStop(Color.FromArgb(0, opaque.R, opaque.G, opaque.B), 0),
                    new GradientStop(opaque, 1),
                },
                new Point(0, 0), new Point(1, 0));
            HueBrush = new SolidColorBrush(_model.HueColor);
        }
    }
}
