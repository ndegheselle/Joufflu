using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class NumericInputsSamplesViewModel : ObservableObject
{
    private int _numericValue = 42;
    private decimal _decimalValue = 3.14m;
    private TimeSpan? _duration = new TimeSpan(0, 1, 30, 0);

    public int NumericValue { get => _numericValue; set => SetProperty(ref _numericValue, value); }

    public decimal DecimalValue { get => _decimalValue; set => SetProperty(ref _decimalValue, value); }

    public TimeSpan? Duration { get => _duration; set => SetProperty(ref _duration, value); }

    public string NumericCode =>
        "<inputs:NumericUpDown Value=\"{Binding NumericValue, Mode=TwoWay}\" />";

    public string DecimalCode =>
        "<inputs:DecimalUpDown Value=\"{Binding DecimalValue, Mode=TwoWay}\" />";

    public string TimeSpanCode =>
        "<inputs:TimeSpanPicker Value=\"{Binding Duration, Mode=TwoWay}\" />";

    public string FormatCode =>
        "<format:FormatTextBox Format=\"{}{max:23}h {max:59}m {max:59}s\" />";
}
