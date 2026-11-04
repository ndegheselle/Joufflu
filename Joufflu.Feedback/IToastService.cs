using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Assets.Fonts;

namespace Joufflu.Feedback;

/// <summary>
/// Shows toasts. A <see cref="Controls.ToastContainer"/> bound to the same <see cref="ToastService"/>
/// renders them, stacked above the rest of the UI.
/// </summary>
public interface IToastService
{
    ToastInstance Show(ToastOptions options);

    ToastInstance Info(string message, string title = "");

    ToastInstance Success(string message, string title = "");

    ToastInstance Warning(string message, string title = "");

    ToastInstance Error(string message, string title = "");

    /// <summary>
    /// Sticky toast with a spinner. Update the returned instance once the work is done
    /// (e.g. <see cref="ToastInstance.Type"/> and <see cref="ToastInstance.Duration"/>).
    /// </summary>
    ToastInstance Loading(string message, string title = "");

    void Close(ToastInstance toast);
}

public enum ToastType
{
    Info,
    Success,
    Warning,
    Danger,
    Loading
}

/// <summary>Description of a toast to display.</summary>
public class ToastOptions
{
    public ToastType Type { get; set; } = ToastType.Info;

    public string Title { get; set; } = "";

    public string Message { get; set; } = "";

    /// <summary>Time before the toast auto-dismisses. <see cref="TimeSpan.Zero"/> keeps it sticky.</summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// A live toast hosted by the <see cref="ToastService"/>. Its type, texts and duration can be
/// changed while it is displayed.
/// </summary>
public class ToastInstance : ObservableObject
{
    private readonly ToastService _service;
    private readonly DispatcherTimer _timer = new();

    private ToastType _type;
    public ToastType Type
    {
        get => _type;
        set
        {
            if (SetProperty(ref _type, value))
                OnPropertyChanged(nameof(Icon));
        }
    }

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
                OnPropertyChanged(nameof(HasTitle));
        }
    }

    public bool HasTitle => !string.IsNullOrEmpty(Title);

    private string _message;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private TimeSpan _duration;
    /// <summary>
    /// Time before the toast auto-dismisses, counted from the moment it is set.
    /// <see cref="TimeSpan.Zero"/> keeps it sticky.
    /// </summary>
    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            if (SetProperty(ref _duration, value))
                StartTimer();
        }
    }

    /// <summary>Lucide glyph representing the toast type (a loading toast shows a spinner instead).</summary>
    public string Icon => Type switch
    {
        ToastType.Success => LucideFontIcons.BadgeCheck,
        ToastType.Warning => LucideFontIcons.BadgeAlert,
        ToastType.Danger => LucideFontIcons.CircleX,
        _ => LucideFontIcons.Info
    };

    public ICommand CloseCommand { get; }

    public ToastInstance(ToastOptions options, ToastService service)
    {
        _service = service;
        _type = options.Type;
        _title = options.Title;
        _message = options.Message;
        _duration = options.Duration;

        CloseCommand = new RelayCommand(() => _service.Close(this));
        _timer.Tick += OnTick;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _service.Close(this);
    }

    internal void StartTimer()
    {
        _timer.Stop();
        if (Duration <= TimeSpan.Zero)
            return;

        _timer.Interval = Duration;
        _timer.Start();
    }

    internal void StopTimer()
    {
        _timer.Stop();
    }
}

/// <summary>
/// Default <see cref="IToastService"/> implementation. Newest toast is inserted on top.
/// </summary>
public class ToastService : ObservableObject, IToastService
{
    public ObservableCollection<ToastInstance> Toasts { get; } = new();

    public ToastInstance Show(ToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var instance = new ToastInstance(options, this);
        instance.StartTimer();
        Toasts.Insert(0, instance);
        return instance;
    }

    public ToastInstance Info(string message, string title = "")
        => Show(new ToastOptions { Type = ToastType.Info, Message = message, Title = title });

    public ToastInstance Success(string message, string title = "")
        => Show(new ToastOptions { Type = ToastType.Success, Message = message, Title = title });

    public ToastInstance Warning(string message, string title = "")
        => Show(new ToastOptions { Type = ToastType.Warning, Message = message, Title = title });

    public ToastInstance Error(string message, string title = "")
        => Show(new ToastOptions { Type = ToastType.Danger, Message = message, Title = title });

    public ToastInstance Loading(string message, string title = "")
        => Show(new ToastOptions { Type = ToastType.Loading, Message = message, Title = title, Duration = TimeSpan.Zero });

    public void Close(ToastInstance toast)
    {
        toast.StopTimer();
        Toasts.Remove(toast);
    }
}
