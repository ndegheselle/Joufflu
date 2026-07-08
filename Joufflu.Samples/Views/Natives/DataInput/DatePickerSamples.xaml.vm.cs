using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.DataInput;

public class DatePickerSamplesViewModel : ObservableObject
{
    private DateTime? _selectedDate = DateTime.Today;

    public DateTime? SelectedDate { get => _selectedDate; set => SetProperty(ref _selectedDate, value); }

    public string Code => "<DatePicker SelectedDate=\"{Binding SelectedDate}\" />";
}
