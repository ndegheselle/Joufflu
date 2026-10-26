using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Joufflu.Toolkit;

namespace Joufflu.Tests.Toolkit;

/// <summary>
/// Fixtures run in a single-threaded apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class SharedWidthTests
{
    private Window _window = null!;
    private ScrollViewer _scroll = null!;
    private StackPanel _rows = null!;

    /// <summary>
    /// Rows under a horizontal scroll, as in a tree: the scroll's extent follows the widest row.
    /// </summary>
    [SetUp]
    public void Show()
    {
        _rows = new StackPanel();
        _scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _rows
        };
        var scope = new Border { Child = _scroll };
        SharedWidth.SetIsScope(scope, true);
        _window = new Window
        {
            Width = 400,
            SizeToContent = SizeToContent.Height,
            Left = -10000,
            ShowActivated = false,
            ShowInTaskbar = false,
            Content = scope
        };
        _window.Show();
    }

    [TearDown]
    public void Close() => _window.Close();

    [Test]
    public void Every_member_takes_the_width_of_the_narrowest_one()
    {
        TextBox wide = AddRow(labelWidth: 50);
        TextBox narrow = AddRow(labelWidth: 150);

        Settle();

        Assert.That(wide.ActualWidth, Is.EqualTo(narrow.ActualWidth));
        Assert.That(wide.ActualWidth, Is.LessThan(_rows.ActualWidth - 150 + 1));
    }

    [Test]
    public void Members_widen_again_once_the_narrowest_one_leaves()
    {
        TextBox wide = AddRow(labelWidth: 50);
        TextBox narrow = AddRow(labelWidth: 150);
        Settle();
        double shared = wide.ActualWidth;

        _rows.Children.RemoveAt(1);
        Settle();

        Assert.That(wide.ActualWidth, Is.GreaterThan(shared + 50));
    }

    [Test]
    public void Members_narrow_down_with_their_scope_under_a_horizontal_scroll()
    {
        TextBox wide = AddRow(labelWidth: 50);
        AddRow(labelWidth: 150);
        Settle();

        _window.Width = 300;
        Settle();

        Assert.That(_scroll.ExtentWidth, Is.LessThanOrEqualTo(_scroll.ViewportWidth));
        Assert.That(wide.ActualWidth, Is.LessThan(_scroll.ViewportWidth - 150 + 1));
    }

    /// <summary>A label of a fixed width and a member filling the rest, like a form row.</summary>
    private TextBox AddRow(double labelWidth)
    {
        var editor = new TextBox();
        SharedWidth.SetIsShared(editor, true);
        Grid.SetColumn(editor, 1);

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.Children.Add(new Border { Width = labelWidth });
        row.Children.Add(editor);
        _rows.Children.Add(row);
        return editor;
    }

    /// <summary>Lets the loads and the layout passes the widths trigger run to the end.</summary>
    private static void Settle()
        => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
}
