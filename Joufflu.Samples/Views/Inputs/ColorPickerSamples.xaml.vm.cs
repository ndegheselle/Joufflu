using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

/// <summary>An <see cref="ObservableValidator"/> so the validation sample has errors to report.</summary>
public class ColorPickerSamplesViewModel : ObservableValidator
{
    private Color _color = Color.FromRgb(0x4F, 0x46, 0xE5);
    private Color _opaqueColor = Color.FromArgb(0x80, 0x4F, 0x46, 0xE5);

    public ColorPickerSamplesViewModel() => ValidateAllProperties();

    public Color Color { get => _color; set => SetProperty(ref _color, value); }

    [CustomValidation(typeof(ColorPickerSamplesViewModel), nameof(ValidateOpaque))]
    public Color OpaqueColor { get => _opaqueColor; set => SetProperty(ref _opaqueColor, value, validate: true); }

    public static ValidationResult? ValidateOpaque(Color color, ValidationContext _)
        => color.A == 0xFF ? ValidationResult.Success : new ValidationResult("The colour must be fully opaque.");

    public string ColorPickerCode =>
        "<inputs:ColorPicker Color=\"{Binding Color, Mode=TwoWay}\" />";

    public string ValidationCode =>
        "<inputs:ColorPicker Color=\"{Binding OpaqueColor}\" />\n\n" +
        "<!-- Opt out of the template: only the input's own red border remains -->\n" +
        "<inputs:ColorPicker Validation.ErrorTemplate=\"{x:Null}\" Color=\"{Binding OpaqueColor}\" />";
}
