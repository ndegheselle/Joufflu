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

        protected override IReadOnlyList<FormatPart> DefaultParts { get; } =
            [new IntegerGroup { IsNullable = true, SelectsWhole = false }];
    }
}
