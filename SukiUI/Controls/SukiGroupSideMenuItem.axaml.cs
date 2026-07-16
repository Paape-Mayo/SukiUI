using Avalonia;
using Avalonia.Media;
using SukiUI.Enums;

namespace SukiUI.Controls;

/// <summary>
/// A SukiSideMenuItem subclass that participates in a visual "group":
/// a connected colored rectangle that spans all sibling group members.
/// The host application is responsible for setting the four brush properties
/// (typically derived from the active SukiUI color theme) and for updating
/// IsGroupHovered / IsGroupActive in response to pointer and selection events.
/// </summary>
public class SukiGroupSideMenuItem : SukiSideMenuItem
{
    // ── Segment position ────────────────────────────────────────────────────

    public static readonly StyledProperty<SukiGroupSegmentPosition> SegmentPositionProperty =
        AvaloniaProperty.Register<SukiGroupSideMenuItem, SukiGroupSegmentPosition>(
            nameof(SegmentPosition),
            defaultValue: SukiGroupSegmentPosition.Sole);

    public SukiGroupSegmentPosition SegmentPosition
    {
        get => GetValue(SegmentPositionProperty);
        set => SetValue(SegmentPositionProperty, value);
    }

    // ── Group state flags ────────────────────────────────────────────────────

    public static readonly StyledProperty<bool> IsGroupHoveredProperty =
        AvaloniaProperty.Register<SukiGroupSideMenuItem, bool>(nameof(IsGroupHovered));

    public bool IsGroupHovered
    {
        get => GetValue(IsGroupHoveredProperty);
        set => SetValue(IsGroupHoveredProperty, value);
    }

    public static readonly StyledProperty<bool> IsGroupActiveProperty =
        AvaloniaProperty.Register<SukiGroupSideMenuItem, bool>(nameof(IsGroupActive));

    public bool IsGroupActive
    {
        get => GetValue(IsGroupActiveProperty);
        set => SetValue(IsGroupActiveProperty, value);
    }

    // ── Brush properties — set by the host app ───────────────────────────────

    /// <summary>Faint group tint shown at rest (~7% opacity base color).</summary>
    public static readonly StyledProperty<IBrush?> GroupRestBrushProperty =
        AvaloniaProperty.Register<SukiGroupSideMenuItem, IBrush?>(nameof(GroupRestBrush));

    public IBrush? GroupRestBrush
    {
        get => GetValue(GroupRestBrushProperty);
        set => SetValue(GroupRestBrushProperty, value);
    }

    /// <summary>Brighter tint shown while any group member is hovered (~15% opacity).</summary>
    public static readonly StyledProperty<IBrush?> GroupHoverBrushProperty =
        AvaloniaProperty.Register<SukiGroupSideMenuItem, IBrush?>(nameof(GroupHoverBrush));

    public IBrush? GroupHoverBrush
    {
        get => GetValue(GroupHoverBrushProperty);
        set => SetValue(GroupHoverBrushProperty, value);
    }

    /// <summary>Tint shown while any group member is selected (~10% opacity).</summary>
    public static readonly StyledProperty<IBrush?> GroupActiveBrushProperty =
        AvaloniaProperty.Register<SukiGroupSideMenuItem, IBrush?>(nameof(GroupActiveBrush));

    public IBrush? GroupActiveBrush
    {
        get => GetValue(GroupActiveBrushProperty);
        set => SetValue(GroupActiveBrushProperty, value);
    }

    /// <summary>Full-opacity color used for the compact-mode left border strip.</summary>
    public static readonly StyledProperty<IBrush?> GroupStripBrushProperty =
        AvaloniaProperty.Register<SukiGroupSideMenuItem, IBrush?>(nameof(GroupStripBrush));

    public IBrush? GroupStripBrush
    {
        get => GetValue(GroupStripBrushProperty);
        set => SetValue(GroupStripBrushProperty, value);
    }

    // ── Compact strip toggle ─────────────────────────────────────────────────

    /// <summary>
    /// When true and the sidebar is collapsed (IsTopMenuExpanded=False),
    /// a colored left-border strip is shown instead of the full background tint.
    /// </summary>
    public static readonly StyledProperty<bool> ShowCompactStripProperty =
        AvaloniaProperty.Register<SukiGroupSideMenuItem, bool>(
            nameof(ShowCompactStrip),
            defaultValue: true);

    public bool ShowCompactStrip
    {
        get => GetValue(ShowCompactStripProperty);
        set => SetValue(ShowCompactStripProperty, value);
    }
}
