using Avalonia;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using SukiUI.Enums;
using SukiUI.Extensions;
using SukiUI.Models;
using System.Globalization;
using Avalonia.Controls;
using SukiUI.Locale;

namespace SukiUI;

public partial class SukiTheme : Styles
{
    public static readonly StyledProperty<SukiColor> ThemeColorProperty =
        AvaloniaProperty.Register<SukiTheme, SukiColor>(nameof(Color), defaultBindingMode: BindingMode.OneTime,
            defaultValue: SukiColor.Blue);

    public static readonly StyledProperty<bool> IsRightToLeftProperty =
        AvaloniaProperty.Register<SukiTheme, bool>(nameof(IsRightToLeft), defaultBindingMode: BindingMode.OneTime,
            defaultValue: false);

    /// <summary>
    /// Used to assign the ColorTheme at launch,
    /// </summary>
    public SukiColor ThemeColor
    {
        get => GetValue(ThemeColorProperty);
        set
        {
            SetValue(ThemeColorProperty, value);
            SetColorThemeResourcesOnColorThemeChanged();
        }
    }

    public bool IsRightToLeft
    {
        get => GetValue(IsRightToLeftProperty);
        set => SetValue(IsRightToLeftProperty, value);
    }

    /// <summary>
    /// Called whenever the application's <see cref="SukiColorTheme"/> is changed.
    /// Useful where controls cannot use "DynamicResource"
    /// </summary>
    public Action<SukiColorTheme>? OnColorThemeChanged { get; set; }

    /// <summary>
    /// Called whenever the application's <see cref="ThemeVariant"/> is changed.
    /// Useful where controls need to change based on light/dark.
    /// </summary>
    public Action<ThemeVariant>? OnBaseThemeChanged { get; set; }

    /// <summary>
    /// Currently active <see cref="SukiColorTheme"/>
    /// If you want to change this please use <see cref="ChangeColorTheme(SukiUI.Models.SukiColorTheme)"/>
    /// </summary>
    public SukiColorTheme? ActiveColorTheme { get; private set; }

    /// <summary>
    /// All available Color Themes.
    /// </summary>
    public IAvaloniaReadOnlyList<SukiColorTheme> ColorThemes => _allThemes;

    /// <summary>
    /// Currently active <see cref="ThemeVariant"/>
    /// If you want to change this please use <see cref="ChangeBaseTheme"/> or <see cref="SwitchBaseTheme"/>
    /// </summary>
    public ThemeVariant ActiveBaseTheme => _app.ActualThemeVariant;

    private readonly Application _app;

    private readonly HashSet<SukiColorTheme> _colorThemeHashset = new();
    private readonly AvaloniaList<SukiColorTheme> _allThemes = new();

    private static readonly (string Key, Func<SukiThemePalette, Color?> Getter)[] PaletteTokens =
    {
        ("SukiBackground", p => p.Background),
        ("SukiStrongBackground", p => p.StrongBackground),
        ("SukiLightBackground", p => p.LightBackground),
        ("SukiCardBackground", p => p.CardBackground),
        ("SukiPopupBackground", p => p.PopupBackground),
        ("SukiGlassCardBackground", p => p.GlassCardBackground),
        ("SukiGlassCardOpaqueBackground", p => p.GlassCardOpaqueBackground),
        ("SukiBorderBrush", p => p.BorderBrush),
        ("SukiControlBorderBrush", p => p.ControlBorderBrush),
        ("SukiMediumBorderBrush", p => p.MediumBorderBrush),
        ("SukiLightBorderBrush", p => p.LightBorderBrush),
        ("SukiText", p => p.Text),
        ("SukiLowText", p => p.LowText),
        ("SukiMuteText", p => p.MuteText),
    };

    public SukiTheme()
    {
        AvaloniaXamlLoader.Load(this);
        _app = Application.Current!;
        _app.ActualThemeVariantChanged += (_, e) => OnBaseThemeChanged?.Invoke(_app.ActualThemeVariant);
        foreach (var theme in DefaultColorThemes)
            AddColorTheme(theme.Value);

        UpdateFlowDirectionResources(IsRightToLeft);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsRightToLeftProperty)
        {
            UpdateFlowDirectionResources(change.GetNewValue<bool>());
        }

        base.OnPropertyChanged(change);
    }

    /// <summary>
    /// Change the theme to one of the default themes.
    /// </summary>
    /// <param name="sukiColor">The <see cref="SukiColor"/> to change to.</param>
    public void ChangeColorTheme(SukiColor sukiColor) =>
        ThemeColor = sukiColor;

    /// <summary>
    /// Tries to change the theme to a specific theme, this can be either a default or a custom defined one.
    /// </summary>
    /// <param name="sukiColorTheme"></param>
    public void ChangeColorTheme(SukiColorTheme sukiColorTheme) =>
        SetColorTheme(sukiColorTheme);

    /// <summary>
    /// Blindly switches to the "next" theme available in the <see cref="ColorThemes"/> collection.
    /// </summary>
    public void SwitchColorTheme()
    {
        var index = -1;
        for (var i = 0; i < ColorThemes.Count; i++)
        {
            if (ColorThemes[i] != ActiveColorTheme) continue;
            index = i;
            break;
        }
        if (index == -1) return;
        var newIndex = (index + 1) % ColorThemes.Count;
        var newColorTheme = ColorThemes[newIndex];
        ChangeColorTheme(newColorTheme);
    }

    /// <summary>
    /// Add a new <see cref="SukiColorTheme"/> to the ones available, without making it active.
    /// </summary>
    /// <param name="sukiColorTheme">New <see cref="SukiColorTheme"/> to add.</param>
    public void AddColorTheme(SukiColorTheme sukiColorTheme)
    {
        if (_colorThemeHashset.Contains(sukiColorTheme))
            throw new InvalidOperationException("This color theme has already been added.");
        _colorThemeHashset.Add(sukiColorTheme);
        _allThemes.Add(sukiColorTheme);
    }

    /// <summary>
    /// Adds multiple new <see cref="SukiColorTheme"/> to the ones available, without making any active.
    /// </summary>
    /// <param name="sukiColorThemes">A collection of new <see cref="SukiColorTheme"/> to add.</param>
    public void AddColorThemes(IEnumerable<SukiColorTheme> sukiColorThemes)
    {
        foreach (var colorTheme in sukiColorThemes)
            AddColorTheme(colorTheme);
    }

    /// <summary>
    /// Tries to change the base theme to the one provided, if it is different.
    /// </summary>
    /// <param name="baseTheme"><see cref="ThemeVariant"/> to change to.</param>
    public void ChangeBaseTheme(ThemeVariant baseTheme)
    {
        if (_app.ActualThemeVariant == baseTheme) return;
        _app.RequestedThemeVariant = baseTheme;

        SetColorThemeResourcesOnColorThemeChanged();
    }

    /// <summary>
    /// Simply switches from Light -> Dark and visa versa.
    /// </summary>
    public void SwitchBaseTheme()
    {
        if (Application.Current is null) return;
        var newBase = Application.Current.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
        Application.Current.RequestedThemeVariant = newBase;

        SetColorThemeResourcesOnColorThemeChanged();
    }

    private void UpdateFlowDirectionResources(bool rightToLeft)
    {
        var primary = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        var opposite = rightToLeft ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;

        Resources["FlowDirectionPrimary"] = primary;
        Resources["FlowDirectionOpposite"] = opposite;
    }

    /// <summary>
    /// Initializes the color theme resources whenever the property is changed.
    /// In an ideal world people wouldn't use the property
    /// </summary>
    private void SetColorThemeResourcesOnColorThemeChanged()
    {
        if (!DefaultColorThemes.TryGetValue(ThemeColor, out var colorTheme))
            throw new Exception($"{ThemeColor} has no defined color theme.");
        SetColorTheme(colorTheme);
    }

    private void SetColorTheme(SukiColorTheme colorTheme)
    {
        SetColorWithOpacities("SukiPrimaryColor", colorTheme.Primary);
        SetResource("SukiPrimaryDarkColor", colorTheme.PrimaryDark);
        SetColorWithOpacities("SukiAccentColor", colorTheme.Accent);
        SetResource("SukiAccentDarkColor", colorTheme.AccentDark);

        var palette = _app.ActualThemeVariant == ThemeVariant.Dark
            ? colorTheme.DarkPalette
            : colorTheme.LightPalette;
        ApplyPaletteOverrides(palette);

        ActiveColorTheme = colorTheme;
        OnColorThemeChanged?.Invoke(ActiveColorTheme);
    }

    private void ApplyPaletteOverrides(SukiThemePalette? palette)
    {
        foreach (var (key, getter) in PaletteTokens)
        {
            var value = palette is not null ? getter(palette) : null;
            if (value.HasValue)
                _app.Resources[key] = value.Value;
            else
                _app.Resources.Remove(key);
        }
    }

    private void SetColorWithOpacities(string baseName, Color baseColor)
    {
        SetResource(baseName, baseColor);
        SetResource($"{baseName}75", baseColor.WithAlpha(0.75));
        SetResource($"{baseName}50", baseColor.WithAlpha(0.50));
        SetResource($"{baseName}25", baseColor.WithAlpha(0.25));
        SetResource($"{baseName}20", baseColor.WithAlpha(0.2));
        SetResource($"{baseName}15", baseColor.WithAlpha(0.15));
        SetResource($"{baseName}10", baseColor.WithAlpha(0.10));
        SetResource($"{baseName}7", baseColor.WithAlpha(0.07));
        SetResource($"{baseName}5", baseColor.WithAlpha(0.05));
        SetResource($"{baseName}3", baseColor.WithAlpha(0.03));
        SetResource($"{baseName}1", baseColor.WithAlpha(0.005));
        SetResource($"{baseName}0", baseColor.WithAlpha(0.00));

        if (ActiveBaseTheme == ThemeVariant.Dark)
        {
            SetResource($"{baseName}120", Lighten(baseColor,0.7));
            SetResource($"{baseName}150",  Lighten(baseColor,1));
        }
        else
        {
            SetResource($"{baseName}120", baseColor);
            SetResource($"{baseName}150", baseColor);
        }
    }

    public static Color Lighten(Color color, double amount)
    {
        amount = Clamp(amount, 0.0, 1.0);

        byte lighten(byte component)
        {
            int result = (int)(component + (255 - component) * amount);
            return (byte)Clamp(result, 0, 255);
        }

        return Color.FromArgb(
            color.A,
            lighten(color.R),
            lighten(color.G),
            lighten(color.B)
        );
    }

    public static double Clamp(double value, double min, double max)
    {
        return value < min ? min : (value > max ? max : value);
    }

    private static int Clamp(int value, int min, int max)
    {
        return value < min ? min : (value > max ? max : value);
    }

    private void SetResource(string name, Color color) =>
        _app.Resources[name] = color;

    // Static Members...

    /// <summary>
    /// The default Color Themes included with SukiUI.
    /// </summary>
    public static readonly IReadOnlyDictionary<SukiColor, SukiColorTheme> DefaultColorThemes;

    static SukiTheme()
    {
        var defaultThemes = new[]
        {
            new DefaultSukiColorTheme(SukiColor.Orange,    Color.Parse("#d48806"), Color.Parse("#176CE8")),
            new DefaultSukiColorTheme(SukiColor.Red,       Color.Parse("#D03A2F"), Color.Parse("#2FC5D0")),
            new DefaultSukiColorTheme(SukiColor.Green,     Color.Parse("#537834"), Color.Parse("#B24DB0")),
            new DefaultSukiColorTheme(SukiColor.Blue,      Color.Parse("#0A59F7"), Color.Parse("#F7A80A")),
            new DefaultSukiColorTheme(SukiColor.Purple,    Color.Parse("#7B2FD0"), Color.Parse("#56D02F")),
            new DefaultSukiColorTheme(SukiColor.Teal,      Color.Parse("#0E9AA7"), Color.Parse("#A7210E")),
            new DefaultSukiColorTheme(SukiColor.Pink,      Color.Parse("#D9468B"), Color.Parse("#46D9A4")),
            new DefaultSukiColorTheme(SukiColor.Yellow,    Color.Parse("#C4A616"), Color.Parse("#1674C4")),
            new DefaultSukiColorTheme(SukiColor.Indigo,    Color.Parse("#6236C7"), Color.Parse("#8BC736")),
            new DefaultSukiColorTheme(SukiColor.Grayscale, Color.Parse("#6B7280"), Color.Parse("#9CA3AF")),

            new DefaultSukiColorTheme(SukiColor.Nord, Color.Parse("#5E81AC"), Color.Parse("#D08770"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#2e3440"),
                    StrongBackground   = Color.Parse("#242933"),
                    LightBackground    = Color.Parse("#3b4252"),
                    CardBackground     = Color.Parse("#434c5e"),
                    PopupBackground    = Color.Parse("#2e3440"),
                    BorderBrush        = Color.Parse("#4c566a"),
                    ControlBorderBrush = Color.Parse("#434c5e"),
                    MediumBorderBrush  = Color.Parse("#3b4252"),
                    LightBorderBrush   = Color.Parse("#363d4c"),
                    Text               = Color.Parse("#eceff4"),
                    LowText            = Color.Parse("#d8dee9"),
                    MuteText           = Color.Parse("#8d96a8"),
                },
                LightPalette = new SukiThemePalette
                {
                    StrongBackground   = Color.Parse("#e5e9f0"),
                    LightBackground    = Color.Parse("#d8dee9"),
                    CardBackground     = Color.Parse("#eceff4"),
                    PopupBackground    = Color.Parse("#eceff4"),
                    BorderBrush        = Color.Parse("#81a1c1"),
                    ControlBorderBrush = Color.Parse("#8fbcbb"),
                    MediumBorderBrush  = Color.Parse("#afc5d8"),
                    LightBorderBrush   = Color.Parse("#d0d8e4"),
                    Text               = Color.Parse("#2e3440"),
                    LowText            = Color.Parse("#4c566a"),
                    MuteText           = Color.Parse("#61728a"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.Rosepine, Color.Parse("#C4A7E7"), Color.Parse("#EA9A97"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#191724"),
                    StrongBackground   = Color.Parse("#111020"),
                    LightBackground    = Color.Parse("#26233a"),
                    CardBackground     = Color.Parse("#1f1d2e"),
                    PopupBackground    = Color.Parse("#191724"),
                    BorderBrush        = Color.Parse("#403d52"),
                    ControlBorderBrush = Color.Parse("#26233a"),
                    MediumBorderBrush  = Color.Parse("#21202e"),
                    LightBorderBrush   = Color.Parse("#1e1c2e"),
                    Text               = Color.Parse("#e0def4"),
                    LowText            = Color.Parse("#908caa"),
                    MuteText           = Color.Parse("#6e6a86"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.Dracula, Color.Parse("#644ac9"), Color.Parse("#50fa7b"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#282a36"),
                    StrongBackground   = Color.Parse("#21222c"),
                    LightBackground    = Color.Parse("#343746"),
                    CardBackground     = Color.Parse("#44475a"),
                    PopupBackground    = Color.Parse("#282a36"),
                    BorderBrush        = Color.Parse("#6272a4"),
                    ControlBorderBrush = Color.Parse("#44475a"),
                    MediumBorderBrush  = Color.Parse("#3d3f4f"),
                    LightBorderBrush   = Color.Parse("#303244"),
                    Text               = Color.Parse("#f8f8f2"),
                    LowText            = Color.Parse("#bcc2e0"),
                    MuteText           = Color.Parse("#6272a4"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.Monokai, Color.Parse("#66d9ef"), Color.Parse("#fd971f"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#272822"),
                    StrongBackground   = Color.Parse("#1e1f1c"),
                    LightBackground    = Color.Parse("#33342c"),
                    CardBackground     = Color.Parse("#3e3d32"),
                    PopupBackground    = Color.Parse("#272822"),
                    BorderBrush        = Color.Parse("#75715e"),
                    ControlBorderBrush = Color.Parse("#49483e"),
                    MediumBorderBrush  = Color.Parse("#3e3d32"),
                    LightBorderBrush   = Color.Parse("#32312a"),
                    Text               = Color.Parse("#f8f8f2"),
                    LowText            = Color.Parse("#cfcfc2"),
                    MuteText           = Color.Parse("#75715e"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.TokyoNight, Color.Parse("#7aa2f7"), Color.Parse("#f7768e"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#1a1b26"),
                    StrongBackground   = Color.Parse("#16161e"),
                    LightBackground    = Color.Parse("#292e42"),
                    CardBackground     = Color.Parse("#1e2030"),
                    PopupBackground    = Color.Parse("#1a1b26"),
                    BorderBrush        = Color.Parse("#3b4261"),
                    ControlBorderBrush = Color.Parse("#292e42"),
                    MediumBorderBrush  = Color.Parse("#222436"),
                    LightBorderBrush   = Color.Parse("#1e2030"),
                    Text               = Color.Parse("#c0caf5"),
                    LowText            = Color.Parse("#a9b1d6"),
                    MuteText           = Color.Parse("#565f89"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.Everforest, Color.Parse("#a7c080"), Color.Parse("#7fbbb3"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#272e33"),
                    StrongBackground   = Color.Parse("#1e2326"),
                    LightBackground    = Color.Parse("#2e383c"),
                    CardBackground     = Color.Parse("#374145"),
                    PopupBackground    = Color.Parse("#272e33"),
                    BorderBrush        = Color.Parse("#4f585e"),
                    ControlBorderBrush = Color.Parse("#415055"),
                    MediumBorderBrush  = Color.Parse("#374145"),
                    LightBorderBrush   = Color.Parse("#2e383c"),
                    Text               = Color.Parse("#d3c6aa"),
                    LowText            = Color.Parse("#9da9a0"),
                    MuteText           = Color.Parse("#859289"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.Kanagawa, Color.Parse("#7fb4ca"), Color.Parse("#957fb8"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#1f1f28"),
                    StrongBackground   = Color.Parse("#16161d"),
                    LightBackground    = Color.Parse("#2a2a37"),
                    CardBackground     = Color.Parse("#363646"),
                    PopupBackground    = Color.Parse("#1f1f28"),
                    BorderBrush        = Color.Parse("#54546d"),
                    ControlBorderBrush = Color.Parse("#363646"),
                    MediumBorderBrush  = Color.Parse("#2a2a37"),
                    LightBorderBrush   = Color.Parse("#252530"),
                    Text               = Color.Parse("#dcd7ba"),
                    LowText            = Color.Parse("#c8c093"),
                    MuteText           = Color.Parse("#727169"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.Mocha, Color.Parse("#fab387"), Color.Parse("#cba6f7"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#1e1e2e"),
                    StrongBackground   = Color.Parse("#181825"),
                    LightBackground    = Color.Parse("#313244"),
                    CardBackground     = Color.Parse("#313244"),
                    PopupBackground    = Color.Parse("#1e1e2e"),
                    BorderBrush        = Color.Parse("#585b70"),
                    ControlBorderBrush = Color.Parse("#45475a"),
                    MediumBorderBrush  = Color.Parse("#313244"),
                    LightBorderBrush   = Color.Parse("#292938"),
                    Text               = Color.Parse("#cdd6f4"),
                    LowText            = Color.Parse("#bac2de"),
                    MuteText           = Color.Parse("#a6adc8"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.Gruvbox, Color.Parse("#d79921"), Color.Parse("#98971a"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#282828"),
                    StrongBackground   = Color.Parse("#1d2021"),
                    LightBackground    = Color.Parse("#3c3836"),
                    CardBackground     = Color.Parse("#504945"),
                    PopupBackground    = Color.Parse("#282828"),
                    BorderBrush        = Color.Parse("#665c54"),
                    ControlBorderBrush = Color.Parse("#504945"),
                    MediumBorderBrush  = Color.Parse("#3c3836"),
                    LightBorderBrush   = Color.Parse("#32302f"),
                    Text               = Color.Parse("#ebdbb2"),
                    LowText            = Color.Parse("#d5c4a1"),
                    MuteText           = Color.Parse("#a89984"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.Solarized, Color.Parse("#268bd2"), Color.Parse("#2aa198"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#002b36"),
                    StrongBackground   = Color.Parse("#001e27"),
                    LightBackground    = Color.Parse("#073642"),
                    CardBackground     = Color.Parse("#094555"),
                    PopupBackground    = Color.Parse("#002b36"),
                    BorderBrush        = Color.Parse("#586e75"),
                    ControlBorderBrush = Color.Parse("#073642"),
                    MediumBorderBrush  = Color.Parse("#003847"),
                    LightBorderBrush   = Color.Parse("#002e38"),
                    Text               = Color.Parse("#eee8d5"),
                    LowText            = Color.Parse("#93a1a1"),
                    MuteText           = Color.Parse("#657b83"),
                },
                LightPalette = new SukiThemePalette
                {
                    StrongBackground   = Color.Parse("#eee8d5"),
                    LightBackground    = Color.Parse("#e0dbc7"),
                    CardBackground     = Color.Parse("#fdf6e3"),
                    PopupBackground    = Color.Parse("#fdf6e3"),
                    BorderBrush        = Color.Parse("#93a1a1"),
                    ControlBorderBrush = Color.Parse("#adb5bd"),
                    MediumBorderBrush  = Color.Parse("#bdc3bd"),
                    LightBorderBrush   = Color.Parse("#d8d8cc"),
                    Text               = Color.Parse("#073642"),
                    LowText            = Color.Parse("#586e75"),
                    MuteText           = Color.Parse("#839496"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.Ayu, Color.Parse("#ffcc66"), Color.Parse("#ff8f40"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#0d1017"),
                    StrongBackground   = Color.Parse("#080c12"),
                    LightBackground    = Color.Parse("#1a2130"),
                    CardBackground     = Color.Parse("#131721"),
                    PopupBackground    = Color.Parse("#0d1017"),
                    BorderBrush        = Color.Parse("#253340"),
                    ControlBorderBrush = Color.Parse("#1e2b38"),
                    MediumBorderBrush  = Color.Parse("#17212b"),
                    LightBorderBrush   = Color.Parse("#121a24"),
                    Text               = Color.Parse("#b3b1ad"),
                    LowText            = Color.Parse("#8a8680"),
                    MuteText           = Color.Parse("#626a73"),
                },
            },

            new DefaultSukiColorTheme(SukiColor.OneDark, Color.Parse("#61afef"), Color.Parse("#c678dd"))
            {
                DarkPalette = new SukiThemePalette
                {
                    Background         = Color.Parse("#282c34"),
                    StrongBackground   = Color.Parse("#21252b"),
                    LightBackground    = Color.Parse("#30343d"),
                    CardBackground     = Color.Parse("#3a3f4b"),
                    PopupBackground    = Color.Parse("#282c34"),
                    BorderBrush        = Color.Parse("#4b5363"),
                    ControlBorderBrush = Color.Parse("#3e4452"),
                    MediumBorderBrush  = Color.Parse("#353b45"),
                    LightBorderBrush   = Color.Parse("#2f333d"),
                    Text               = Color.Parse("#abb2bf"),
                    LowText            = Color.Parse("#9da5b4"),
                    MuteText           = Color.Parse("#5c6370"),
                },
            },
        };
        DefaultColorThemes = defaultThemes.ToDictionary(x => x.ThemeColor, y => (SukiColorTheme)y);
    }

    /// <summary>
    /// Retrieves an instance tied to a specific instance of an application.
    /// </summary>
    /// <returns>A <see cref="SukiTheme"/> instance that can be used to change themes.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no SukiTheme has been defined in App.axaml.</exception>
    public static SukiTheme GetInstance(Application app)
    {
        var theme = app.Styles.FirstOrDefault(style => style is SukiTheme);
        if (theme is not SukiTheme sukiTheme)
            throw new InvalidOperationException(
                "No SukiTheme instance available. Ensure SukiTheme has been set in Application.Styles in App.axaml.");
        return sukiTheme;
    }

    /// <summary>
    /// Retrieves an instance tied to the currently active application.
    /// </summary>
    /// <returns>A <see cref="SukiTheme"/> instance that can be used to change themes.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no SukiTheme has been defined in App.axaml.</exception>
    public static SukiTheme GetInstance() => GetInstance(Application.Current!);

    // Localization

    private static readonly Dictionary<CultureInfo, ResourceDictionary> LocaleToResource = new()
    {
        { new CultureInfo("en-US"), new en_US() },
        { new CultureInfo("nl-NL"), new nl_NL() },
        { new CultureInfo("pt-PT"), new pt_PT() },
        { new CultureInfo("zh-CN"), new zh_CN() },
        { new CultureInfo("de-DE"), new de_DE() },
        { new CultureInfo("es-ES"), new es_ES() },
        { new CultureInfo("fr-FR"), new fr_FR() },
        { new CultureInfo("it-IT"), new it_IT() },
        { new CultureInfo("ru-RU"), new ru_RU() },
        { new CultureInfo("ja-JP"), new ja_JP() }
    };

    private static readonly ResourceDictionary DefaultResource = new en_US();

    private CultureInfo? _locale;

    public CultureInfo? Locale
    {
        get => _locale;
        set
        {
            try
            {
                if (TryGetLocaleResource(value, out var resource) && resource is not null)
                {
                    _locale = value;
                    foreach (var keyValue in resource) Resources[keyValue.Key] = keyValue.Value;
                }
                else
                {
                    _locale = new CultureInfo("en-US");
                    foreach (var keyValue in DefaultResource) Resources[keyValue.Key] = keyValue.Value;
                }
            }
            catch
            {
                _locale = CultureInfo.InvariantCulture;
            }
        }
    }

    private static bool TryGetLocaleResource(CultureInfo? locale, out ResourceDictionary? resourceDictionary)
    {
        if (Equals(locale, CultureInfo.InvariantCulture))
        {
            resourceDictionary = DefaultResource;
            return true;
        }

        if (locale is null)
        {
            resourceDictionary = DefaultResource;
            return false;
        }

        if (LocaleToResource.TryGetValue(locale, out var resource))
        {
            resourceDictionary = resource;
            return true;
        }

        resourceDictionary = DefaultResource;
        return false;
    }

    public static void OverrideLocaleResources(Application application, CultureInfo? culture)
    {
        if (culture is null) return;
        if (!LocaleToResource.TryGetValue(culture, out var resources)) return;
        foreach (var keyValue in resources)
        {
            application.Resources[keyValue.Key] = keyValue.Value;
        }
    }

    public static void OverrideLocaleResources(StyledElement element, CultureInfo? culture)
    {
        if (culture is null) return;
        if (!LocaleToResource.TryGetValue(culture, out var resources)) return;
        foreach (var keyValue in resources)
        {
            element.Resources[keyValue.Key] = keyValue.Value;
        }
    }
}