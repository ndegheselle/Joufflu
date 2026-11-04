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
    /// Shows <paramref name="content"/> as a modal overlay and completes when it is closed, with how it was closed:
    /// true when validated, false when cancelled, null when ignored.
    /// </summary>
    Task<bool?> ShowAsync(object content, OverlayOptions? options = null);

    /// <summary>
    /// Shows <paramref name="content"/> and completes with its <c>Result</c> when it is validated, the default of
    /// <typeparamref name="TResult"/> when cancelled or ignored.
    /// </summary>
    Task<TResult?> ShowAsync<TResult>(IOverlayContent<TResult> content, OverlayOptions? options = null);

    /// <summary>
    /// Shows the built-in confirmation: <paramref name="message"/> above a Cancel / Confirm pair, the confirm button
    /// coloured by <paramref name="type"/>. Completes with true when confirmed, false when cancelled, null when ignored.
    /// </summary>
    Task<bool?> Confirm(string message, string title = "", EnumConfirmationType type = EnumConfirmationType.Neutral);

    /// <summary> Close [content] by ignoring it (null returned).</summary>
    void Ignore(object content);
    /// <summary> Close [content] by canceling it (false returned).</summary>
    void Cancel(object content);
    /// <summary> Close [content] by validating it (true returned).</summary>
    void Validate(object content);
}

/// <summary>
/// Optional contract for overlay content that wants to provide its own options
/// (title, close cross, click away, ...) instead of having them supplied to <c>IOverlayer.ShowAsync</c>.
/// </summary>
public interface IOverlayContent : IPage
{
    OverlayOptions Options { get; }
}

/// <summary>
/// Optional contract for overlay content that hands a value back to its caller when validated.
/// </summary>
public interface IOverlayContent<TResult> : IOverlayContent
{
    /// <summary>What the generic <c>IOverlayer.ShowAsync</c> returns once the content is validated.</summary>
    TResult? Result { get; }
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
    public ICommand IgnoreCommand { get; }

    /// <summary>Closes the overlay only when <see cref="OverlayOptions.CloseOnClickAway"/> is set.</summary>
    public ICommand ClickAwayCommand { get; }

    internal TaskCompletionSource<bool?> Completion { get; } = new();

    public OverlayInstance(object content, OverlayOptions options, Overlayer service)
    {
        Content = content;
        Options = options;
        _service = service;

        IgnoreCommand = new RelayCommand(() => _service.Ignore(Content));
        ClickAwayCommand = new RelayCommand(() =>
        {
            if (Options.CloseOnClickAway)
                _service.Ignore(Content);
        });
    }
}

/// <summary>
/// Content of the overlay <see cref="IOverlayer.Confirm"/> shows, rendered by an implicit template of the library.
/// </summary>
public class ConfirmationContent: IOverlayContent
{
    public OverlayOptions Options { get; }
    public string Message { get; set; } = "";
    public EnumConfirmationType Type { get; set; }

    public IRelayCommand CancelCommand { get; }
    public IRelayCommand ConfirmCommand { get; }


    public ConfirmationContent(IOverlayer overlays, string title, string message, EnumConfirmationType type)
    {
        Options = new OverlayOptions() { Title = title };
        Message = message;
        Type = type;
        CancelCommand = new RelayCommand(() => overlays.Cancel(this));
        ConfirmCommand = new RelayCommand(() => overlays.Validate(this));
    }
}

/// <summary>
/// Default <see cref="IOverlayer"/> implementation: a stack of modal overlays.
/// </summary>
public class Overlayer : ObservableObject, IOverlayer
{
    public ObservableCollection<OverlayInstance> Overlays { get; } = new();

    public bool HasOverlays => Overlays.Count > 0;

    /// <inheritdoc/>
    public Task<bool?> ShowAsync(object content, OverlayOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        options ??= (content as IOverlayContent)?.Options ?? new OverlayOptions();
        var instance = new OverlayInstance(content, options, this);

        Overlays.Add(instance);
        OnPropertyChanged(nameof(HasOverlays));
        (content as IPage)?.OnNavigatedTo();

        return instance.Completion.Task;
    }

    /// <inheritdoc/>
    public async Task<TResult?> ShowAsync<TResult>(IOverlayContent<TResult> content, OverlayOptions? options = null)
    {
        return await ShowAsync((object)content, options) == true ? content.Result : default;
    }

    public Task<bool?> Confirm(string message, string title = "", EnumConfirmationType type = EnumConfirmationType.Neutral)
    {
        return ShowAsync(new ConfirmationContent(this, title, message, type));
    }

    public void Cancel(object content)
    {
        Close(content, false);
    }
    public void Validate(object content)
    {
        Close(content, true);
    }
    public void Ignore(object content)
    {
        Close(content, null);
    }

    private void Close(object content, bool? result = null)
    {
        OverlayInstance? overlay = Overlays.FirstOrDefault(x => ReferenceEquals(x.Content, content));
        if (overlay != null)
            Close(overlay, result);
    }

    private void Close(OverlayInstance overlay, bool? result = null)
    {
        if (!Overlays.Remove(overlay))
            return;

        (overlay.Content as IPage)?.OnNavigatedFrom();
        overlay.Completion.TrySetResult(result);
        OnPropertyChanged(nameof(HasOverlays));
    }
}