using Avalonia.Media;

namespace SukiUI.Models;

/// <summary>
/// Optional per-theme surface/border overrides. Any null property uses the base Light/Dark value.
/// </summary>
public record SukiThemePalette
{
    public Color? Background { get; init; }
    public Color? StrongBackground { get; init; }
    public Color? LightBackground { get; init; }
    public Color? CardBackground { get; init; }
    public Color? PopupBackground { get; init; }
    public Color? DialogBackground { get; init; }
    public Color? ControlTouchBackground { get; init; }
    public Color? GlassCardBackground { get; init; }
    public Color? GlassCardOpaqueBackground { get; init; }
    public Color? BorderBrush { get; init; }
    public Color? ControlBorderBrush { get; init; }
    public Color? MediumBorderBrush { get; init; }
    public Color? LightBorderBrush { get; init; }
    public Color? MenuBorderBrush { get; init; }
    public Color? GlassBorderBrush { get; init; }
    public Color? Text { get; init; }
    public Color? LowText { get; init; }
    public Color? MuteText { get; init; }
    public Color? DisabledText { get; init; }

    /// <summary>
    /// Foreground painted on top of SukiPrimaryColor fills (checkmarks, checked toggles, active step glyphs).
    /// Null keeps the base white; high-contrast themes with a light primary set this to a dark value.
    /// </summary>
    public Color? PrimaryForeground { get; init; }

    // Semantic status colours. Left null on most themes so the base Light/Dark values apply;
    // high-contrast themes override them so success/warning/danger stay legible on pure black/white.
    public Color? SuccessColor { get; init; }
    public Color? WarningColor { get; init; }
    public Color? DangerColor { get; init; }
    public Color? InformationColor { get; init; }

    /// <summary>
    /// Optional glass/acrylic fill opacity (0..1). Set to 1.0 on high-contrast themes to force
    /// translucent surfaces opaque. Null keeps the base Light/Dark value.
    /// </summary>
    public double? GlassOpacity { get; init; }
}
