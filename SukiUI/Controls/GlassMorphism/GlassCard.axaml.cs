using System;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (ContextMenu is null) return;
        ContextMenu.Opening += ContextMenuOnOpening;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (IsAnimated)
        {
            var b = e.NameScope.Get<Panel>("RootPanel");
            b.Loaded += (sender, args) =>
            {
                var v = ElementComposition.GetElementVisual(b);
                CompositionAnimationHelper.MakeOpacityAnimated(v);
            };

            var b2 = e.NameScope.Get<Border>("PART_BorderCardLight");
            b2.Loaded += (sender, args) =>
            {
                var v = ElementComposition.GetElementVisual(b2);
                CompositionAnimationHelper.MakeSizeAnimated(v);
            };
            
            var b2d = e.NameScope.Get<Border>("PART_BorderCardDark");
            b2d.Loaded += (sender, args) =>
            {
                // b2d, not b2: animating the light border a second time left the dark
                // border with no size animation at all.
                var v = ElementComposition.GetElementVisual(b2d);
                CompositionAnimationHelper.MakeSizeAnimated(v);
            };

            var b3 = e.NameScope.Get<Border>("PART_ClipBorder");
            b3.Loaded += (sender, args) =>
            {
                var v = ElementComposition.GetElementVisual(b3);
                CompositionAnimationHelper.MakeSizeAnimated(v);
            };

        }

    }
    



    private void ContextMenuOnOpening(object sender, CancelEventArgs e)
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