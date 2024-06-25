using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using iNKORE.UI.WPF.Modern.Controls;

namespace HellDivers2OneKeyStratagem;

[ContentProperty(nameof(Children))]
public class MyHorizontalBlock : UserControl
{
    public MyHorizontalBlock()
    {
        var border = new Border
        {
            Padding = new Thickness(20, 10, 20, 10),
            Margin = new Thickness(0, 1, 0, 1),
            Height = double.NaN,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        border.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
        Content = border;

        var simpleStackPanel = new SimpleStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
        };
        border.Child = simpleStackPanel;

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
        typeof(MyHorizontalBlock),
        new PropertyMetadata());
}
