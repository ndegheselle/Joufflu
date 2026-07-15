using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Navigation;

namespace Joufflu.Samples.ViewModels;

/// <summary>Simple overlay content showing a message.</summary>
public class ConfirmViewModel : ObservableObject
{
    public ConfirmViewModel(string message) => Message = message;

    public string Message { get; }
}

/// <summary>
/// Overlay content that renders its own confirm/cancel buttons and closes the overlay itself.
/// Shows how the caller now owns the action bar instead of <see cref="OverlayOptions"/>.
/// </summary>
public class DeleteConfirmViewModel : ObservableObject
{
    private readonly IOverlayService _overlays;

    public DeleteConfirmViewModel(IOverlayService overlays, string message)
    {
        _overlays = overlays;
        Message = message;
        CancelCommand = new RelayCommand(() => _overlays.CloseTop(false));
        DeleteCommand = new RelayCommand(() => _overlays.CloseTop(true));
    }

    public string Message { get; }

    public IRelayCommand CancelCommand { get; }

    public IRelayCommand DeleteCommand { get; }
}

/// <summary>Overlay content with an editable field, used by the form overlay demo.</summary>
public class SampleFormViewModel : ObservableObject
{
    private readonly IOverlayService _overlays;
    private string _name = "Ada Lovelace";
    private bool _subscribe = true;

    public SampleFormViewModel(IOverlayService overlays)
    {
        _overlays = overlays;
        CancelCommand = new RelayCommand(() => _overlays.CloseTop(false));
        SaveCommand = new RelayCommand(() => _overlays.CloseTop(true));
    }

    public string Name { get => _name; set => SetProperty(ref _name, value); }

    public bool Subscribe { get => _subscribe; set => SetProperty(ref _subscribe, value); }

    public IRelayCommand CancelCommand { get; }

    public IRelayCommand SaveCommand { get; }
}
