using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace SukiUI.Controls;

public class SukiSideMenuItem : TreeViewItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, object?>(nameof(Icon));

    private Border? _border;

    private static readonly Point s_invalidPoint = new Point(double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;
    
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, string?>(nameof(Header));

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<object> PageContentProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, object>(nameof(PageContent));

    public object PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> SideDisplayTemplateProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, IDataTemplate?>(nameof(SideDisplayTemplate));

    public IDataTemplate? SideDisplayTemplate
    {
        get => GetValue(SideDisplayTemplateProperty);
        set => SetValue(SideDisplayTemplateProperty, value);
    }

    public void Show()
    {
        if (_border == null)
        {
            return;
        }

        _border.MaxHeight = 200.0;
    }

    public void Hide()
    {
        if (_border == null)
        {
            return;
        }

        _border.MaxHeight = 0.0;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _border = e.NameScope.Get<Border>("PART_Border");

        
        // /!\ WARNING /!\ - Commented to make SubMenus work.
        
    /*    if (e.NameScope.Get<ContentPresenter>("PART_AltDisplay") is { } contentControl) 
        {
            if (Header is not null || Icon is not null)
            {
                contentControl.IsVisible = false;
            }
        } */
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        // Expander rows: toggle IsExpanded on left-button press and consume the
        // event so the base TreeViewItem selection logic never runs. Done in
        // OnPointerPressed (not Released) because the released-side branch was
        // gated on _pointerDownPoint, which is only captured for touch input;
        // mouse clicks fell through silently and the expander never toggled.
        //
        // Skip when the click actually landed on a nested child SukiSideMenuItem
        // inside this expander's items presenter -- pointer events bubble up,
        // so without this guard a click on "Script Lab" would also toggle the
        // parent "Scripts" expander.
        if (IsExpander
            && e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed
            && !ClickCameFromNestedItem(e.Source as Visual))
        {
            IsExpanded = !IsExpanded;
            e.Handled = true;
            return;
        }

        base.OnPointerPressed(e);

        _pointerDownPoint = s_invalidPoint;

        if (e.Handled)
            return;

        if (!e.Handled && ItemsControl.ItemsControlFromItemContainer(this) is SukiSideMenu owner)
        {
            var p = e.GetCurrentPoint(this);

            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or
                PointerUpdateKind.RightButtonPressed)
            {
                if (p.Pointer.Type == PointerType.Mouse)
                {
                    // If the pressed point comes from a mouse, perform the selection immediately.


                    // /!\ WARNING /!\ - Line commented to make it work with subitems. Doesn't seem to break anything on my
                        // e.Handled = owner.UpdateSelectionFromPointerEvent(this);
                }
                else
                {
                    // Otherwise perform the selection when the pointer is released as to not
                    // interfere with gestures.
                    _pointerDownPoint = p.Position;

                    // Ideally we'd set handled here, but that would prevent the scroll gesture
                    // recognizer from working.
                    ////e.Handled = true;
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!e.Handled &&
            !double.IsNaN(_pointerDownPoint.X) &&
            e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point = e.GetCurrentPoint(this);
            var settings = (e.Source as Visual)?.GetPlatformSettings();
            var tapSize = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
            var tapRect = new Rect(_pointerDownPoint, new Size())
                .Inflate(new Thickness(tapSize.Width, tapSize.Height));

            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                tapRect.ContainsExclusive(point.Position) &&
                ItemsControl.ItemsControlFromItemContainer(this) is SukiSideMenu owner &&
                // Expander rows are handled entirely in OnPointerPressed; skip the
                // selection-on-release path so a touch tap doesn't both toggle (on
                // press, via the IsExpander branch above) AND try to select here.
                !IsExpander)
            {
                if (owner.UpdateSelectionFromPointerEvent(this))
                {
                    // As we only update selection from touch/pen on pointer release, we need to raise
                    // the pointer event on the owner to trigger a commit.
                    if (e.Pointer.Type != PointerType.Mouse)
                    {
                        var sourceBackup = e.Source;
                        owner.RaiseEvent(e);
                        e.Source = sourceBackup;
                    }

                    e.Handled = true;
                }
            }
        }

        _pointerDownPoint = s_invalidPoint;
    }
    
    public static readonly StyledProperty<bool> IsContentMovableProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, bool>(nameof(IsContentMovable), defaultValue: true);

    public bool IsContentMovable
    {
        get => GetValue(IsContentMovableProperty);
        set => SetValue(IsContentMovableProperty, value);
    }

    public static readonly StyledProperty<bool> IsTopMenuExpandedProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, bool>(nameof(IsTopMenuExpanded), defaultValue: true);

    public bool IsTopMenuExpanded
    {
        get => GetValue(IsTopMenuExpandedProperty);
        set => SetValue(IsTopMenuExpandedProperty, value);
    }

    /// <summary>
    /// When true, clicks on the row toggle <see cref="TreeViewItem.IsExpanded"/>
    /// instead of selecting the item. Use for group-header rows that own a
    /// nested ItemsSource: the row acts purely as an expand/collapse control
    /// and is never the navigation target. The chevron in the template's
    /// PART_ExpandedButton continues to work the same way.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpanderProperty =
        AvaloniaProperty.Register<SukiSideMenuItem, bool>(nameof(IsExpander), defaultValue: false);

    public bool IsExpander
    {
        get => GetValue(IsExpanderProperty);
        set => SetValue(IsExpanderProperty, value);
    }

    // Nested-children container generation. SukiSideMenu does the same trick at
    // the root level: build the parent ItemTemplate against the data item and
    // use the resulting SukiSideMenuItem AS the container. We mirror that here
    // so children of an expander row also pass through the configured template
    // (rather than rendering as plain TreeViewItem instances with no styling).
    // Falls back to a bare SukiSideMenuItem when the template doesn't match.
    //
    // IsTopMenuExpanded is propagated to the new child so it picks up the
    // sidebar's current expansion state at creation time -- the root-level
    // UpdateMenuItemsExpansion loop only sees top-level items, and the
    // OnPropertyChanged hook below keeps subsequent toggles in sync.
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var child = (ItemTemplate != null && ItemTemplate.Match(item) &&
                     ItemTemplate.Build(item) is SukiSideMenuItem sukiMenuItem)
            ? sukiMenuItem
            : new SukiSideMenuItem();
        child.IsTopMenuExpanded = IsTopMenuExpanded;
        return child;
    }

    // True when the pointer-event source is inside a descendant SukiSideMenuItem
    // (i.e. a child row of an expander). Walks up the visual tree; if we hit
    // another SukiSideMenuItem before reaching this control, the click was on a
    // nested item, not on the expander row's own chrome.
    private bool ClickCameFromNestedItem(Visual? source)
    {
        var current = source;
        while (current != null && !ReferenceEquals(current, this))
        {
            if (current is SukiSideMenuItem) return true;
            current = current.GetVisualParent();
        }
        return false;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<SukiSideMenuItem>(item, out recycleKey);
    }

    // Cascade IsTopMenuExpanded into realized nested children so child rows
    // adopt the correct selection styling (the base IsSelected style branches
    // on IsTopMenuExpanded and would otherwise slide the icon off-screen when
    // the sidebar is collapsed).
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsTopMenuExpandedProperty)
        {
            var newVal = change.GetNewValue<bool>();
            foreach (var c in this.GetRealizedContainers())
            {
                if (c is SukiSideMenuItem nested)
                    nested.IsTopMenuExpanded = newVal;
            }
        }
    }
}
