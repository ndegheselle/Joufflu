using System.Globalization;
using System.Numerics;

namespace Joufflu.Inputs.Controls.Format
{
    internal static class GroupsFactory
    {
        /// <summary>
        /// Create a group from its parameters, "numeric|max:59|padded" for one.
        /// </summary>
        /// <param name="stringParams">The group's own parameters, separated by |</param>
        /// <param name="globalStringParams">Parameters shared by every group, separated by |</param>
        /// <exception cref="ArgumentException">If no type is given, or an option is unknown</exception>
        public static BaseGroup Create(string stringParams, string? globalStringParams)
        {
            // Global parameters go first so that the group's own override them.
            IEnumerable<string> splitParams = stringParams.Split("|");
            if (globalStringParams != null)
                splitParams = globalStringParams.Split("|").Concat(splitParams);

            if (splitParams.Contains("numeric"))
            {
                GroupOptions options = GroupOptions.Parse(splitParams.Where(x => x != "numeric"));
                return new NumberGroup<long>(options, defaultIncrementDelta: 1);
            }
            if (splitParams.Contains("decimal"))
            {
                GroupOptions options = GroupOptions.Parse(splitParams.Where(x => x != "decimal"));
                return new NumberGroup<decimal>(options, defaultIncrementDelta: 0.1m);
            }

            throw new ArgumentException("Unknown type key.");
        }
    }

    /// <summary>
    /// The options a group is written with: "key:value" pairs, and flags with no value. The bounds
    /// stay text, for the group to read in the type it counts in.
    /// </summary>
    internal record GroupOptions
    {
        private static readonly HashSet<string> _knownKeys = new HashSet<string>()
        {
            "length", "format", "nullable", "nullableChar", "noGlobalSelection", "padded", "min", "max", "incrementDelta",
        };

        /// <summary>
        /// How many characters the group holds, 0 when nothing says.
        /// </summary>
        public int Length { get; init; }

        public string? StringFormat { get; init; }

        public bool IsNullable { get; init; }

        public char NullableChar { get; init; } = '-';

        public bool NoGlobalSelection { get; init; }

        public bool IsPadded { get; init; }

        public string? Min { get; init; }

        public string? Max { get; init; }

        public string? IncrementDelta { get; init; }

        /// <exception cref="ArgumentException">If an option is unknown, most likely a typo</exception>
        public static GroupOptions Parse(IEnumerable<string> stringParams)
        {
            // A later option overrides an earlier one with the same key.
            Dictionary<string, string?> options = new Dictionary<string, string?>();
            foreach (string param in stringParams)
            {
                string[] keyValue = param.Split(":", 2);
                options[keyValue[0]] = keyValue.Length > 1 ? keyValue[1] : null;
            }

            string[] unknownKeys = options.Keys.Where(key => !_knownKeys.Contains(key)).ToArray();
            if (unknownKeys.Length > 0)
                throw new ArgumentException("Unknown option(s): " + string.Join(", ", unknownKeys));

            return new GroupOptions()
            {
                Length = options.GetValueOrDefault("length") is string length ? int.Parse(length) : 0,
                StringFormat = options.GetValueOrDefault("format"),
                IsNullable = options.ContainsKey("nullable"),
                NullableChar = options.GetValueOrDefault("nullableChar") is string nullableChar ? nullableChar[0] : '-',
                NoGlobalSelection = options.ContainsKey("noGlobalSelection"),
                IsPadded = options.ContainsKey("padded"),
                Min = options.GetValueOrDefault("min"),
                Max = options.GetValueOrDefault("max"),
                IncrementDelta = options.GetValueOrDefault("incrementDelta"),
            };
        }
    }

    /// <summary>
    /// What became of an edit: whether the group took it, and where the caret belongs in the
    /// group once the text is built again.
    /// </summary>
    internal readonly record struct EditResult(bool Accepted, int Caret)
    {
        public static EditResult Rejected => new EditResult(false, 0);
    }

    /// <summary>
    /// One editable part of a <see cref="FormatTextBox"/>. A group knows nothing of the box: it
    /// is handed the caret and selection within its own text, and the box places them back.
    /// </summary>
    internal abstract class BaseGroup
    {
        /// <summary>
        /// How many characters the group holds, 0 when nothing says: a group given neither a
        /// length nor a max is bounded by its type alone, and takes as much as that type does.
        /// </summary>
        public int Length { get; protected set; } = 0;

        public int Index { get; set; } = -1;

        // Number of characters this group actually renders (may be less than Length when
        // the value is unpadded). Set by the parent when it formats the text.
        public int RenderedLength { get; set; }

        /// <summary>
        /// What the group holds, boxed in the type it counts in, null when it holds nothing.
        /// </summary>
        public abstract object? Value { get; }

        /// <summary>
        /// A group selected whole is replaced by what is typed and emptied as one thing. One that
        /// is not keeps a caret of its own and is edited character by character.
        /// </summary>
        public abstract bool SelectsWhole { get; }

        /// <summary>
        /// Holds nothing, and nothing is being typed into it either.
        /// </summary>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Uses every character it has, so a further digit could not be appended.
        /// </summary>
        public abstract bool IsFull { get; }

        /// <summary>
        /// Type [input] over the [selectionLength] characters at [caret], both within the group.
        /// </summary>
        public abstract EditResult Input(string input, int caret, int selectionLength);

        /// <summary>
        /// Take out the selected characters, or the one beside the caret when none is: the one
        /// before it when [backwards], the one after it otherwise.
        /// </summary>
        public abstract EditResult DeleteCharacter(bool backwards, int caret, int selectionLength);

        public abstract void Clear();

        public abstract void Increment();

        public abstract void Decrement();

        /// <summary>
        /// Take [value] as this group's own, coming from outside where nothing says which type a
        /// group counts in.
        /// </summary>
        public abstract void Load(object? value);

        /// <summary>
        /// The text the group shows in the box.
        /// </summary>
        public abstract string Render();
    }

    /// <summary>
    /// A number, counted in long for a "numeric" group and in decimal for a "decimal" one.
    /// </summary>
    internal class NumberGroup<T> : BaseGroup where T : struct, INumber<T>, IMinMaxValue<T>
    {
        #region Options
        public string? StringFormat { get; }

        public bool IsNullable { get; }

        public char NullableChar { get; }

        public override bool SelectsWhole { get; }

        public bool IsPadded { get; }

        public T Min { get; }

        public T Max { get; }

        public T IncrementDelta { get; }
        #endregion

        /// <summary>
        /// Written through <see cref="SetNumber"/> when the user edits the group, and through
        /// <see cref="Load"/> when the value comes from outside.
        /// </summary>
        private T? _number;

        public override object? Value => _number;

        /// <summary>
        /// What has been typed while a number cannot give it back as it stands: "3," on its way to
        /// "3,5", or "-" on its way to "-4". Null when the text is the value read out and nothing
        /// more, which is all it is once the number is whole.
        /// </summary>
        private string? _typedText;

        public override bool IsEmpty => _number == null && _typedText == null;

        /// <summary>
        /// Not full before, just because the next digit might go past the max: an over-large value
        /// is clamped by <see cref="SetNumber"/> instead. A group with no length of its own is
        /// never full.
        /// </summary>
        public override bool IsFull => _number is T number && Length > 0 && number.ToString()!.Length >= Length;

        public NumberGroup(GroupOptions options, T defaultIncrementDelta)
        {
            StringFormat = options.StringFormat;
            IsNullable = options.IsNullable;
            NullableChar = options.NullableChar;
            SelectsWhole = !options.NoGlobalSelection;
            IsPadded = options.IsPadded;

            T? max = ParseOption(options.Max);
            Min = ParseOption(options.Min) ?? T.MinValue;
            Max = max ?? T.MaxValue;
            IncrementDelta = ParseOption(options.IncrementDelta) ?? defaultIncrementDelta;

            // A group given no length is as wide as its max, when it is given one.
            Length = options.Length;
            if (Length == 0 && max is T explicitMax)
                Length = explicitMax.ToString()!.Length;

            // Last, so that the bounds it is clamped between are known.
            if (!IsNullable)
                SetNumber(T.Zero);
        }

        /// <summary>
        /// Hold [number], brought back between the bounds. Whatever was being typed is answered
        /// for by it.
        /// </summary>
        private void SetNumber(T? number)
        {
            _typedText = null;

            // null is a valid value (cleared/nullable group) and must not be clamped.
            if (number is not T given)
            {
                _number = null;
                return;
            }
            if (given > Max)
            {
                _number = Max;
                return;
            }
            if (given < Min)
            {
                _number = Min;
                return;
            }
            _number = given;
        }

        private static T? ParseOption(string? text)
        {
            if (text == null || !TryParse(text, out T value))
                return null;
            return value;
        }

        private static bool TryParse(string text, out T value)
            => T.TryParse(text, CultureInfo.CurrentCulture, out value);

        /// <summary>
        /// A number read back out of a box only comes out as the very type it went in as, and a
        /// host filling in the values has no reason to know which type the group counts in.
        /// Anything countable is taken and counted as the group's own.
        /// <para>
        /// Not clamped, unlike what the user edits: what a host hands over is taken as it stands.
        /// </para>
        /// </summary>
        public override void Load(object? value)
        {
            // What is being typed stands until the user finishes it.
            if (_typedText != null)
                return;

            if (value is null)
            {
                _number = null;
                return;
            }
            _number = (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public override EditResult Input(string input, int caret, int selectionLength)
        {
            input = NormalizeInput(input);

            string newText;
            int newCaret;
            if (SelectsWhole)
            {
                newText = (_typedText ?? _number?.ToString()) + input;
                newCaret = newText.Length;
            }
            else if (IsEmpty)
            {
                // What a group holding nothing shows stands for nothing: it is not text to type
                // into, so what is typed starts the number afresh rather than landing among the
                // characters that say the group is empty.
                newText = input;
                newCaret = input.Length;
            }
            else
            {
                string remainingText = Render().Remove(caret, selectionLength);
                newText = remainingText.Insert(caret, input);
                newCaret = caret + input.Length;
            }

            // If the number is too big we loop back to only the new number. A group that holds
            // as much as its type does never fills up: what does not fit fails to parse instead.
            if (Length > 0 && newText.Length > Length)
            {
                newText = input;
                newCaret = input.Length;
            }

            if (!ApplyText(newText))
                return EditResult.Rejected;
            return new EditResult(true, newCaret);
        }

        public override EditResult DeleteCharacter(bool backwards, int caret, int selectionLength)
        {
            string oldText = Render();
            int length = selectionLength;

            if (length == 0)
            {
                // Nothing is selected, so the key points at the one character beside the caret,
                // where there is one to point at.
                if (backwards)
                {
                    if (caret <= 0)
                        return new EditResult(true, caret);
                    caret -= 1;
                }
                else if (caret >= oldText.Length)
                {
                    return new EditResult(true, caret);
                }

                length = 1;
            }

            if (!ApplyText(oldText.Remove(caret, length)))
                return EditResult.Rejected;
            return new EditResult(true, caret);
        }

        /// <summary>
        /// Take [newText] as what the group now reads: the number it amounts to once it amounts to
        /// one, and what was written while it does not yet. Tells whether the text was taken.
        /// </summary>
        private bool ApplyText(string newText)
        {
            // Nothing left is nothing held, which is what emptying the group means.
            if (newText.Length == 0)
            {
                Clear();
                return true;
            }

            if (TryParse(newText, out T newValue))
            {
                SetNumber(newValue);

                // What was written is kept only where a number cannot give it back: a separator
                // with no fraction after it yet. A leading zero, say, is the group's own to render
                // as it always has. Never when the group clamped what was written either: what is
                // shown is then what is held.
                if (EqualityComparer<T?>.Default.Equals(_number, newValue)
                    && newText.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                    _typedText = newText;
                return true;
            }

            // Not a number yet, but on its way to one, which a digit would finish: a minus waiting
            // for its digits, a separator waiting for its fraction.
            if (TryParse(newText + "0", out _))
            {
                SetNumber(null);
                _typedText = newText;
                return true;
            }

            return false;
        }

        /// <summary>
        /// A fraction is written with whatever character the culture separates it by, and typed
        /// with whichever of the two keys the keyboard offers — a numeric keypad's point included.
        /// Harmless for a whole number: it refuses either separator all the same.
        /// </summary>
        private static string NormalizeInput(string input)
            => input is "." or ","
                ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
                : input;

        public override void Clear()
        {
            if (IsNullable)
                SetNumber(null);
            else
                SetNumber(T.Zero);
        }

        public override string Render()
        {
            // What is being typed stands for itself until a number can give it back.
            if (_typedText != null)
                return _typedText;

            // A group with a width of its own shows what it is waiting for; one without has no
            // slots to show, so an empty number is an empty field, the way a number field reads.
            if (_number is not T number)
                return new string(NullableChar, Length);

            string text = number.ToString()!;
            if (StringFormat != null)
                text = string.Format("{0" + StringFormat + "}", number);
            if (IsPadded)
                text = text.PadLeft(Length, '0');

            return text;
        }

        public override void Increment()
        {
            // An empty group starts at zero rather than one step past it.
            if (_number is not T number)
            {
                SetNumber(T.Zero);
                return;
            }
            SetNumber(number + IncrementDelta);
        }

        public override void Decrement()
        {
            // An empty group starts at zero rather than one step before it.
            if (_number is not T number)
            {
                SetNumber(T.Zero);
                return;
            }
            SetNumber(number - IncrementDelta);
        }
    }
}
