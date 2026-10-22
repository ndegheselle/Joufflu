using System.Numerics;

namespace Joufflu.Inputs.Controls.Format
{
    /// <summary>
    /// One part of what a <see cref="FormatTextBox"/> shows, in the order of its
    /// <see cref="FormatTextBox.Parts"/>: a literal text, or a group the user types into.
    /// <para>
    /// Parts only describe the format: each box builds its own editable groups from them, so a
    /// part holds no value of its own.
    /// </para>
    /// </summary>
    public abstract class FormatPart
    {
        // Only the parts below are known to the editor.
        private protected FormatPart() { }
    }

    /// <summary>
    /// Text shown as it is between the groups, which the user does not type into.
    /// </summary>
    public sealed class FormatLiteral : FormatPart
    {
        public string Text { get; set; } = "";
    }

    /// <summary>
    /// A number the user types into, the way an HTML number input reads its
    /// <see cref="Min"/>, <see cref="Max"/> and <see cref="Step"/>.
    /// </summary>
    public abstract class NumberGroup<T> : FormatPart where T : struct, INumber<T>, IMinMaxValue<T>
    {
        private protected NumberGroup(T step)
        {
            Step = step;
        }

        /// <summary>
        /// The lowest number the user can type or spin to, that of the type when not set.
        /// </summary>
        public T? Min { get; set; }

        /// <summary>
        /// The highest number the user can type or spin to, that of the type when not set. Also
        /// sets the width of the group: once the number is as long as the max, typing moves on
        /// to the next group.
        /// </summary>
        public T? Max { get; set; }

        /// <summary>
        /// What Up, Down and the mouse wheel add or take away.
        /// </summary>
        public T Step { get; set; }

        /// <summary>
        /// A .NET numeric format string the number is shown with: "00" pads it with zeros to two
        /// digits, "N0" separates its thousands.
        /// </summary>
        public string? StringFormat { get; set; }

        /// <summary>
        /// Whether the group can hold no number at all, rather than zero.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Shown once for each character of a group holding no number.
        /// </summary>
        public char PromptChar { get; set; } = '-';

        /// <summary>
        /// Whether the group is selected whole, typed over and emptied as one thing. When false it
        /// keeps a caret of its own and is edited character by character, like a plain text box.
        /// </summary>
        public bool SelectsWhole { get; set; } = true;

        internal NumberGroupEditor<T> CreateEditor() => new NumberGroupEditor<T>(this);
    }

    /// <summary>
    /// A whole number, counted in <see cref="long"/>.
    /// </summary>
    public sealed class IntegerGroup : NumberGroup<long>
    {
        public IntegerGroup() : base(step: 1) { }
    }

    /// <summary>
    /// A decimal number, counted in <see cref="decimal"/> and typed with either "." or "," for
    /// the decimal separator.
    /// </summary>
    public sealed class DecimalGroup : NumberGroup<decimal>
    {
        public DecimalGroup() : base(step: 0.1m) { }
    }
}
