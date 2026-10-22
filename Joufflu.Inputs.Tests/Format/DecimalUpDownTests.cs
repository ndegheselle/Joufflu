using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Format;

// The decimal separator follows the current culture, so it is pinned.
[SetCulture("fr-FR")]
[Apartment(ApartmentState.STA)]
public class DecimalUpDownTests
{
    private FormatInputHost<DecimalUpDown> _host = null!;
    private DecimalUpDown Box => _host.Box;

    [SetUp]
    public void SetUp() => _host = new FormatInputHost<DecimalUpDown>(new DecimalUpDown());

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Typing_a_fraction_gives_the_decimal_value()
    {
        _host.Click(0);
        _host.Type("3,5");

        Assert.That(Box.Value, Is.EqualTo(3.5m));
        Assert.That(Box.Text, Is.EqualTo("3,5"));
    }
}
