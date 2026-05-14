using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls;

public partial class BusyArea : UserControl
{
    public BusyArea()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<BusyArea, bool>(nameof(IsBusy), defaultValue: false);

    public bool IsBusy
    {
        get { return GetValue(IsBusyProperty); }
        set { SetValue(IsBusyProperty, value); }
    }

    public static readonly StyledProperty<string?> BusyTextProperty =
        AvaloniaProperty.Register<BusyArea, string?>(nameof(BusyText), defaultValue: null);

    public string? BusyText
    {
        get => GetValue(BusyTextProperty);
        set => SetValue(BusyTextProperty, value);
    }

    /// <summary>
    /// Determinate progress 0-100. Default -1 hides the progress bar so the
    /// indeterminate spinner shows alone. Sites that report determinate progress
    /// (e.g. via <c>ViewModelBase.LoadingProgress</c>) set this to drive a bar
    /// under the busy text.
    /// </summary>
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<BusyArea, double>(nameof(Progress), defaultValue: -1d);

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }
}