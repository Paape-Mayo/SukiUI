using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Theme.Density;

public partial class DensityCompact : ResourceDictionary
{
    public DensityCompact()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
