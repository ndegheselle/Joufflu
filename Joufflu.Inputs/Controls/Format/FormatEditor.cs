using System.Text;

namespace Joufflu.Inputs.Controls.Format
{
    /// <summary>
    /// What a <see cref="FormatTextBox"/> shows and how it answers the keyboard, kept apart from
    /// WPF so that it can be tested without a window: the text built from the groups, the
    /// selection within it, and the edits the user makes. The box forwards its input here and
    /// shows the outcome.
    /// </summary>
    internal sealed class FormatEditor
    {
        // Ordered format parts: GroupEditor for a group, string for literal text between groups.
        private readonly List<object> _parts;
        private readonly List<GroupEditor> _groups;

        public string Text { get; private set; } = "";

        public int SelectionStart { get; private set; }

        public int SelectionLength { get; private set; }

        /// <summary>
        /// -1 while the selection is outside every group.
        /// </summary>
        public int SelectedGroupIndex { get; private set; } = -1;

        public GroupEditor? SelectedGroup => SelectedGroupIndex >= 0 ? _groups[SelectedGroupIndex] : null;

        public FormatEditor(IEnumerable<FormatPart> parts)
        {
            _parts = parts.Select(CreateEditorPart).ToList();
            _groups = _parts.OfType<GroupEditor>().ToList();
            Render();
        }

        private static object CreateEditorPart(FormatPart part) => part switch
        {
            FormatLiteral literal => literal.Text,
            IntegerGroup group => group.CreateEditor(),
            DecimalGroup group => group.CreateEditor(),
            // FormatPart cannot be derived from outside the library: a part added to it, not here.
            _ => throw new NotSupportedException($"{part.GetType().Name} is not a known format part."),
        };

        /// <summary>
        /// What each group holds, in the order of the format.
        /// </summary>
        public List<object?> GetValues() => _groups.Select(group => group.Value).ToList();

        /// <summary>
        /// Take [values] from outside, one per group. A list that does not answer the format is
        /// left alone: there is no telling which group each of its values would belong to.
        /// </summary>
        public void Load(IReadOnlyList<object?>? values)
        {
            if (values == null || values.Count != _groups.Count)
                return;

            for (int i = 0; i < _groups.Count; i++)
                _groups[i].Load(values[i]);
            Render();
        }

        #region User actions
        /// <summary>
        /// The user selecting [length] characters from [start]. A group selected whole is
        /// selected as one thing, wherever in it the user clicked.
        /// </summary>
        public void Select(int start, int length)
        {
            SetSelection(start, length);
            SelectedGroupIndex = GroupIndexAt(SelectionStart);
            if (SelectedGroup is { SelectsWhole: true } group)
                SelectWhole(group);
        }

        /// <summary>
        /// Move [delta] groups along, and tell whether there was one to move to.
        /// </summary>
        public bool MoveToGroup(int delta)
        {
            int newIndex = SelectedGroupIndex + delta;
            if (newIndex < 0 || newIndex >= _groups.Count)
                return false;

            SelectedGroupIndex = newIndex;
            GroupEditor group = _groups[newIndex];
            if (group.SelectsWhole)
                SelectWhole(group);
            else
                SetSelection(group.Index, 0);
            return true;
        }

        /// <summary>
        /// How an arrow key pointing in the direction [delta] is handled. A group not selected
        /// whole lets the caret move inside it, so the key is left to the text box until the
        /// caret reaches the edge of the group.
        /// </summary>
        /// <returns>true if the key is handled here</returns>
        public bool MoveCaret(int delta)
        {
            if (SelectedGroup is not GroupEditor group)
                return false;

            if (group.SelectsWhole)
            {
                MoveToGroup(delta);
                return true;
            }

            bool isAtEdge = delta < 0
                ? SelectionStart <= group.Index
                : SelectionStart >= group.Index + group.RenderedLength;
            if (!isAtEdge)
                return false;

            MoveToGroup(delta);
            return true;
        }

        public void Type(string input)
        {
            // If no group is selected default to the first one
            if (SelectedGroup == null)
                MoveToGroup(1);
            if (SelectedGroup is not GroupEditor group)
                return;

            (int caret, int selectionLength) = SelectionInGroup(group);
            EditResult result = group.Input(input, caret, selectionLength);
            if (!result.Accepted)
                return;
            CommitEdit(group, result.Caret);
        }

        /// <summary>
        /// A group keeping its own caret takes out the character the key points at; a group
        /// selected whole is selected as one thing, so it is emptied as one.
        /// </summary>
        public void Delete(bool backwards)
        {
            if (SelectedGroup is not GroupEditor group)
                return;

            if (group.SelectsWhole)
            {
                group.Clear();
                CommitEdit(group, 0);
                return;
            }

            (int caret, int selectionLength) = SelectionInGroup(group);
            EditResult result = group.DeleteCharacter(backwards, caret, selectionLength);
            if (!result.Accepted)
                return;
            CommitEdit(group, result.Caret);
        }

        /// <summary>
        /// Increment (direction &gt; 0) or decrement the selected group.
        /// </summary>
        public void Spin(int direction)
        {
            if (SelectedGroup == null)
                MoveToGroup(1);
            if (SelectedGroup is not GroupEditor group)
                return;

            if (direction >= 0)
                group.Increment();
            else
                group.Decrement();

            // A group keeping its own caret has it at the end of the number it now reads.
            string spunText = group.Render();
            CommitEdit(group, spunText.Length);
        }

        public void Clear()
        {
            foreach (GroupEditor group in _groups)
                group.Clear();
            Render();
        }
        #endregion

        /// <summary>
        /// The caret and the length of the selection within [group], kept inside it: a group only
        /// edits its own text.
        /// </summary>
        private (int Caret, int SelectionLength) SelectionInGroup(GroupEditor group)
        {
            int caret = Math.Clamp(SelectionStart - group.Index, 0, group.RenderedLength);
            int selectionLength = Math.Min(SelectionLength, group.RenderedLength - caret);
            return (caret, selectionLength);
        }

        /// <summary>
        /// Build the text again from what [group] now holds, then place the caret at [caret]
        /// within it. Not before: the caret could not be placed in a text not yet holding what
        /// was typed.
        /// </summary>
        private void CommitEdit(GroupEditor group, int caret)
        {
            Render();
            PlaceCaretAfterEdit(group, caret);
        }

        private void PlaceCaretAfterEdit(GroupEditor group, int caret)
        {
            // Nothing typed, nothing to answer for.
            if (group.IsEmpty)
                return;

            // Once the field is full, another digit can no longer fit, so move on to the next
            // group. When there is none to move to - a lone group, or the last one — a group
            // keeping its own caret keeps it at the end of what was typed rather than letting it
            // drift back into the middle of the number.
            if (group.IsFull)
            {
                bool movedOn = MoveToGroup(1);
                if (movedOn || group.SelectsWhole)
                    return;
            }

            if (group.SelectsWhole)
            {
                SelectWhole(group);
                return;
            }

            // The caret is only ever set after an edit, so that clicking about the box stays the
            // user's own business.
            int caretInGroup = Math.Min(caret, group.RenderedLength);
            SetSelection(group.Index + caretInGroup, 0);
        }

        /// <summary>
        /// Select the whole of [group]: its rendered width, which may be shorter than its max
        /// Length when the value is not padded.
        /// </summary>
        private void SelectWhole(GroupEditor group) => SetSelection(group.Index, group.RenderedLength);

        // Kept within the text, as the text box would.
        private void SetSelection(int start, int length)
        {
            SelectionStart = Math.Clamp(start, 0, Text.Length);
            SelectionLength = Math.Clamp(length, 0, Text.Length - SelectionStart);
        }

        private int GroupIndexAt(int position)
        {
            for (int i = 0; i < _groups.Count; i++)
            {
                GroupEditor group = _groups[i];
                if (position >= group.Index && position <= group.Index + group.RenderedLength)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Build the text from the ordered parts, recording each group's actual start index and
        /// rendered length. Groups can render fewer characters than their max Length (e.g. an
        /// unpadded "0"), so positions must come from the real text, not from the max width,
        /// otherwise selection drifts.
        /// </summary>
        private void Render()
        {
            StringBuilder builder = new StringBuilder();
            foreach (object part in _parts)
            {
                if (part is not GroupEditor group)
                {
                    builder.Append((string)part);
                    continue;
                }

                string rendered = group.Render();
                group.Index = builder.Length;
                group.RenderedLength = rendered.Length;
                builder.Append(rendered);
            }

            Text = builder.ToString();
            SetSelection(SelectionStart, SelectionLength);
        }
    }
}
