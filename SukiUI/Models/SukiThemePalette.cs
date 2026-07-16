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
    public Color? GlassCardBackground { get; init; }
    public Color? GlassCardOpaqueBackground { get; init; }
    public Color? BorderBrush { get; init; }
    public Color? ControlBorderBrush { get; init; }
    public Color? MediumBorderBrush { get; init; }
    public Color? LightBorderBrush { get; init; }
    public Color? Text { get; init; }
    public Color? LowText { get; init; }
    public Color? MuteText { get; init; }
}
