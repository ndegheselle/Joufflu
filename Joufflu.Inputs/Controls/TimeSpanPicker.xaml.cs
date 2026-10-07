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

        public TimeSpanPicker()
        {
            GlobalFormat = "numeric|min:0|padded|nullable";
            Format = "{max:365}d {max:23}h {max:59}m {max:59}s";
        }

        protected override TimeSpan? FromValues(IReadOnlyList<object?> values)
        {
            if (values.Count < 4)
                return null;

            // Groups are days / hours / minutes / seconds (see the Format above). A numeric group
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
