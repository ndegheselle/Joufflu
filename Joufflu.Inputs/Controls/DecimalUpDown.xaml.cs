using System.Windows;
using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Controls
{
    public partial class DecimalUpDown : SingleValueFormatTextBox<decimal?>
    {
        static DecimalUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalUpDown), new FrameworkPropertyMetadata(typeof(DecimalUpDown)));
        }

        protected override IReadOnlyList<FormatPart> DefaultParts { get; } =
            [new DecimalGroup { IsNullable = true, SelectsWhole = false }];
    }
}
