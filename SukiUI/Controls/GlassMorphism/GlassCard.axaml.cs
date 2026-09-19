using System;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using SukiUI.Enums;
using SukiUI.Helpers;

namespace SukiUI.Controls;

public class GlassCard : ContentControl
{
    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<GlassCard, CornerRadius>(nameof(CornerRadius), new CornerRadius(20));

    public new CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public new static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<GlassCard, Thickness>(nameof(BorderThickness), new Thickness(1));

    public new Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    
 
    public static readonly StyledProperty<bool> IsAnimatedProperty =
        AvaloniaProperty.Register<GlassCard, bool>(nameof(IsAnimated), true);

    public bool IsAnimated
    {
        get => GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsOpaqueProperty =
        AvaloniaProperty.Register<GlassCard, bool>(nameof(IsOpaque), false);

    public bool IsOpaque
    {
        get => GetValue(IsOpaqueProperty);
        set => SetValue(IsOpaqueProperty, value);
    }

    public static readonly StyledProperty<bool> IsInteractiveProperty = AvaloniaProperty.Register<GlassCard, bool>(nameof(IsInteractive));

    public bool IsInteractive
    {
        get => GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    /// <summary>
    /// Resting drop shadow. Defaults to <see cref="GlassCardElevation.None"/>, which is
    /// exactly today's appearance for every existing card.
    /// </summary>
    public static readonly StyledProperty<GlassCardElevation> ElevationProperty =
        AvaloniaProperty.Register<GlassCard, GlassCardElevation>(nameof(Elevation), GlassCardElevation.None);

    public GlassCardElevation Elevation
    {
        get => GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>
    /// Opt in to a hover lift: the card translates up slightly and gains a shadow while the
    /// pointer is over it, settling back on press.
    ///
    /// <para>Deliberately a separate flag rather than a behaviour keyed on
    /// <see cref="IsInteractive"/>. IsInteractive is already set inside four SukiUI control
    /// templates (CheckBox, RadioButton, the Shadcn styles, TouchNavigationStack), so keying
    /// the lift on it would make every checkbox and radio button in the app jump on hover.</para>
    /// </summary>
    public static readonly StyledProperty<bool> IsHoverLiftedProperty =
        AvaloniaProperty.Register<GlassCard, bool>(nameof(IsHoverLifted), false);

    public bool IsHoverLifted
    {
        get => GetValue(IsHoverLiftedProperty);
        set => SetValue(IsHoverLiftedProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<GlassCard, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<GlassCard, object?>(nameof(CommandParameter));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private Panel? _animationRoot;
    private Border?[] _animatedBorders = Array.Empty<Border?>();
    private ContextMenu? _subscribedContextMenu;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Detached cards can receive ContentControl's fallback template. Animation
        // parts are optional; resolve them again when the themed template is applied.
        _animationRoot = e.NameScope.Find<Panel>("RootPanel");
        _animatedBorders = new[] { e.NameScope.Find<Border>("PART_BorderCardLight"),
            e.NameScope.Find<Border>("PART_BorderCardDark"), e.NameScope.Find<Border>("PART_ClipBorder") };
        if (IsLoaded) UpdateAnimations();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateAnimations();
        UpdateContextMenuSubscription(ContextMenu);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        UpdateContextMenuSubscription(null);
        base.OnUnloaded(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!IsLoaded) return;
        if (change.Property == IsAnimatedProperty) UpdateAnimations();
        if (change.Property == ContextMenuProperty) UpdateContextMenuSubscription(ContextMenu);
    }

    private void UpdateAnimations()
    {
        if (_animationRoot is not null && ElementComposition.GetElementVisual(_animationRoot) is { } root)
        {
            if (IsAnimated) CompositionAnimationHelper.MakeOpacityAnimated(root);
            else root.ImplicitAnimations = null;
        }
        foreach (var border in _animatedBorders)
        {
            if (border is null || ElementComposition.GetElementVisual(border) is not { } visual) continue;
            if (IsAnimated) CompositionAnimationHelper.MakeSizeAnimated(visual);
            else visual.ImplicitAnimations = null;
        }
    }

    private void UpdateContextMenuSubscription(ContextMenu? menu)
    {
        if (_subscribedContextMenu is not null) _subscribedContextMenu.Opening -= ContextMenuOnOpening;
        _subscribedContextMenu = menu;
        if (menu is not null) menu.Opening += ContextMenuOnOpening;
    }

    private void ContextMenuOnOpening(object? sender, CancelEventArgs e)
    {
        PseudoClasses.Set(":pointerdown", false);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PseudoClasses.Set(":pointerdown", true);
        if(IsInteractive && Command is not null && Command.CanExecute(CommandParameter))
            Command.Execute(CommandParameter);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Set(":pointerdown", false);
    }
}
