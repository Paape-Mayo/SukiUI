using Avalonia.Data.Converters;

namespace SukiUI.Converters;

public static class DoubleConvert
{
    /// <summary>
    /// True when the value is finite and not negative. Used to gate visibility of
    /// determinate-progress UI on a sentinel default of -1 (or NaN).
    /// </summary>
    public static readonly IValueConverter NonNegativeToBool =
        new FuncValueConverter<double, bool>(v => !double.IsNaN(v) && v >= 0);
}
