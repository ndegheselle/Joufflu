using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Joufflu.FileExplorer.DragAndDrop;

/// <summary>
/// Attaches a <see cref="DragHandler"/> to an element : <c>dnd:Drag.Handler="{Binding DragHandler}"</c>, instead of
/// registering every mouse event by hand.
/// </summary>
public static class Drag
{
    #region Handler

    /// <summary>
    /// Handler driving the drags started from the element.
    /// </summary>
    public static readonly DependencyProperty HandlerProperty = DependencyProperty.RegisterAttached(
        "Handler",
        typeof(DragHandler),
        typeof(Drag),
        new PropertyMetadata(null, OnHandlerChanged));

    /// <inheritdoc cref="HandlerProperty"/>
    public static DragHandler? GetHandler(DependencyObject obj) => (DragHandler?)obj.GetValue(HandlerProperty);

    /// <inheritdoc cref="HandlerProperty"/>
    public static void SetHandler(DependencyObject obj, DragHandler? value) => obj.SetValue(HandlerProperty, value);

    private static void OnHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        // Unsubscribing is what keeps a rebind from firing the drag twice
        if (e.OldValue is DragHandler previous)
        {
            element.RemoveHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(previous.HandleMouseDown));
            element.RemoveHandler(Mouse.MouseMoveEvent, new MouseEventHandler(previous.HandleMouseMove));
            element.GiveFeedback -= previous.HandleGiveFeedback;
            element.QueryContinueDrag -= previous.HandleQueryContinueDrag;
            previous.PropertyChanged -= GetStateWatcher(element);
            SetStateWatcher(element, null);
            SetIsDragSource(element, false);
            previous.Detach(element);
        }

        if (e.NewValue is not DragHandler handler)
            return;

        handler.Attach(element);
        // The template is declared in XAML next to the element, not on the view model
        if (GetAdornerTemplate(element) is { } template)
            handler.AdornerTemplate = template;

        // handledEventsToo is required : the items of a selector (ListBoxItem, ListViewItem, TreeViewItem, ...) mark
        // the mouse down as handled to select themselves, so it never reaches the element as a plain subscription
        element.AddHandler(
            Mouse.MouseDownEvent,
            new MouseButtonEventHandler(handler.HandleMouseDown),
            handledEventsToo: true);
        element.AddHandler(
            Mouse.MouseMoveEvent,
            new MouseEventHandler(handler.HandleMouseMove),
            handledEventsToo: true);
        element.GiveFeedback += handler.HandleGiveFeedback;
        element.QueryContinueDrag += handler.HandleQueryContinueDrag;

        // Mirror the state of the handler on the element, so a style can react to it
        void OnHandlerPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(DragHandler.IsDragging))
                SetIsDragSource(element, handler.IsDragging);
        }

        SetStateWatcher(element, OnHandlerPropertyChanged);
        handler.PropertyChanged += OnHandlerPropertyChanged;
        SetIsDragSource(element, handler.IsDragging);
    }

    #endregion

    #region AdornerTemplate

    /// <summary>
    /// Sets <see cref="DragHandler.AdornerTemplate"/> from XAML, so the visual shown under the cursor stays in the
    /// view : <c>dnd:Drag.AdornerTemplate="{StaticResource MyTemplate}"</c>.
    /// </summary>
    public static readonly DependencyProperty AdornerTemplateProperty = DependencyProperty.RegisterAttached(
        "AdornerTemplate",
        typeof(DataTemplate),
        typeof(Drag),
        new PropertyMetadata(null, OnAdornerTemplateChanged));

    /// <inheritdoc cref="AdornerTemplateProperty"/>
    public static DataTemplate? GetAdornerTemplate(DependencyObject obj) =>
        (DataTemplate?)obj.GetValue(AdornerTemplateProperty);

    /// <inheritdoc cref="AdornerTemplateProperty"/>
    public static void SetAdornerTemplate(DependencyObject obj, DataTemplate? value) =>
        obj.SetValue(AdornerTemplateProperty, value);

    private static void OnAdornerTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (GetHandler(d) is { } handler)
            handler.AdornerTemplate = e.NewValue as DataTemplate;
    }

    #endregion

    #region IsDragSource

    private static readonly DependencyPropertyKey IsDragSourcePropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "IsDragSource",
            typeof(bool),
            typeof(Drag),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// True while a drag started from the element is in progress. Read only, meant to be used in a style trigger.
    /// </summary>
    public static readonly DependencyProperty IsDragSourceProperty = IsDragSourcePropertyKey.DependencyProperty;

    /// <inheritdoc cref="IsDragSourceProperty"/>
    public static bool GetIsDragSource(DependencyObject obj) => (bool)obj.GetValue(IsDragSourceProperty);

    private static void SetIsDragSource(DependencyObject obj, bool value) =>
        obj.SetValue(IsDragSourcePropertyKey, value);

    #endregion

    #region State watcher

    /// <summary>
    /// Keeps the subscription to the handler, so it can be removed when the handler changes.
    /// </summary>
    private static readonly DependencyProperty StateWatcherProperty = DependencyProperty.RegisterAttached(
        "StateWatcher",
        typeof(PropertyChangedEventHandler),
        typeof(Drag),
        new PropertyMetadata(null));

    private static PropertyChangedEventHandler? GetStateWatcher(DependencyObject obj) =>
        (PropertyChangedEventHandler?)obj.GetValue(StateWatcherProperty);

    private static void SetStateWatcher(DependencyObject obj, PropertyChangedEventHandler? value) =>
        obj.SetValue(StateWatcherProperty, value);

    #endregion
}

/// <summary>
/// Attaches a <see cref="DropHandler"/> to an element : <c>dnd:Drop.Handler="{Binding DropHandler}"</c>, instead of
/// registering every drag event by hand and setting <see cref="UIElement.AllowDrop"/>.
/// </summary>
public static class Drop
{
    #region Handler

    /// <summary>
    /// Handler validating and applying the drops on the element.
    /// </summary>
    public static readonly DependencyProperty HandlerProperty = DependencyProperty.RegisterAttached(
        "Handler",
        typeof(DropHandler),
        typeof(Drop),
        new PropertyMetadata(null, OnHandlerChanged));

    /// <inheritdoc cref="HandlerProperty"/>
    public static DropHandler? GetHandler(DependencyObject obj) => (DropHandler?)obj.GetValue(HandlerProperty);

    /// <inheritdoc cref="HandlerProperty"/>
    public static void SetHandler(DependencyObject obj, DropHandler? value) => obj.SetValue(HandlerProperty, value);

    private static void OnHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        // Unsubscribing is what keeps a rebind from applying the drop twice
        if (e.OldValue is DropHandler previous)
        {
            element.DragEnter -= previous.HandleDragEnter;
            element.DragOver -= previous.HandleDragOver;
            element.DragLeave -= previous.HandleDragLeave;
            element.Drop -= previous.HandleDrop;
            previous.PropertyChanged -= GetStateWatcher(element);
            SetStateWatcher(element, null);
            SetIsDropTarget(element, false);
            previous.Detach(element);
        }

        if (e.NewValue is not DropHandler handler)
        {
            element.AllowDrop = false;
            return;
        }

        handler.Attach(element);
        element.AllowDrop = true;
        element.DragEnter += handler.HandleDragEnter;
        element.DragOver += handler.HandleDragOver;
        element.DragLeave += handler.HandleDragLeave;
        element.Drop += handler.HandleDrop;

        // Mirror the state of the handler on the element, so a style can react to it
        void OnHandlerPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(DropHandler.IsDropTarget))
                SetIsDropTarget(element, handler.IsDropTarget);
        }

        SetStateWatcher(element, OnHandlerPropertyChanged);
        handler.PropertyChanged += OnHandlerPropertyChanged;
        SetIsDropTarget(element, handler.IsDropTarget);
    }

    #endregion

    #region IsHovering

    private static readonly DependencyPropertyKey IsHoveringPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "IsHovering",
            typeof(bool),
            typeof(Drop),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// True while a drop that would be accepted hovers this container, see
    /// <see cref="DropHandler.HoverContainerType"/>. Read only, meant to be used in a style trigger to highlight the
    /// destination.
    /// </summary>
    public static readonly DependencyProperty IsHoveringProperty = IsHoveringPropertyKey.DependencyProperty;

    /// <inheritdoc cref="IsHoveringProperty"/>
    public static bool GetIsHovering(DependencyObject obj) => (bool)obj.GetValue(IsHoveringProperty);

    internal static void SetIsHovering(DependencyObject obj, bool value) =>
        obj.SetValue(IsHoveringPropertyKey, value);

    #endregion

    #region IsDropTarget

    private static readonly DependencyPropertyKey IsDropTargetPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "IsDropTarget",
            typeof(bool),
            typeof(Drop),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// True while a drop the element accepts is hovering it. Read only, meant to be used in a style trigger to show
    /// the element can receive the drop.
    /// </summary>
    public static readonly DependencyProperty IsDropTargetProperty = IsDropTargetPropertyKey.DependencyProperty;

    /// <inheritdoc cref="IsDropTargetProperty"/>
    public static bool GetIsDropTarget(DependencyObject obj) => (bool)obj.GetValue(IsDropTargetProperty);

    private static void SetIsDropTarget(DependencyObject obj, bool value) =>
        obj.SetValue(IsDropTargetPropertyKey, value);

    #endregion

    #region State watcher

    /// <summary>
    /// Keeps the subscription to the handler, so it can be removed when the handler changes.
    /// </summary>
    private static readonly DependencyProperty StateWatcherProperty = DependencyProperty.RegisterAttached(
        "StateWatcher",
        typeof(PropertyChangedEventHandler),
        typeof(Drop),
        new PropertyMetadata(null));

    private static PropertyChangedEventHandler? GetStateWatcher(DependencyObject obj) =>
        (PropertyChangedEventHandler?)obj.GetValue(StateWatcherProperty);

    private static void SetStateWatcher(DependencyObject obj, PropertyChangedEventHandler? value) =>
        obj.SetValue(StateWatcherProperty, value);

    #endregion
}
