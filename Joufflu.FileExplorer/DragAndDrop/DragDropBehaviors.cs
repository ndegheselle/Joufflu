using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.FileExplorer.DragAndDrop;

/// <summary>
/// Common part of <see cref="DragHandler"/> and <see cref="DropHandler"/> : the single element the handler drives,
/// and its subscription to the events of that element.
/// </summary>
public abstract class DragDropHandler : ObservableObject
{
    /// <summary>
    /// Element the handler is bound to, null while it is not bound.
    /// </summary>
    protected FrameworkElement? Element { get; private set; }

    /// <summary>
    /// Called by <see cref="Drag"/> and <see cref="Drop"/> when the handler is bound to an element.
    /// </summary>
    internal void Attach(FrameworkElement element)
    {
        Element = element;
        Subscribe(element);
    }

    /// <summary>
    /// Called by <see cref="Drag"/> and <see cref="Drop"/> when the handler is unbound from an element.
    /// </summary>
    internal void Detach(FrameworkElement element)
    {
        if (Element != element)
            return;

        Unsubscribe(element);
        Element = null;
    }

    /// <summary>
    /// Subscribes to the events the handler needs, and pushes its state on the element.
    /// </summary>
    protected abstract void Subscribe(FrameworkElement element);

    /// <summary>
    /// Undoes <see cref="Subscribe"/>, which is what keeps a rebind from firing the drag or the drop twice.
    /// </summary>
    protected abstract void Unsubscribe(FrameworkElement element);
}

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

        (e.OldValue as DragHandler)?.Detach(element);
        (e.NewValue as DragHandler)?.Attach(element);
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

    internal static void SetIsDragSource(DependencyObject obj, bool value) =>
        obj.SetValue(IsDragSourcePropertyKey, value);

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

        (e.OldValue as DropHandler)?.Detach(element);
        (e.NewValue as DropHandler)?.Attach(element);
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
    /// True while a drop the handler accepts hovers the element : set on the element the handler is attached to, and
    /// on the container under the cursor when <see cref="DropHandler.HoverContainerType"/> asks for one. Read only,
    /// meant to be used in a style trigger to highlight the destination, the whole list and the row under the cursor
    /// being highlightable at the same time.
    /// </summary>
    public static readonly DependencyProperty IsHoveringProperty = IsHoveringPropertyKey.DependencyProperty;

    /// <inheritdoc cref="IsHoveringProperty"/>
    public static bool GetIsHovering(DependencyObject obj) => (bool)obj.GetValue(IsHoveringProperty);

    internal static void SetIsHovering(DependencyObject obj, bool value) =>
        obj.SetValue(IsHoveringPropertyKey, value);

    #endregion
}
