using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.DataInput;

public class SliderSamplesViewModel : ObservableObject
{
    private double _value = 40;

    public double Value { get => _value; set => SetProperty(ref _value, value); }

    public string ValueCode =>
        "<Slider Minimum=\"0\" Maximum=\"100\" Value=\"{Binding Value}\" />";

    public string TicksCode =>
        "<Slider Minimum=\"0\" Maximum=\"10\"\n" +
        "        TickFrequency=\"1\" TickPlacement=\"BottomRight\"\n" +
        "        IsSnapToTickEnabled=\"True\" />";
}
