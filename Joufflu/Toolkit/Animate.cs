using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Joufflu.Toolkit;

/// <summary>
/// Attached properties that animate any <see cref="UIElement"/> without touching its template.
/// <para>
/// Set <see cref="BounceProperty"/> to <c>true</c> for a looping vertical hop — useful to draw
/// attention to an element (a notification badge, a call-to-action button). Because it is a plain
/// boolean it can be bound, so the hop starts and stops with your view model:
/// <c>toolkit:Animate.Bounce="{Binding HasUnread}"</c>. Tune the hop with
/// <see cref="BounceHeightProperty"/> and <see cref="BounceDurationProperty"/>.
/// </para>
/// <para>
/// Set <see cref="SpinProperty"/> to <c>true</c> for an endless turn around the element's own
/// centre — a loading glyph, a refresh icon while it works. <see cref="SpinDurationProperty"/>
/// sets the time of one full turn.
/// </para>
/// <para>
/// Both run on transforms added to the element's <see cref="UIElement.RenderTransform"/>, so they
/// never affect layout and never move its neighbours, and they can run together on the same
/// element. Any transform already set is kept and composed with them, and restored once both
/// have stopped.
/// </para>
/// <para>
/// The animations pause themselves whenever the element is not visible — collapsed or hidden,
/// under a collapsed ancestor, or out of the visual tree — and resume when it comes back, so an
/// animation left on a hidden element costs nothing. Nothing needs to be unhooked to let the
/// element be collected either, whether the animation was stopped first or not.
/// </para>
/// </summary>
public static class Animate
{
    /// <summary>Whether the element hops continuously. Bindable: <c>false</c> stops the loop and restores the element.</summary>
    public static readonly DependencyProperty BounceProperty =
        DependencyProperty.RegisterAttached(
            "Bounce",
            typeof(bool),
            typeof(Animate),
            new PropertyMetadata(false, OnBounceChanged));

    public static bool GetBounce(DependencyObject obj) => (bool)obj.GetValue(BounceProperty);
    public static void SetBounce(DependencyObject obj, bool value) => obj.SetValue(BounceProperty, value);

    /// <summary>
    /// Height of the first hop in DIPs (defaults to 6). The two rebounds that follow it are
    /// fractions of this height, so the whole settle scales with it.
    /// </summary>
    public static readonly DependencyProperty BounceHeightProperty =
        DependencyProperty.RegisterAttached(
            "BounceHeight",
            typeof(double),
            typeof(Animate),
            new PropertyMetadata(6d, OnBounceSettingChanged));

    public static double GetBounceHeight(DependencyObject obj) => (double)obj.GetValue(BounceHeightProperty);
    public static void SetBounceHeight(DependencyObject obj, double value) => obj.SetValue(BounceHeightProperty, value);

    /// <summary>
    /// Length of one full cycle — the hop and its rebounds plus the pause before the next one
    /// (defaults to 1s). A shorter duration reads as impatient, a longer one as a slow idle nudge.
    /// </summary>
    public static readonly DependencyProperty BounceDurationProperty =
        DependencyProperty.RegisterAttached(
            "BounceDuration",
            typeof(Duration),
            typeof(Animate),
            new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1)), OnBounceSettingChanged));

    public static Duration GetBounceDuration(DependencyObject obj) => (Duration)obj.GetValue(BounceDurationProperty);
    public static void SetBounceDuration(DependencyObject obj, Duration value) => obj.SetValue(BounceDurationProperty, value);

    /// <summary>Whether the element turns continuously around its centre. Bindable: <c>false</c> stops it and restores the element.</summary>
    public static readonly DependencyProperty SpinProperty =
        DependencyProperty.RegisterAttached(
            "Spin",
            typeof(bool),
            typeof(Animate),
            new PropertyMetadata(false, OnSpinChanged));

    public static bool GetSpin(DependencyObject obj) => (bool)obj.GetValue(SpinProperty);
    public static void SetSpin(DependencyObject obj, bool value) => obj.SetValue(SpinProperty, value);

    /// <summary>Time of one full clockwise turn (defaults to 1s).</summary>
    public static readonly DependencyProperty SpinDurationProperty =
        DependencyProperty.RegisterAttached(
            "SpinDuration",
            typeof(Duration),
            typeof(Animate),
            new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1)), OnSpinSettingChanged));

    public static Duration GetSpinDuration(DependencyObject obj) => (Duration)obj.GetValue(SpinDurationProperty);
    public static void SetSpinDuration(DependencyObject obj, Duration value) => obj.SetValue(SpinDurationProperty, value);

    /// <summary>
    /// The transforms the animations drive, the transform they replaced, and the clocks driving
    /// them. Kept per-element so each animation can be paused or removed on its own, and the
    /// original <see cref="UIElement.RenderTransform"/> put back once none is left.
    /// </summary>
    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached(
            "State",
            typeof(AnimationState),
            typeof(Animate),
            new PropertyMetadata(null));

    private sealed class AnimationState(Transform? original)
    {
        public Transform? Original { get; } = original;

        public RotateTransform Rotate { get; } = new();

        public TranslateTransform Translate { get; } = new();

        /// <summary>Replaced whenever the hop is rebuilt, so only the current clock is controlled.</summary>
        public AnimationClock? BounceClock { get; set; }

        /// <summary>Replaced whenever the turn is rebuilt, so only the current clock is controlled.</summary>
        public AnimationClock? SpinClock { get; set; }

        public bool IsIdle => BounceClock is null && SpinClock is null;
    }

    private static void OnBounceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
            StartBounce(element);
        else
            StopBounce(element);
    }

    // Height and duration are baked into the key frames, so a change while bouncing needs a rebuild.
    private static void OnBounceSettingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && GetBounce(element))
            StartBounce(element);
    }

    private static void OnSpinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
            StartSpin(element);
        else
            StopSpin(element);
    }

    // The duration is baked into the animation, so a change while spinning needs a rebuild.
    private static void OnSpinSettingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && GetSpin(element))
            StartSpin(element);
    }

    private static void StartBounce(UIElement element)
    {
        AnimationState state = Attach(element);
        state.BounceClock = Run(element, state.Translate, TranslateTransform.YProperty, BuildBounce(element));
    }

    private static void StopBounce(UIElement element)
    {
        if (element.GetValue(StateProperty) is not AnimationState state)
            return;

        // Passing null removes the animation and lets the property fall back to its local value (0).
        state.Translate.ApplyAnimationClock(TranslateTransform.YProperty, null);
        state.BounceClock = null;
        DetachIfIdle(element, state);
    }

    private static void StartSpin(UIElement element)
    {
        AnimationState state = Attach(element);
        UpdateSpinCentre(element, state);
        state.SpinClock = Run(element, state.Rotate, RotateTransform.AngleProperty, BuildSpin(element));
    }

    private static void StopSpin(UIElement element)
    {
        if (element.GetValue(StateProperty) is not AnimationState state)
            return;

        state.Rotate.ApplyAnimationClock(RotateTransform.AngleProperty, null);
        state.SpinClock = null;
        DetachIfIdle(element, state);
    }

    /// <summary>
    /// Swaps the element's transform for a group holding its original transform, then the turn,
    /// then the hop — so the element spins as it is drawn, and the hop moves it without moving the
    /// centre it spins around. Shared by both animations; a no-op once installed.
    /// </summary>
    private static AnimationState Attach(UIElement element)
    {
        if (element.GetValue(StateProperty) is AnimationState state)
            return state;

        Transform? original = element.RenderTransform;
        // Transform.Identity is the default and is frozen, so treat it as "no transform" rather
        // than composing with it; anything else is kept so the animation doesn't drop a rotation
        // or scale the element already had.
        if (original == Transform.Identity)
            original = null;

        state = new AnimationState(original);
        var group = new TransformGroup();
        // A transform declared in XAML can be frozen; clone it before putting it in a group.
        if (original is not null)
            group.Children.Add(original.IsFrozen ? original.Clone() : original);
        group.Children.Add(state.Rotate);
        group.Children.Add(state.Translate);
        element.RenderTransform = group;

        element.SetValue(StateProperty, state);
        // Only while the element is on screen: an animation nobody can see is wasted frames, and
        // nothing else ever stops it — it repeats forever. IsVisible covers Collapsed/Hidden, a
        // collapsed ancestor and being out of the tree altogether, in one flag. The handlers are
        // static methods, so subscribing roots nothing.
        element.IsVisibleChanged += OnIsVisibleChanged;
        if (element is FrameworkElement framework)
            framework.SizeChanged += OnSizeChanged;

        return state;
    }

    private static void DetachIfIdle(UIElement element, AnimationState state)
    {
        if (!state.IsIdle)
            return;

        element.IsVisibleChanged -= OnIsVisibleChanged;
        if (element is FrameworkElement framework)
            framework.SizeChanged -= OnSizeChanged;
        element.RenderTransform = state.Original ?? Transform.Identity;
        element.SetValue(StateProperty, null);
    }

    // A clock of our own (rather than BeginAnimation) is what makes the animation pausable.
    private static AnimationClock Run(UIElement element, Animatable target, DependencyProperty property, AnimationTimeline animation)
    {
        AnimationClock clock = animation.CreateClock();
        target.ApplyAnimationClock(property, clock);

        // An element gets its first IsVisibleChanged only once it is rendered, so an animation set
        // in XAML starts out invisible and is resumed by the handler below.
        if (!element.IsVisible)
            clock.Controller?.Pause();

        return clock;
    }

    // Hidden, collapsed, or no longer in the tree: hold the clocks rather than let them tick
    // against nothing. They start again from rest when the element comes back.
    private static void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not UIElement element || element.GetValue(StateProperty) is not AnimationState state)
            return;

        bool visible = (bool)e.NewValue;
        Toggle(state.BounceClock?.Controller, visible);
        Toggle(state.SpinClock?.Controller, visible);

        static void Toggle(ClockController? controller, bool visible)
        {
            if (controller is null)
                return;

            if (visible)
            {
                controller.Resume();
            }
            else
            {
                controller.Pause();
                // Park it at rest rather than mid-air: the element would otherwise come back
                // offset for a frame before the animation brings it back.
                controller.Seek(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            }
        }
    }

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is UIElement element && element.GetValue(StateProperty) is AnimationState state)
            UpdateSpinCentre(element, state);
    }

    /// <summary>
    /// Points the turn at the middle of the element, whatever its
    /// <see cref="UIElement.RenderTransformOrigin"/>: the whole transform is drawn relative to that
    /// origin, so the centre is offset by it, then carried through the original transform the
    /// turn is composed after. Kept current on resize since the middle moves with the size.
    /// </summary>
    private static void UpdateSpinCentre(UIElement element, AnimationState state)
    {
        Size size = element.RenderSize;
        Point origin = element.RenderTransformOrigin;
        var centre = new Point(size.Width * (0.5 - origin.X), size.Height * (0.5 - origin.Y));
        if (state.Original is not null)
            centre = state.Original.Value.Transform(centre);

        state.Rotate.CenterX = centre.X;
        state.Rotate.CenterY = centre.Y;
    }

    /// <summary>A full clockwise turn at constant speed, repeated forever, so the loop has no seam.</summary>
    private static DoubleAnimation BuildSpin(UIElement element)
    {
        Duration duration = GetSpinDuration(element);
        // A turn can be set to Automatic/Forever, which would never complete one revolution.
        TimeSpan turn = duration.HasTimeSpan ? duration.TimeSpan : TimeSpan.FromSeconds(1);

        return new DoubleAnimation(0, 360, turn) { RepeatBehavior = RepeatBehavior.Forever };
    }

    /// <summary>
    /// Height of each rebound as a fraction of <see cref="BounceHeightProperty"/>. A ball loses
    /// the same share of its energy on every impact, so the heights fall off geometrically —
    /// the second rebound is to the first what the first is to the hop.
    /// </summary>
    private static readonly double[] ReboundHeights = [0.42, 0.18];

    /// <summary>
    /// Key times of the settle, as fractions of the cycle: the element rises to the full height
    /// and lands (0 → 0.48), then rebounds twice, each shorter than the last because a lower hop
    /// spends less time in the air. What is left of the cycle is the idle tail.
    /// </summary>
    private static readonly double[] ReboundPeakTimes = [0.58, 0.74];
    private static readonly double[] ReboundLandTimes = [0.68, 0.80];

    /// <summary>
    /// Up, down, twice again smaller, then wait. Each rise decelerates into its peak
    /// (<c>EaseOut</c>) and each fall accelerates into the landing (<c>EaseIn</c>), so the element
    /// reads as settling under gravity rather than pulsing; it then holds still for the rest of
    /// the cycle.
    /// </summary>
    private static DoubleAnimationUsingKeyFrames BuildBounce(UIElement element)
    {
        Duration duration = GetBounceDuration(element);
        // A cycle can be set to Automatic/Forever, which has no TimeSpan to take fractions of.
        TimeSpan cycle = duration.HasTimeSpan ? duration.TimeSpan : TimeSpan.FromSeconds(1);
        double height = GetBounceHeight(element);

        var animation = new DoubleAnimationUsingKeyFrames
        {
            Duration = cycle,
            RepeatBehavior = RepeatBehavior.Forever,
            KeyFrames =
            {
                new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)),
                Up(-height, 0.24),
                Down(0.48),
            },
        };

        for (int i = 0; i < ReboundHeights.Length; i++)
        {
            animation.KeyFrames.Add(Up(-height * ReboundHeights[i], ReboundPeakTimes[i]));
            animation.KeyFrames.Add(Down(ReboundLandTimes[i]));
        }

        // Idle tail: keeps the element at rest until the cycle restarts.
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1)));

        return animation;

        static EasingDoubleKeyFrame Up(double peak, double at) =>
            new(peak, KeyTime.FromPercent(at)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };

        static EasingDoubleKeyFrame Down(double at) =>
            new(0, KeyTime.FromPercent(at)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
    }
}
