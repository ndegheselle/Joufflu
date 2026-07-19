using System.Windows;
using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Controls
{
    public partial class NumericUpDown : SingleValueFormatTextBox<int>
    {
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(default(int), (o, e) => ((NumericUpDown)o).OnValueChanged(e)
        ));

        public override int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public NumericUpDown()
        {
            Format = "{numeric|noGlobalSelection}";
        }
    }
}
