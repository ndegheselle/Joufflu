using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

/// <summary>An <see cref="ObservableValidator"/> so the validation sample has errors to report.</summary>
public class NumericInputsSamplesViewModel : ObservableValidator
{
    private long? _numericValue = 42;
    private decimal? _decimalValue = 3.14m;
    private TimeSpan? _duration = new TimeSpan(0, 1, 30, 0);
    private long? _rating = 42;
    private decimal? _ratio = 3.14m;
    private TimeSpan? _shortDuration = new TimeSpan(0, 1, 30, 0);

    public NumericInputsSamplesViewModel() => ValidateAllProperties();

    public long? NumericValue { get => _numericValue; set => SetProperty(ref _numericValue, value); }

    public decimal? DecimalValue { get => _decimalValue; set => SetProperty(ref _decimalValue, value); }

    public TimeSpan? Duration { get => _duration; set => SetProperty(ref _duration, value); }

    [Range(1, 10, ErrorMessage = "A rating goes from 1 to 10.")]
    public long? Rating { get => _rating; set => SetProperty(ref _rating, value, validate: true); }

    [Range(typeof(decimal), "0", "1", ErrorMessage = "A ratio goes from 0 to 1.")]
    public decimal? Ratio { get => _ratio; set => SetProperty(ref _ratio, value, validate: true); }

    [Range(typeof(TimeSpan), "00:00:00", "01:00:00", ErrorMessage = "At most one hour.")]
    public TimeSpan? ShortDuration { get => _shortDuration; set => SetProperty(ref _shortDuration, value, validate: true); }

    public string NumericCode =>
        "<inputs:NumericUpDown Value=\"{Binding NumericValue, Mode=TwoWay}\" />";

    public string DecimalCode =>
        "<inputs:DecimalUpDown Value=\"{Binding DecimalValue, Mode=TwoWay}\" />";

    public string TimeSpanCode =>
        "<inputs:TimeSpanPicker Value=\"{Binding Duration, Mode=TwoWay}\" />";

    public string FormatCode =>
        "<format:FormatTextBox Format=\"{}{max:23}h {max:59}m {max:59}s\" GlobalFormat=\"numeric\" />";

    public string ValidationCode =>
        "<inputs:NumericUpDown Value=\"{Binding Rating, Mode=TwoWay}\" />\n" +
        "<inputs:DecimalUpDown Value=\"{Binding Ratio, Mode=TwoWay}\" />\n" +
        "<inputs:TimeSpanPicker Value=\"{Binding ShortDuration, Mode=TwoWay}\" />\n\n" +
        "<!-- Opt out of the template: only the input's own red border remains -->\n" +
        "<inputs:NumericUpDown Validation.ErrorTemplate=\"{x:Null}\" Value=\"{Binding Rating, Mode=TwoWay}\" />";
}
