﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Jeek.Avalonia.Localization;

namespace HellDivers2OneKeyStratagem;

public class HotkeyStratagemPanel : TemplatedControl
{
    private Border? _border;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _border = e.NameScope.Get<Border>("Border");
        UpdateIsBorderVisible();
    }

    public string HotkeyName
    {
        get => GetValue(HotkeyNameProperty);
        set => SetValue(HotkeyNameProperty, value);
    }

    public static readonly StyledProperty<string> HotkeyNameProperty =
        AvaloniaProperty.Register<HotkeyStratagemPanel, string>(nameof(HotkeyName));

    public string StratagemName
    {
        get => GetValue(StratagemNameProperty);
        set => SetValue(StratagemNameProperty, value);
    }

    public static readonly StyledProperty<string> StratagemNameProperty =
        AvaloniaProperty.Register<HotkeyStratagemPanel, string>(nameof(StratagemName));

    public bool HasStratagem => StratagemName != Localizer.Get("None");

    public void ClearStratagem()
    {
        Bind(StratagemNameProperty, new LocalizeExtension("None"));
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
