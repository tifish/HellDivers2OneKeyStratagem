using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace HellDivers2OneKeyStratagem;

public class HotkeyStratagemPanel : TemplatedControl
{
    private Border? _border;
    private StratagemControl? _stratagemControl;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _border = e.NameScope.Get<Border>("Border");
        UpdateIsBorderVisible();

        _stratagemControl = e.NameScope.Get<StratagemControl>("StratagemControl");
    }

    public string HotkeyName
    {
        get => GetValue(HotkeyNameProperty);
        set => SetValue(HotkeyNameProperty, value);
    }

    public static readonly StyledProperty<string> HotkeyNameProperty =
        AvaloniaProperty.Register<HotkeyStratagemPanel, string>(nameof(HotkeyName));

    public Stratagem? Stratagem
    {
        get => _stratagemControl?.Stratagem;
        set => _stratagemControl!.Stratagem = value;
    }

    public bool IsBorderVisible
    {
        get => GetValue(IsBorderVisibleProperty);
        set
        {
            if (IsBorderVisible == value)
                return;

            SetValue(IsBorderVisibleProperty, value);
            UpdateIsBorderVisible();
        }
    }

    public static readonly StyledProperty<bool> IsBorderVisibleProperty =
        AvaloniaProperty.Register<HotkeyStratagemPanel, bool>(nameof(IsBorderVisible));

    private void UpdateIsBorderVisible()
    {
        if (_border != null)
            _border.BorderBrush = IsBorderVisible ? Brushes.Gray : Brushes.Transparent;
    }
}
