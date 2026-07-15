using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Joufflu.Navigation.Controls
{
    /// <summary>
    /// Corner of the popup that is anchored to the matching corner of the toggle button.
    /// </summary>
    public enum DropdownPlacement
    {
        /// <summary>Popup top-left at the button's bottom-left (default, opens downward).</summary>
        BottomLeft,
        /// <summary>Popup top-right at the button's bottom-right (right-aligned, opens downward).</summary>
        BottomRight,
        /// <summary>Popup bottom-left at the button's top-left (opens upward).</summary>
        TopLeft,
        /// <summary>Popup bottom-right at the button's top-right (right-aligned, opens upward).</summary>
        TopRight
    }

    [TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
    public class Dropdown : ContentControl
    {
        private const string PART_Popup = "PART_Popup";

        private Popup? _popup;

        #region Dependency Properties
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
            nameof(ButtonStyle),
            typeof(Style),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PopupStyleProperty = DependencyProperty.Register(
            nameof(PopupStyle),
            typeof(Style),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PopupPlacementProperty = DependencyProperty.Register(
            nameof(PopupPlacement),
            typeof(DropdownPlacement),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(DropdownPlacement.BottomLeft));

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(
            nameof(HorizontalOffset),
            typeof(double),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(0d));

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            nameof(VerticalOffset),
            typeof(double),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(0d));
        #endregion

        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>Style applied to the toggle button that opens the dropdown.</summary>
        public Style ButtonStyle
        {
            get { return (Style)GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }

        /// <summary>Style applied to the <see cref="Popup"/> that hosts the content.</summary>
        public Style PopupStyle
        {
            get { return (Style)GetValue(PopupStyleProperty); }
            set { SetValue(PopupStyleProperty, value); }
        }

        /// <summary>Corner alignment of the popup relative to the toggle button.</summary>
        public DropdownPlacement PopupPlacement
        {
            get { return (DropdownPlacement)GetValue(PopupPlacementProperty); }
            set { SetValue(PopupPlacementProperty, value); }
        }

        /// <summary>Extra horizontal offset applied on top of <see cref="PopupPlacement"/>.</summary>
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        /// <summary>Extra vertical offset applied on top of <see cref="PopupPlacement"/>.</summary>
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _popup = GetTemplateChild(PART_Popup) as Popup;
            if (_popup != null)
            {
                _popup.Placement = PlacementMode.Custom;
                _popup.CustomPopupPlacementCallback = PlacePopup;
            }
        }

        private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            double x = PopupPlacement is DropdownPlacement.BottomRight or DropdownPlacement.TopRight
                ? targetSize.Width - popupSize.Width
                : 0;
            double y = PopupPlacement is DropdownPlacement.TopLeft or DropdownPlacement.TopRight
                ? -popupSize.Height
                : targetSize.Height;

            return new[]
            {
                new CustomPopupPlacement(new Point(x + offset.X, y + offset.Y), PopupPrimaryAxis.Vertical)
            };
        }
    }
}
