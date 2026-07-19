using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// Either get the display member path or the ToString() of the object
    /// </summary>
    public class DisplayMemberPathConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
                return string.Empty;

            if (values[1] == DependencyProperty.UnsetValue)
                return values[0].ToString();

            string lDisplayMemberPath = (string)values[1];
            if (!string.IsNullOrEmpty(lDisplayMemberPath))
                return values[0].GetType().GetProperty(lDisplayMemberPath)?.GetValue(values[0])?.ToString();
            else
                return values[0].ToString();
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        { throw new NotSupportedException(); }
    }

    public partial class ComboBoxTags : ComboBoxSearch
    {
        #region Dependency Properties
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList),
            typeof(ComboBoxTags),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, e) => ((ComboBoxTags)o).OnSelectedItemsChanged()));

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }
        #endregion

        #region Properties
        // The control always edits this working collection (the tag list is bound to it). Edits are
        // mirrored back into the bound SelectedItems, so two-way binding works for any mutable IList,
        // not only ObservableCollection.
        private readonly ObservableCollection<object> _internalSelectedItems = new();
        public IList InternalSelectedItems => _internalSelectedItems;

        // Guards the internal -> source mirroring while we are loading source -> internal.
        private bool _syncingSelection;

        public bool AllowAdd { get; set; } = false;

        // Only add to selection then clicking or pressing enter (like combobox with IsEditable = false)
        // Sad that the combobox doesn't allow to set this behavior
        private bool _ignoreNextSelection = false;
        private Popup? _popup;
        #endregion

        public ComboBoxTags()
        {
            _internalSelectedItems.CollectionChanged += OnInternalSelectedItemsChanged;

            SizeChanged += (s, e) =>
            {
                if (_popup == null)
                    return;

                var offset = _popup.HorizontalOffset;
                _popup.HorizontalOffset = offset + 1;
                _popup.HorizontalOffset = offset;
            };
        }

        public override void OnApplyTemplate()
        {
            _popup = (Popup)GetTemplateChild("PART_Popup");
            base.OnApplyTemplate();
        }

        // Load the bound selection into the working collection. Fired when the SelectedItems
        // reference changes (e.g. a view model assigns its collection).
        void OnSelectedItemsChanged()
        {
            _syncingSelection = true;
            try
            {
                _internalSelectedItems.Clear();
                if (SelectedItems != null)
                {
                    foreach (var item in SelectedItems)
                        _internalSelectedItems.Add(item);
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        // Mirror edits made through the control back into the bound collection, whatever concrete
        // (mutable) IList it is, so the binding source stays in sync.
        private void OnInternalSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_syncingSelection || SelectedItems == null || SelectedItems.IsReadOnly || SelectedItems.IsFixedSize)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems!)
                        SelectedItems.Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems!)
                        SelectedItems.Remove(item);
                    break;
                default:
                    // Replace / Move / Reset: rebuild the source from the working collection.
                    SelectedItems.Clear();
                    foreach (var item in _internalSelectedItems)
                        SelectedItems.Add(item);
                    break;
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (_ignoreNextSelection)
            {
                _ignoreNextSelection = false;
                return;
            }

            AddSelectedItem();
        }

        #region UI events
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            // ComboBox cycles the selection on mouse wheel when the dropdown is closed,
            // which would auto-add a tag. Block it, but still allow scrolling the open list.
            if (IsDropDownOpen == false)
                e.Handled = true;

            base.OnPreviewMouseWheel(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
                _ignoreNextSelection = true;

            if (e.Key == Key.Back && string.IsNullOrEmpty(Text) && InternalSelectedItems.Count > 0)
                InternalSelectedItems.RemoveAt(InternalSelectedItems.Count - 1);

            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Back && string.IsNullOrEmpty(Text) && InternalSelectedItems.Count > 0)
            {
                InternalSelectedItems.RemoveAt(InternalSelectedItems.Count - 1);
            }
            else if (e.Key == Key.Enter)
            {
                if (SelectedItem != null)
                {
                    AddSelectedItem();
                }
                else if (AllowAdd && !string.IsNullOrEmpty(Text))
                {
                    InternalSelectedItems.Add(Text);
                    Text = string.Empty;
                }
            }

            base.OnKeyDown(e);
        }

        #endregion

        #region Methods
        protected override bool DoesItemPassFilter(object value)
        {
            // If the item is already selected, don't show it in the list
            if (InternalSelectedItems.Contains(value) == true)
                return false;

            return base.DoesItemPassFilter(value);
        }

        private void AddSelectedItem()
        {
            if (SelectedItem == null)
                return;
            InternalSelectedItems.Add(SelectedItem);
            Text = string.Empty;
        }

        [RelayCommand]
        private void RemoveSelected(object? parameter) => InternalSelectedItems.Remove(parameter);
        #endregion
    }
}
