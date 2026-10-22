using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Format;

[Apartment(ApartmentState.STA)]
public class NumericUpDownTests
{
    private FormatInputHost<NumericUpDown> _host = null!;
    private NumericUpDown Box => _host.Box;

    [SetUp]
    public void SetUp() => _host = new FormatInputHost<NumericUpDown>(new NumericUpDown());

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Typing_digits_builds_the_number()
    {
        _host.Click(0);
        _host.Type("42");

        Assert.That(Box.Value, Is.EqualTo(42));
        Assert.That(Box.Text, Is.EqualTo("42"));
        Assert.That(Box.CaretIndex, Is.EqualTo(2));
    }

    [Test]
    public void Up_starts_an_empty_number_at_zero_then_counts()
    {
        _host.Click(0);
        _host.Press(Key.Up);
        _host.Press(Key.Up);

        Assert.That(Box.Value, Is.EqualTo(1));
    }

    [Test]
    public void A_value_set_from_outside_shows_in_the_text()
    {
        Box.Value = 12;

        Assert.That(Box.Text, Is.EqualTo("12"));
        Assert.That(Box.Values, Is.EqualTo(new object?[] { 12L }));

        Box.Value = null;

        Assert.That(Box.Text, Is.Empty);
    }

    [Test]
    public void A_two_way_binding_follows_both_ways()
    {
        var source = new NumberSource();
        Box.SetBinding(NumericUpDown.ValueProperty, new Binding(nameof(NumberSource.Number)) { Source = source });

        _host.Click(0);
        _host.Type("42");

        Assert.That(source.Number, Is.EqualTo(42));

        source.Number = 7;

        Assert.That(Box.Text, Is.EqualTo("7"));
    }

    [Test]
    public void A_one_way_binding_survives_typing()
    {
        var source = new NumberSource();
        Box.SetBinding(NumericUpDown.ValueProperty, new Binding(nameof(NumberSource.Number)) { Source = source, Mode = BindingMode.OneWay });

        _host.Click(0);
        _host.Type("4");
        source.Number = 9;

        Assert.That(Box.Value, Is.EqualTo(9));
    }

    [Test]
    public void Value_can_be_set_in_xaml()
    {
        const string xaml = "<NumericUpDown xmlns=\"clr-namespace:Joufflu.Inputs.Controls;assembly=Joufflu.Inputs\" Value=\"5\" />";
        var box = (NumericUpDown)XamlReader.Parse(xaml);

        Assert.That(box.Value, Is.EqualTo(5));
        Assert.That(box.Text, Is.EqualTo("5"));
    }

    [Test]
    public void A_group_written_in_xaml_replaces_the_default_one()
    {
        const string xaml = "<NumericUpDown xmlns=\"clr-namespace:Joufflu.Inputs.Controls;assembly=Joufflu.Inputs\""
            + " xmlns:format=\"clr-namespace:Joufflu.Inputs.Controls.Format;assembly=Joufflu.Inputs\">"
            + "<format:IntegerGroup StringFormat=\"00\" />"
            + "</NumericUpDown>";
        var box = (NumericUpDown)XamlReader.Parse(xaml);
        box.Value = 5;

        Assert.That(box.Text, Is.EqualTo("05"));
    }

    [Test]
    public void Value_can_be_bound_in_xaml()
    {
        const string xaml = "<NumericUpDown xmlns=\"clr-namespace:Joufflu.Inputs.Controls;assembly=Joufflu.Inputs\""
            + " xmlns:wpf=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\""
            + " Value=\"{wpf:Binding Number}\" />";
        var box = (NumericUpDown)XamlReader.Parse(xaml);
        box.DataContext = new NumberSource { Number = 6 };
        // A binding on the DataContext is resolved by the dispatcher, which the window runs.
        using var host = new FormatInputHost<NumericUpDown>(box);

        Assert.That(box.Value, Is.EqualTo(6));
        Assert.That(box.Text, Is.EqualTo("6"));
    }

    [Test]
    public void ValueChanged_is_raised_once_per_new_value()
    {
        var raised = new List<long?>();
        Box.ValueChanged += (_, value) => raised.Add(value);

        _host.Click(0);
        _host.Type("42");

        Assert.That(raised, Is.EqualTo(new long?[] { 4, 42 }));
    }

    private sealed class NumberSource : INotifyPropertyChanged
    {
        private long? _number;

        public long? Number
        {
            get => _number;
            set
            {
                _number = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Number)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
