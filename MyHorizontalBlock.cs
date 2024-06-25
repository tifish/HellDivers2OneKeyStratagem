using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using iNKORE.UI.WPF.Modern.Controls;

namespace HellDivers2OneKeyStratagem;

[ContentProperty(nameof(Children))]
public class MyHorizontalBlock : UserControl
{
    private readonly Border _border;

    public MyHorizontalBlock()
    {
        _border = new Border
        {
            Padding = new Thickness(20, 10, 20, 10),
            Margin = new Thickness(0, 1, 0, 1),
            Height = double.NaN,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        _border.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
        Content = _border;

        var simpleStackPanel = new SimpleStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
        };
        _border.Child = simpleStackPanel;

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

    public bool HasBackground
    {
        get => (bool)GetValue(HasBackgroundProperty);
        set => SetValue(HasBackgroundProperty, value);
    }

    public static readonly DependencyProperty HasBackgroundProperty = DependencyProperty.Register(
        nameof(HasBackground),
        typeof(bool),
        typeof(MyHorizontalBlock),
        new PropertyMetadata(true, OnHasBackgroundChanged));

    private static void OnHasBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (MyHorizontalBlock)d;
        var hasBackground = (bool)e.NewValue;
        if (hasBackground)
            control._border.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
        else
            control._border.ClearValue(Border.BackgroundProperty);
    }
}
