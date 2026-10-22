using System.Windows.Media;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// The colour a <see cref="ColorPicker"/> edits, seen as RGB, as hue / saturation / brightness and as hex at
    /// once : every edit goes through here and the picker only shows the result. Free of rendering so the colour
    /// rules are tested without a control.
    /// </summary>
    internal sealed class ColorModel
    {
        public Color Color { get; private set; } = System.Windows.Media.Colors.Black;

        /// <summary>Hue in degrees (0-360).</summary>
        public double Hue { get; private set; }
        /// <summary>Saturation (0-1).</summary>
        public double Saturation { get; private set; }
        /// <summary>Brightness / value (0-1).</summary>
        public double Brightness { get; private set; }
        /// <summary>Opacity (0-1), kept as given rather than rounded to the byte of <see cref="Color"/>.</summary>
        public double Alpha { get; private set; } = 1;

        public double AlphaPercent => Math.Round(Alpha * 100);

        /// <summary>The colour as <c>#RRGGBB</c>, the alpha being edited on its own.</summary>
        public string Hex => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

        /// <summary>The current hue fully saturated and bright, the base fill of the saturation square.</summary>
        public Color HueColor => FromHsv(Hue, 1, 1);

        public void SetColor(Color color)
        {
            Color = color;
            Alpha = color.A / 255d;
            ToHsv(color, out double hue, out double saturation, out double brightness);
            // Hue is undefined for greys and black : keep it where it was so the slider doesn't jump.
            if (saturation > 0)
                Hue = hue;
            Saturation = saturation;
            Brightness = brightness;
        }

        public void SetHsb(double hue, double saturation, double brightness)
        {
            Hue = hue;
            Saturation = Math.Clamp(saturation, 0, 1);
            Brightness = Math.Clamp(brightness, 0, 1);
            Color rgb = FromHsv(Hue, Saturation, Brightness);
            Color = Color.FromArgb(AlphaByte(), rgb.R, rgb.G, rgb.B);
        }

        public void SetAlpha(double alpha)
        {
            Alpha = Math.Clamp(alpha, 0, 1);
            Color = Color.FromArgb(AlphaByte(), Color.R, Color.G, Color.B);
        }

        public void SetAlphaPercent(double percent)
        {
            double clamped = Math.Clamp(percent, 0, 100);
            SetAlpha(clamped / 100);
        }

        /// <summary>
        /// Takes the colour [text] spells, a hex or a colour name, keeping the current alpha unless the text gives
        /// one (<c>#AARRGGBB</c> or <c>#ARGB</c>). Returns false, the colour unchanged, when [text] spells none.
        /// </summary>
        public bool TrySetHex(string? text)
        {
            if (!TryParse(text, out Color parsed))
                return false;

            Color color = SpellsAlpha(text!) ? parsed : Color.FromArgb(Color.A, parsed.R, parsed.G, parsed.B);
            SetColor(color);
            return true;
        }

        private byte AlphaByte() => (byte)Math.Round(Alpha * 255);

        private static bool SpellsAlpha(string text)
        {
            string trimmed = text.Trim();
            return trimmed.StartsWith('#') && (trimmed.Length == 9 || trimmed.Length == 5);
        }

        private static bool TryParse(string? text, out Color color)
        {
            color = System.Windows.Media.Colors.Black;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            try
            {
                object? parsed = ColorConverter.ConvertFromString(text.Trim());
                if (parsed is not Color parsedColor)
                    return false;
                color = parsedColor;
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        #region HSV <-> RGB
        private static void ToHsv(Color color, out double h, out double s, out double v)
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

        private static Color FromHsv(double h, double s, double v)
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
