using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Navigation.Controls;

/// <summary>
/// Options describing an overlay's chrome (title, close affordances).
/// The overlay content is responsible for rendering its own action buttons.
/// </summary>
public class OverlayOptions : ObservableObject
{
    public string Title { get; set; } = "";

    /// <summary>Shows the close cross in the title bar.</summary>
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>Closes the overlay when the dimmed background behind it is clicked.</summary>
    public bool CloseOnClickAway { get; set; } = true;
}

/// <summary>
/// A live overlay sitting on the <see cref="OverlayService"/> stack.
/// </summary>
public class OverlayInstance : ObservableObject
{
    private readonly OverlayService _service;

    public object Content { get; }

    public OverlayOptions Options { get; }

    /// <summary>Closes the overlay with a <see langword="null"/> (dismissed) result.</summary>
    public ICommand CloseCommand { get; }

    /// <summary>Closes the overlay only when <see cref="OverlayOptions.CloseOnClickAway"/> is set.</summary>
    public ICommand ClickAwayCommand { get; }

    internal TaskCompletionSource<bool?> Completion { get; } = new();

    public OverlayInstance(object content, OverlayOptions options, OverlayService service)
    {
        Content = content;
        Options = options;
        _service = service;

        CloseCommand = new RelayCommand(() => Close(null));
        ClickAwayCommand = new RelayCommand(() =>
        {
            if (Options.CloseOnClickAway)
                Close(null);
        });
    }

    public void Close(bool? result) => _service.Close(this, result);
}

/// <summary>
/// Default <see cref="IOverlayService"/> implementation: a stack of modal overlays.
/// </summary>
public class OverlayService : ObservableObject, IOverlayService
{
    public ObservableCollection<OverlayInstance> Overlays { get; } = new();

    public bool HasOverlays => Overlays.Count > 0;

    public Task<bool?> Show(object content, OverlayOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        options ??= (content as IOverlayContent)?.Options ?? new OverlayOptions();
        var instance = new OverlayInstance(content, options, this);

        Overlays.Add(instance);
        OnPropertyChanged(nameof(HasOverlays));
        (content as IPage)?.OnNavigatedTo();

        return instance.Completion.Task;
    }

    public void Close(OverlayInstance overlay, bool? result = null)
    {
        if (!Overlays.Remove(overlay))
            return;

        (overlay.Content as IPage)?.OnNavigatedFrom();
        overlay.Completion.TrySetResult(result);
        OnPropertyChanged(nameof(HasOverlays));
    }

    public void CloseTop(bool? result = null)
    {
        if (Overlays.Count > 0)
            Close(Overlays[^1], result);
    }
}
