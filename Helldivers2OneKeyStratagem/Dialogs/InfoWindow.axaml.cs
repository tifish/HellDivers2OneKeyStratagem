using Avalonia.Controls;
using Avalonia.Input;

namespace Helldivers2OneKeyStratagem;

public partial class InfoWindow : Window
{
    private bool _isClickThrough;
    public bool IsClickThrough
    {
        get => _isClickThrough;
        set
        {
            if (_isClickThrough != value)
            {
                _isClickThrough = value;
                UpdateClickThrough(value);
            }
        }
    }

    public InfoWindow()
    {
        DataContext = MainViewModel.Instance;
        InitializeComponent();
    }

    private void UpdateClickThrough(bool isClickThrough)
    {
        if (!OperatingSystem.IsWindows())
            return;

        var platformHandle = TryGetPlatformHandle();
        if (platformHandle == null)
            return;

        var hwnd = platformHandle.Handle;
        var extendedStyle = Win32.GetWindowLong(hwnd, Win32.GWL_EXSTYLE);

        if (isClickThrough)
        {
            extendedStyle |= Win32.WS_EX_LAYERED | Win32.WS_EX_TRANSPARENT;
        }
        else
        {
            extendedStyle &= ~(Win32.WS_EX_LAYERED | Win32.WS_EX_TRANSPARENT);
        }

        Win32.SetWindowLong(hwnd, Win32.GWL_EXSTYLE, extendedStyle);
    }

    private MainViewModel M => (MainViewModel)DataContext!;

    private void InfoWindow_OnClosed(object? sender, EventArgs e)
    {
        MainViewModel.Instance.ShowSpeechInfoWindow = false;
    }

    private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsClickThrough)
        {
            BeginMoveDrag(e);
        }
    }
}
