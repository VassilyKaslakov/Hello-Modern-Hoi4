using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace Hoi4Launcher.Models;

public sealed class ThemeSwatch
{
    public required string Name { get; init; }
    public required Color Color { get; init; }

    public SolidColorBrush Brush
    {
        get
        {
            var brush = new SolidColorBrush(Color);
            brush.Freeze();
            return brush;
        }
    }
}
