using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Controls;
using Joufflu.Navigation;
using Joufflu.Navigation.Controls;
using Joufflu.Samples.ViewModels;

namespace Joufflu.Samples.Views.Controls.Navigation;

public class OverlaySamplesViewModel : ObservableObject
{
    private readonly IOverlayService _overlays;
    private readonly IToastService _toasts;

    public IRelayCommand OpenSimpleCommand { get; }

    public IRelayCommand OpenConfirmCommand { get; }

    public IRelayCommand OpenFormCommand { get; }

    public IRelayCommand OpenStackedCommand { get; }

    public OverlaySamplesViewModel(IOverlayService overlays, IToastService toasts)
    {
        _overlays = overlays;
        _toasts = toasts;

        OpenSimpleCommand = new RelayCommand(OpenSimple);
        OpenConfirmCommand = new AsyncRelayCommand(OpenConfirmAsync);
        OpenFormCommand = new AsyncRelayCommand(OpenFormAsync);
        OpenStackedCommand = new RelayCommand(OpenStacked);
    }

    private void OpenSimple()
    {
        var content = new ConfirmViewModel("This is a simple overlay. Click the cross or the dimmed background to dismiss it.");
        _overlays.Show(content, new OverlayOptions { Title = "Simple overlay" });
    }

    private async Task OpenConfirmAsync()
    {
        var content = new DeleteConfirmViewModel(_overlays, "Delete the selected item? This action cannot be undone.");
        var options = new OverlayOptions { Title = "Please confirm", CloseOnClickAway = false };

        bool? result = await _overlays.Show(content, options);
        if (result == true)
            _toasts.Success("Item deleted.", "Confirmed");
        else
            _toasts.Info("Cancelled.");
    }

    private async Task OpenFormAsync()
    {
        var form = new SampleFormViewModel(_overlays);
        var options = new OverlayOptions { Title = "Edit profile", CloseOnClickAway = false };

        bool? result = await _overlays.Show(form, options);
        if (result == true)
            _toasts.Success($"Saved name: {form.Name}", "Profile");
    }

    private void OpenStacked()
    {
        _overlays.Show(
            new ConfirmViewModel("First overlay. Open another one on top to see overlays stack."),
            new OverlayOptions { Title = "Overlay 1" });

        _overlays.Show(
            new ConfirmViewModel("Second overlay, stacked above the first. Close me to reveal it."),
            new OverlayOptions { Title = "Overlay 2" });
    }

    public string Code =>
        "// The overlay content owns its buttons and closes itself\n" +
        "// via the service, e.g. overlays.CloseTop(true/false).\n" +
        "var content = new DeleteConfirmViewModel(overlays, \"Delete?\");\n" +
        "var options = new OverlayOptions { Title = \"Please confirm\" };\n" +
        "bool? result = await overlays.Show(content, options);";
}
