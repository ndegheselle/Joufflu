using System.Windows;
using System.Windows.Media;
using Joufflu.Samples.Views.Themes;

namespace Joufflu.Samples.Tests.Themes;

/// <summary>
/// Fixtures run in a single-threaded apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class ColorSystemPreviewTests
{
    private ColorSystemPreview _preview = null!;

    [SetUp]
    public void Show()
    {
        _preview = new ColorSystemPreview();
        _preview.Measure(new Size(1000, 1400));
    }

    [Test]
    public void An_action_hovers_one_step_from_its_own_background_toward_the_foreground()
    {
        Color foreground = Brush("PreviewForegroundBrush");

        Assert.That(Brush("PreviewActionHoverBrush"), Is.EqualTo(ColorShade.Mix(Brush("PreviewActionSurface1Brush"), foreground, 0.1)));
    }

    [Test]
    public void A_focused_input_and_action_take_a_strong_border_derived_from_their_own()
    {
        Color foreground = Brush("PreviewForegroundBrush");

        Assert.Multiple(() =>
        {
            Assert.That(Brush("PreviewInputBorderStrongBrush"), Is.EqualTo(ColorShade.Mix(Brush("PreviewInputBorderBrush"), foreground, 0.4)));
            Assert.That(Brush("PreviewActionBorderStrongBrush"), Is.EqualTo(ColorShade.Mix(Brush("PreviewActionBorderBrush"), foreground, 0.4)));
        });
    }

    [Test]
    public void Each_category_brush_reads_its_own_colour_token()
    {
        Assert.Multiple(() =>
        {
            foreach (string category in new[] { "Display", "Input", "Action" })
                foreach (string token in new[] { "Surface1", "Surface2", "Border" })
                    Assert.That(Brush($"Preview{category}{token}Brush"), Is.EqualTo((Color)_preview.Resources[$"Preview{category}{token}"]), $"{category} {token}");
        });
    }

    private Color Brush(string key) => ((SolidColorBrush)_preview.Resources[key]).Color;
}
