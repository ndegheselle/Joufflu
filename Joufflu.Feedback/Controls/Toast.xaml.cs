using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Joufflu.Feedback.Controls;

/// <summary>
/// Fills a toast's progress bar over its <see cref="Joufflu.Feedback.ToastInstance.Duration"/>, so the
/// bar shows how close the auto-dismiss is. Restarted whenever the duration changes; a zero duration
/// (a sticky toast) leaves the bar full.
/// </summary>
public static class ToastProgress
{
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.RegisterAttached(
            "Duration",
            typeof(TimeSpan),
            typeof(ToastProgress),
            new PropertyMetadata(TimeSpan.Zero, OnDurationChanged));

    public static void SetDuration(DependencyObject d, TimeSpan value) => d.SetValue(DurationProperty, value);
    public static TimeSpan GetDuration(DependencyObject d) => (TimeSpan)d.GetValue(DurationProperty);

    private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ProgressBar bar) return;

        var duration = (TimeSpan)e.NewValue;

        if (duration <= TimeSpan.Zero)
        {
            bar.BeginAnimation(ProgressBar.ValueProperty, null);
            bar.Value = bar.Maximum;
            return;
        }

        var animation = new DoubleAnimation
        {
            From = 0,
            To = bar.Maximum,
            Duration = new Duration(duration),
            FillBehavior = FillBehavior.HoldEnd
        };

        bar.BeginAnimation(ProgressBar.ValueProperty, animation);
    }
}