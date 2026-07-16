using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class ColorPickerSamplesViewModel : ObservableObject
{
    private Color _color = Color.FromRgb(0x4F, 0x46, 0xE5);

    public Color Color { get => _color; set => SetProperty(ref _color, value); }

    public string ColorPickerCode =>
        "<inputs:ColorPicker Color=\"{Binding Color, Mode=TwoWay}\" />";
}
