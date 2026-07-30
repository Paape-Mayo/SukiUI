using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using SukiUI.ColorTheme;
using SukiUI.Enums;

namespace SukiUI.Controls;

public class InfoBadge : HeaderedContentControl
{
    private Border? _badgeContainer;

    public static readonly StyledProperty<NotificationType> AppearanceProperty = AvaloniaProperty.Register<InfoBadge, NotificationType>(nameof(Appearance), NotificationType.Information);
    public NotificationType Appearance {
        get => GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    public static readonly StyledProperty<CornerPosition> CornerPositionProperty = AvaloniaProperty.Register<InfoBadge, CornerPosition>(nameof(CornerPosition));
    public CornerPosition CornerPosition {
        get => GetValue(CornerPositionProperty);
        set => SetValue(CornerPositionProperty, value);
    }

    public static readonly StyledProperty<bool> IsDotProperty = AvaloniaProperty.Register<InfoBadge, bool>(nameof(IsDot), false);
    public bool IsDot {
        get => GetValue(IsDotProperty);
        set => SetValue(IsDotProperty, value);
    }

    public static readonly StyledProperty<int> OverflowProperty = AvaloniaProperty.Register<InfoBadge, int>(nameof(Overflow));
    public int Overflow {
        get => GetValue(OverflowProperty);
        set => SetValue(OverflowProperty, value);
    }

    static InfoBadge()
    {
        // Appearance is resolved by the ControlTheme's [Appearance=...] selectors rather
        // than by writing Background here. A code-behind write lands at LocalValue
        // priority, which permanently outranks both the theme and any Background the
        // consumer sets; it also never ran for the default value, because Avalonia only
        // raises Changed on an effective-value change, so an unstyled badge drew no fill.
        HeaderProperty.Changed.AddClassHandler<InfoBadge>((badge, _) => badge.UpdateBadgePosition());

        // Must be a static handler, not the CLR setter: property setters are bypassed
        // whenever a Style, a Binding or SetValue supplies the value.
        IsDotProperty.Changed.AddClassHandler<InfoBadge>((badge, _) => badge.UpdateBadgePosition());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _badgeContainer = e.NameScope.Find<Border>("BadgeBorder");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateBadgePosition();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        UpdateBadgePosition();
        return base.ArrangeOverride(finalSize);
    }

    private void UpdateBadgePosition()
    {
        var verticalOffset = -1;
        if (CornerPosition is CornerPosition.BottomLeft or CornerPosition.BottomRight) {
            verticalOffset = 1;
        }

        var horizontalOffset = -1;
        if (CornerPosition is CornerPosition.TopRight or CornerPosition.BottomRight) {
            horizontalOffset = 1;
        }

        if (_badgeContainer is not null && Presenter?.Child is not null) {
            _badgeContainer.RenderTransform = new TransformGroup {
                Children = [
                    new TranslateTransform(
                        horizontalOffset * _badgeContainer.Bounds.Width / 2,
                        verticalOffset * _badgeContainer.Bounds.Height / 2
                    )
                ]
            };
        }
    }
}