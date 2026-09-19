using System;
using Avalonia.Media;
using Avalonia.Styling;
using SukiUI.Enums;

namespace SukiUI.Models;

public record SukiColorTheme
{
    public string DisplayName { get; }

    /// <summary>
    /// When set, selecting this theme also snaps the application's base <see cref="ThemeVariant"/>
    /// to the given value. Themes that only make sense on one base (true-black, high-contrast)
    /// use this so their palette is never applied over the wrong base. Null leaves the base as-is.
    /// </summary>
    public ThemeVariant? PreferredBaseTheme { get; init; }

    public Color Primary { get; }

    public IBrush PrimaryBrush => new SolidColorBrush(Primary);
    
    public Color PrimaryDark { get; }

    public IBrush PrimaryDarkBrush => new SolidColorBrush(PrimaryDark);

    public Color Accent { get; }

    public IBrush AccentBrush => new SolidColorBrush(Accent);

    public Color AccentDark { get; }

    public IBrush AccentDarkBrush => new SolidColorBrush(AccentDark);

    public SukiThemePalette? LightPalette { get; init; }

    public SukiThemePalette? DarkPalette { get; init; }

    // Used in shaders to save calculating them per-frame.
    internal Color BackgroundPrimary { get; }
    internal Color BackgroundAccent { get; }
    internal Color Background => DarkPalette?.Background ?? DarkPalette?.StrongBackground ?? GetBackgroundColor(Primary);

    internal Color BackgroundFor(ThemeVariant variant) => variant == ThemeVariant.Dark
        ? Background
        : LightPalette?.Background ?? LightPalette?.PopupBackground ?? Color.Parse("#f2f2f2");

    internal SukiThemePalette SurfacePalette(ThemeVariant variant)
    {
        var dark = variant == ThemeVariant.Dark;
        var palette = (dark ? DarkPalette : LightPalette) ?? new SukiThemePalette();
        var background = BackgroundFor(variant);
        var raised = Mix(background, dark ? Colors.White : Colors.Black, dark ? 0.065 : 0.025);
        return palette with
        {
            CardBackground = palette.CardBackground ?? raised,
            PopupBackground = palette.PopupBackground ?? raised,
            GlassCardOpaqueBackground = palette.GlassCardOpaqueBackground ?? raised,
            ControlTouchBackground = palette.ControlTouchBackground ?? Mix(background, dark ? Colors.White : Colors.Black, 0.12),
            MenuBorderBrush = palette.MenuBorderBrush ?? palette.ControlBorderBrush ?? Mix(background, dark ? Colors.White : Colors.Black, 0.22)
        };
    }

    private static Color Mix(Color basis, Color tint, double amount) => Color.FromRgb(
        (byte)Math.Round(basis.R + (tint.R - basis.R) * amount),
        (byte)Math.Round(basis.G + (tint.G - basis.G) * amount),
        (byte)Math.Round(basis.B + (tint.B - basis.B) * amount));

    // dark scale...
    private const double dS = 0.5;

    public SukiColorTheme(string displayName, Color primary, Color accent)
    {
        DisplayName = displayName;
        Primary = primary;
        Accent = accent;
        PrimaryDark = new Color(primary.A, (byte)(primary.R * dS), (byte)(primary.G * dS), (byte)(primary.B * dS));
        AccentDark = new Color(accent.A, (byte)(accent.R * dS), (byte)(accent.G * dS), (byte)(accent.B * dS));
        BackgroundPrimary = new Color(primary.A, (byte)(primary.R / 1), (byte)(primary.G / 1), (byte)(primary.B / 1));
        BackgroundAccent = new Color(accent.A, (byte)(accent.R / 1), (byte)(accent.G / 1), (byte)(accent.B / 1));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash *= 31 + Primary.GetHashCode();
            hash *= 31 + Accent.GetHashCode();
            hash *= 31 + DisplayName.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return DisplayName;
    }

    private static Color GetBackgroundColor(Color input)
    {
        return Mix(Color.Parse("#252529"), input, 0.10);
    }
}

internal record DefaultSukiColorTheme : SukiColorTheme
{
    internal SukiColor ThemeColor { get; }

    internal DefaultSukiColorTheme(SukiColor themeColor, Color primary, Color accent)
        : base(themeColor.ToString(), primary, accent)
    {
        ThemeColor = themeColor;
    }
}
