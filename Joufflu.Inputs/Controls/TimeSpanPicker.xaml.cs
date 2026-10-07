using System.Windows;
using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Controls
{
    public partial class TimeSpanPicker : SingleValueFormatTextBox<TimeSpan?>
    {
        static TimeSpanPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSpanPicker), new FrameworkPropertyMetadata(typeof(TimeSpanPicker)));
        }

        protected override IReadOnlyList<FormatPart> DefaultParts { get; } =
        [
            TimeGroup(max: 365, stringFormat: "000"),
            new FormatLiteral { Text = "d " },
            TimeGroup(max: 23, stringFormat: "00"),
            new FormatLiteral { Text = "h " },
            TimeGroup(max: 59, stringFormat: "00"),
            new FormatLiteral { Text = "m " },
            TimeGroup(max: 59, stringFormat: "00"),
            new FormatLiteral { Text = "s" },
        ];

        private static IntegerGroup TimeGroup(long max, string stringFormat)
            => new IntegerGroup { Min = 0, Max = max, StringFormat = stringFormat, IsNullable = true };

        protected override TimeSpan? FromValues(IReadOnlyList<object?> values)
        {
            if (values.Count < 4)
                return null;

            // Groups are days / hours / minutes / seconds (see the DefaultParts above). An integer group
            // counts in long, and a boxed value only comes back out as the very type it went in as.
            long? days = values[0] as long?;
            long? hours = values[1] as long?;
            long? minutes = values[2] as long?;
            long? seconds = values[3] as long?;

            if (!days.HasValue || !hours.HasValue || !minutes.HasValue || !seconds.HasValue)
                return null;

            return new TimeSpan((int)days.Value, (int)hours.Value, (int)minutes.Value, (int)seconds.Value);
        }

        protected override List<object?> ToValues(TimeSpan? value)
        {
            if (value is TimeSpan date)
                return new List<object?>() { (long)date.Days, (long)date.Hours, (long)date.Minutes, (long)date.Seconds };
            else
                return new List<object?>() { null, null, null, null };
        }
    }
}
