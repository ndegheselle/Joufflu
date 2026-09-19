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

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(TimeSpan?),
            typeof(TimeSpanPicker),
            new FrameworkPropertyMetadata(default(TimeSpan?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, e) => ((TimeSpanPicker)o).OnValueChanged(e)));

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

            // Groups are days / hours / minutes / seconds (see the Format above). A numeric group
            // counts in long, and a boxed value only comes back out as the very type it went in as.
            long? days = Values[0] as long?;
            long? hours = Values[1] as long?;
            long? minutes = Values[2] as long?;
            long? seconds = Values[3] as long?;

            if (!days.HasValue || !hours.HasValue || !minutes.HasValue || !seconds.HasValue)
                return null;

            return new TimeSpan((int)days.Value, (int)hours.Value, (int)minutes.Value, (int)seconds.Value);
        }

        public override List<object?> ConvertTo()
        {
            if (Value is TimeSpan date)
                return new List<object?>() { (long)date.Days, (long)date.Hours, (long)date.Minutes, (long)date.Seconds };
            else
                return new List<object?>() { null, null, null, null };
        }
    }
}
