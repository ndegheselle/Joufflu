using System.Diagnostics;

namespace Joufflu.Data.Tests;

/// <summary>
/// Records the binding failures WPF traces while it is alive. They never throw: without a
/// listener they only show up in the debugger's output.
/// </summary>
public sealed class BindingErrors : TraceListener
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages => _messages;

    public BindingErrors()
    {
        PresentationTraceSources.Refresh();
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
        PresentationTraceSources.DataBindingSource.Listeners.Add(this);
    }

    public void Clear() => _messages.Clear();

    public override void Write(string? message) { }

    public override void WriteLine(string? message)
    {
        if (message != null)
            _messages.Add(message);
    }

    protected override void Dispose(bool disposing)
    {
        PresentationTraceSources.DataBindingSource.Listeners.Remove(this);
        base.Dispose(disposing);
    }
}
