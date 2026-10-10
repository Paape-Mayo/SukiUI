using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using SukiUI.Helpers;

namespace SukiUI.Controls.Touch;

public partial class TouchNavigationStack : UserControl
{
    private object? _originalContent;
    private Control? _currentPage;
    private string? _currentTitle;


    public TouchNavigationStack()
    {
        InitializeComponent();
        // Récupère le contenu d'origine lors du chargement
        this.AttachedToVisualTree += async (_, __) =>
        {
            if (_originalContent == null)
            {
                _originalContent = Content;
                UpdateContent();
                
                await Task.Delay(700);
                
                await this.Get<DockPanel>("DP").Animate(OpacityProperty).From(0).To(1).RunAsync();
            }
        };
    }

    public new static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<TouchNavigationStack, object>(nameof(Content));

    public new object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TouchNavigationStack, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // Affiche une nouvelle page (contrôle Avalonia), avec un titre
    public void Push(Control page, string title)
    {
        _currentPage = page;
        _currentTitle = title;
        UpdateContent();
    }

    // Revient à la page d'origine
    public void Pop()
    {
        _currentPage = null;
        _currentTitle = null;
        UpdateContent();
    }

    private void UpdateContent()
    {
      
            
        
        var headerPanel = this.Get<StackPanel>("HeaderPanel");
        var backButton = this.Get<Button>("BackButton");
        var titleText = this.FindControl<TextBlock>("TitleText");
        var contentPresenter = this.Get<ContentControl>("ContentP");

        if (_currentPage != null)
        {
            // Affiche le header (titre + bouton retour)
            headerPanel.IsVisible = true;
            
        
            SetContentPres(contentPresenter, _currentPage, new Action(() =>
            {
                if (backButton.Width != 160)
                    _ = backButton.Animate(WidthProperty).From(0).To(160).RunAsync();
                
                SetTitle( _currentTitle ?? Title);
            }));

            
        }
        else
        {
            // Cache le header et affiche le contenu d'origine
            headerPanel.IsVisible = true;
            SetContentPres(contentPresenter, _originalContent, new Action(() =>
            {
                _ = backButton.Animate(WidthProperty).From(160).To(0).RunAsync();
                SetTitle( _currentTitle ?? Title);
                
            }));
            
            
        }
        
       
    }

    private void SetTitle(string currentTitle)
    {
        var titleTextfrom = this.Get<TextBlock>("TitleTextFrom");
        var titleTextto = this.Get<TextBlock>("TitleTextTo");
        
        
        titleTextfrom.Text = titleTextto.Text;
        
        titleTextfrom.Opacity = 1;
        titleTextto.Opacity = 0;

        titleTextto.Text = currentTitle;
        
        _ = titleTextto.Animate(OpacityProperty).From(0).To(1).RunAsync();
        _ = titleTextfrom.Animate(OpacityProperty).From(1).To(0).RunAsync();

    }

    private async void SetContentPres(ContentControl contentP, object? c, Action action)
    {
        await contentP.AnimateAsync<double>(OpacityProperty, 1, 0, TimeSpan.FromMilliseconds(250));
        
        contentP!.Content = c;

        action();
        await contentP.AnimateAsync<double>(OpacityProperty, 0, 1, TimeSpan.FromMilliseconds(250));
    }

    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Pop();
    }
}


public static class AnimationExtensions
{
    public static Task AnimateAsync<T>(
        this Animatable control, 
        AvaloniaProperty property, 
        T from, 
        T to, 
        TimeSpan? duration = null)
    {
        var animation = new Avalonia.Animation.Animation
        {
            Duration = duration ?? TimeSpan.FromMilliseconds(500),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = property, Value = from } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = property, Value = to } },
                    KeyTime = duration ?? TimeSpan.FromMilliseconds(500)
                }
            }
        };
        
        // On crée un token, mais ici, on retourne la Task de RunAsync, donc c'est awaitable.
        var tokenSource = new CancellationTokenSource();
        return animation.RunAsync(control, tokenSource.Token);
    }
}