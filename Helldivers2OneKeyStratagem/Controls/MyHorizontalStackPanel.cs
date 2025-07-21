using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace Helldivers2OneKeyStratagem;

public class MyHorizontalStackPanel : UserControl
{
    private readonly StackPanel _stackPanel;

    public MyHorizontalStackPanel()
    {
        Content = _stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 10, 0, 10),
        };
    }

    [Content]
    public Controls Children => _stackPanel.Children;
}
