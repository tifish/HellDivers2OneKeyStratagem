using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;
using Jeek.Avalonia.Localization;

namespace HellDivers2OneKeyStratagem;

public class StratagemControl : UserControl
{
    private readonly Image _image;

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
                _image.Source = IconManager.GetNoneIcon();
            }
            else
            {
                _image.Source = IconManager.GetIcon(field.Id) ?? IconManager.GetNoneIcon();
            }

            UpdateToolTip();
        }
    }

    public StratagemControl()
    {
        Content = _image = new Image
        {
            Width = 64,
            Height = 64,
            Stretch = Avalonia.Media.Stretch.Uniform,
            Source = IconManager.GetNoneIcon(),
        };
    }

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
