using Avalonia;
using Avalonia.Controls.Primitives;

namespace SukiUI.Controls;

/// <summary>
/// Layout host that provides correct z-ordering for sliding overlay panels.
/// - Content (z=0): fills the host, base content layer
/// - OverlayContent (z=1): rendered above Content (e.g. editor panels)
/// - FlyoutContent (z=2): rendered above all layers (e.g. details flyout)
/// Z-order comes from Panel child declaration order. Animations via TranslateTransform
/// are applied by the content elements themselves.
/// </summary>
public class FlyoutOverlayHost : TemplatedControl
{
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<FlyoutOverlayHost, object?>(nameof(Content));

    public static readonly StyledProperty<object?> OverlayContentProperty =
        AvaloniaProperty.Register<FlyoutOverlayHost, object?>(nameof(OverlayContent));

    public static readonly StyledProperty<object?> FlyoutContentProperty =
        AvaloniaProperty.Register<FlyoutOverlayHost, object?>(nameof(FlyoutContent));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object? OverlayContent
    {
        get => GetValue(OverlayContentProperty);
        set => SetValue(OverlayContentProperty, value);
    }

    public object? FlyoutContent
    {
        get => GetValue(FlyoutContentProperty);
        set => SetValue(FlyoutContentProperty, value);
    }
}
