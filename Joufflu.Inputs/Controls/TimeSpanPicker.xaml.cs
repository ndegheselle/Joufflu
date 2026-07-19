using System.Windows;
using Joufflu.Inputs.Controls.Format;

namespace Joufflu.Inputs.Controls
{
    public partial class TimeSpanPicker : SingleValueFormatTextBox<TimeSpan?>
    {
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(TimeSpan?),
            typeof(TimeSpanPicker),
            new PropertyMetadata(default(TimeSpan?), (o, e) => ((TimeSpanPicker)o).OnValueChanged(e)));

        public override TimeSpan? Value
        {
            get { return (TimeSpan?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public TimeSpanPicker()
        {
            GlobalFormat = "numeric|min:0|padded|nullable";
            Format = "{max:365}d {max:23}h {max:59}m {max:59}s";
        }

        public override TimeSpan? ConvertFrom()
        {
            if (Values.Count < 4)
                return null;

            // Groups are days / hours / minutes / seconds (see the Format above).
            int? days = Values[0] as int?;
            int? hours = Values[1] as int?;
            int? minutes = Values[2] as int?;
            int? seconds = Values[3] as int?;

            if (!days.HasValue || !hours.HasValue || !minutes.HasValue || !seconds.HasValue)
                return null;

            return new TimeSpan(days.Value, hours.Value, minutes.Value, seconds.Value);
        }

        public override List<object?> ConvertTo()
        {
            if (Value is TimeSpan date)
                return new List<object?>() { date.Days, date.Hours, date.Minutes, date.Seconds };
            else
                return new List<object?>() { null, null, null, null };
        }
    }
}
