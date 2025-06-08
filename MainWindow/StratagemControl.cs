using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;
using Jeek.Avalonia.Localization;

namespace HellDivers2OneKeyStratagem;

public class StratagemControl : UserControl
{
    private readonly StackPanel _stackPanel;
    private readonly Image _image;
    private readonly Label _label;

    public Stratagem? Stratagem
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            if (field == null)
            {
                _image.Source = null;
                _image.IsVisible = true;
                _image.Source = IconManager.GetNoneIcon();
                _label.IsVisible = false;
                _label.Content = Localizer.Get("None");
            }
            else
            {
                _label.Content = field.Name;

                var icon = IconManager.GetIcon(field.Id);
                if (icon == null)
                {
                    _image.Source = null;
                    _image.IsVisible = false;
                    _label.IsVisible = true;
                }
                else
                {
                    _image.Source = icon;
                    _image.IsVisible = true;
                    _label.IsVisible = false;
                }
            }

            UpdateToolTip();
        }
    }

    public StratagemControl()
    {
        Content = _stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
        };

        _image = new Image
        {
            Width = 64,
            Height = 64,
            Stretch = Avalonia.Media.Stretch.Uniform,
            IsVisible = true,
            Source = IconManager.GetNoneIcon(),
        };
        _stackPanel.Children.Add(_image);

        _label = new Label
        {
            Content = Localizer.Get("None"),
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _stackPanel.Children.Add(_label);
    }

    [Content]
    public Controls Children => _stackPanel.Children;

    public void UpdateToolTip()
    {
        if (Stratagem == null)
        {
            ToolTip.SetTip(this, null);
            return;
        }

        var desc = string.Format(
                Localizer.Get("StratagemToolTip"),
                StratagemManager.GetSystemAlias(Stratagem.Name),
                StratagemManager.GetUserAlias(Stratagem.Name));

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
        };
        stackPanel.Children.Add(new TextBlock
        {
            Text = Stratagem.Name,
            FontFamily = FontFamily,
            FontSize = FontSize + 3,
            FontWeight = Avalonia.Media.FontWeight.Bold,
        });
        stackPanel.Children.Add(new TextBlock
        {
            Text = desc,
            FontFamily = FontFamily,
            FontSize = FontSize,
        });

        ToolTip.SetTip(this, stackPanel);
    }
}
