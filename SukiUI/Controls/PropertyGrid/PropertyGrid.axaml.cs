using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace SukiUI.Controls
{
    public partial class PropertyGrid : UserControl
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026",
            Justification = "Avalonia raises this handler when Item changes; the requirement is stated on SetItem and InstanceViewModel, and an app that trims must keep the public properties of the items it shows.")]
        static PropertyGrid()
        {
            ItemProperty.Changed.AddClassHandler<PropertyGrid>((_, args) => OnItemChanged(args));
        }

        public PropertyGrid()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<INotifyPropertyChanged?> ItemProperty = AvaloniaProperty.Register<PropertyGrid, INotifyPropertyChanged?>(nameof(Item), defaultValue: null);

        public INotifyPropertyChanged? Item
        {
            get { return GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly StyledProperty<InstanceViewModel?> InstanceProperty = AvaloniaProperty.Register<PropertyGrid, InstanceViewModel?>(nameof(Instance), defaultValue: null);

        public InstanceViewModel? Instance
        {
            get { return GetValue(InstanceProperty); }
            set { SetValue(InstanceProperty, value); }
        }
        
        [RequiresUnreferencedCode(InstanceViewModel.ReflectsOverItem)]
        private static void OnItemChanged( AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is PropertyGrid propertyGrid)
            {
                propertyGrid.OnItemChanged(e.NewValue);
            }
        }

        [RequiresUnreferencedCode(InstanceViewModel.ReflectsOverItem)]
        private void OnItemChanged(object? newValue)
        {
            SetItem(newValue as INotifyPropertyChanged);
        }

        [RequiresUnreferencedCode(InstanceViewModel.ReflectsOverItem)]
        protected virtual void SetItem(INotifyPropertyChanged? item)
        {
            if (item is null)
            {
                return;
            }

            Instance = new InstanceViewModel(item);
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);

            Instance?.Dispose();
        }
    }
}
