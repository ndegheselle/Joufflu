using System.Windows;
using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Controls
{
    public partial class NumericUpDown : SingleValueFormatTextBox<long?>
    {
        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(long?),
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(default(long?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, e) => ((NumericUpDown)o).OnValueChanged(e)));

        public override long? Value
        {
            get { return (long?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public NumericUpDown()
        {
            Format = "{numeric|noGlobalSelection|nullable}";
        }
    }
}
