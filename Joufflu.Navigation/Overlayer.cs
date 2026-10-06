using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Navigation.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Joufflu.Navigation;

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

    /// <summary>Stretches the overlay to fill the whole surface instead of a centered, sized panel.</summary>
    public bool FullScreen { get; set; } = false;
}

/// <summary>
/// Hosts a stack of modal overlays on top of the current page.
/// </summary>
public interface IOverlayer
{
    /// <summary>
    /// Shows <paramref name="content"/> as a modal overlay and completes when it is closed.
    /// The result carries whatever the closing action provided (<see langword="null"/> when dismissed).
    /// </summary>
    Task<bool?> Show(object content, OverlayOptions? options = null);

    /// <summary>
    /// Show a confirmation overlay with a simple message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<bool?> Confirm(string message, string title = "", EnumConfirmationType type = EnumConfirmationType.Neutral);

    void Close(OverlayInstance overlay, bool? result = null);

    /// <summary>
    /// Closes the overlay showing <paramref name="content"/>, doing nothing when it isn't on the
    /// stack anymore. Lets content close itself rather than whatever is on top, which is what an
    /// overlay opening another one of its own kind needs.
    /// </summary>
    void Close(object content, bool? result = null);

    void CloseTop(bool? result = null);
}

/// <summary>
/// Optional contract for overlay content that wants to provide its own options
/// (title, action bar, ...) instead of having them supplied at <see cref="IOverlayer.Show"/> time.
/// </summary>
public interface IOverlayContent : IPage
{
    OverlayOptions Options { get; }
}

/// <summary>
/// A live overlay sitting on the <see cref="Overlayer"/> stack.
/// </summary>
public class OverlayInstance : ObservableObject
{
    private readonly Overlayer _service;

    public object Content { get; }

    public OverlayOptions Options { get; }

    /// <summary>Closes the overlay with a <see langword="null"/> (dismissed) result.</summary>
    public ICommand CloseCommand { get; }

    /// <summary>Closes the overlay only when <see cref="OverlayOptions.CloseOnClickAway"/> is set.</summary>
    public ICommand ClickAwayCommand { get; }

    internal TaskCompletionSource<bool?> Completion { get; } = new();

    public OverlayInstance(object content, OverlayOptions options, Overlayer service)
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

public class ConfirmationContent : OverlayOptions
{
    public string Message { get; set; } = "";
    public EnumConfirmationType Type { get; set; }

    public IRelayCommand CancelCommand { get; }
    public IRelayCommand ConfirmCommand { get; }

    public ConfirmationContent(IOverlayer overlays, string message, EnumConfirmationType type)
    {
        Message = message;
        Type = type;
        CancelCommand = new RelayCommand(() => overlays.CloseTop(false));
        ConfirmCommand = new RelayCommand(() => overlays.CloseTop(true));
    }
}

/// <summary>
/// Default <see cref="IOverlayer"/> implementation: a stack of modal overlays.
/// </summary>
public class Overlayer : ObservableObject, IOverlayer
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

    public Task<bool?> Confirm(string message, string title = "", EnumConfirmationType type = EnumConfirmationType.Neutral)
    {
        return Show(new ConfirmationContent(this, message, type), new OverlayOptions() { Title = title });
    }

    public void Close(OverlayInstance overlay, bool? result = null)
    {
        if (!Overlays.Remove(overlay))
            return;

        (overlay.Content as IPage)?.OnNavigatedFrom();
        overlay.Completion.TrySetResult(result);
        OnPropertyChanged(nameof(HasOverlays));
    }

    public void Close(object content, bool? result = null)
    {
        OverlayInstance? overlay = Overlays.FirstOrDefault(x => ReferenceEquals(x.Content, content));
        if (overlay != null)
            Close(overlay, result);
    }

    public void CloseTop(bool? result = null)
    {
        if (Overlays.Count > 0)
            Close(Overlays[^1], result);
    }
}