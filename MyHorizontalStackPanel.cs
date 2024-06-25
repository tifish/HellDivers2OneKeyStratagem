using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using iNKORE.UI.WPF.Modern.Controls;

namespace HellDivers2OneKeyStratagem;

[ContentProperty(nameof(Children))]
public class MyHorizontalStackPanel : UserControl
{
    public MyHorizontalStackPanel()
    {
        var simpleStackPanel = new SimpleStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 10, 0, 10),
        };
        Content = simpleStackPanel;

        Children = simpleStackPanel.Children;
    }

    public UIElementCollection Children
    {
        get => (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty);
        private set => SetValue(ChildrenProperty, value);
    }

    public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
        nameof(Children),
        typeof(UIElementCollection),
        typeof(MyHorizontalStackPanel),
        new PropertyMetadata());
}
