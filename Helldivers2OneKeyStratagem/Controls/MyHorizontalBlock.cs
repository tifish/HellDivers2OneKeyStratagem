﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace Helldivers2OneKeyStratagem;

public class MyHorizontalBlock : UserControl
{
    private readonly Border _border;
    private readonly StackPanel _stackPanel;

    public MyHorizontalBlock()
    {
        Content = _border = new Border
        {
            Padding = new Thickness(20, 10, 20, 10),
            Margin = new Thickness(0, 1, 0, 1),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Stretch,

            Child = _stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
            },
        };

        _border.Bind(Border.BackgroundProperty, Resources.GetResourceObservable("SystemControlBackgroundListLowBrush"));
    }

    [Content]
    public Controls Children => _stackPanel.Children;

    public bool HasBackground
    {
        get => GetValue(HasBackgroundProperty);
        set
        {
            if (HasBackground == value)
                return;

            SetValue(HasBackgroundProperty, value);

            if (value)
                _border.Bind(Border.BackgroundProperty, Resources.GetResourceObservable("SystemControlBackgroundListLowBrush"));
            else
                _border.ClearValue(Border.BackgroundProperty);
        }
    }

    public static readonly StyledProperty<bool> HasBackgroundProperty = AvaloniaProperty.Register<MyHorizontalBlock, bool>(nameof(HasBackground), true);
}
