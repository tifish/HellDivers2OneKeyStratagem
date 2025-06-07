using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace HellDivers2OneKeyStratagem;

public class StratagemControl : UserControl
{
    private readonly StackPanel _stackPanel;
    private readonly Image _image;
    private readonly Label _label;

    public Stratagem Stratagem
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;

            _label.Content = field.Name;

            var icon = IconManager.GetIcon(field.IconName);
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
    }

    public StratagemControl(Stratagem stratagem)
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
        };
        _stackPanel.Children.Add(_image);

        _label = new Label
        {
            IsVisible = false,
        };
        _stackPanel.Children.Add(_label);

        Stratagem = stratagem;
        stratagem.Control = this;
    }

    [Content]
    public Controls Children => _stackPanel.Children;
}
