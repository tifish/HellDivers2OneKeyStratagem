using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace HellDivers2OneKeyStratagem;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        ClientSizeProperty.Changed.Subscribe(ClientSizeChanged);

        MainViewModel.Instance.SetMainWindow(this);
        DataContext = MainViewModel.Instance;

        Model.IsLoading = true;

        try
        {
            InitializeComponent();
        }
        finally
        {
            Model.IsLoading = false;
        }
    }

    private bool _hasCenteredWindow = false;

    private void ClientSizeChanged(AvaloniaPropertyChangedEventArgs<Size> args)
    {
        if (_hasCenteredWindow)
            return;

        _hasCenteredWindow = true;

        // Delay execution to get the correct StratagemsStackPanel size
        Dispatcher.UIThread.Post(AddWindowHeightAndCenterWindow);
    }

    private void AddWindowHeightAndCenterWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime)
            return;

        if (Screens.Primary == null)
            return;

        // Add the window height to show more content
        var desktopRect = Screens.Primary.WorkingArea.ToRect(DesktopScaling);

        if (StratagemsStackPanel.Bounds.Height > StratagemsScrollViewer.Bounds.Height)
        {
            var diff1 = StratagemsStackPanel.Bounds.Height - StratagemsScrollViewer.Bounds.Height;
            var diff2 = desktopRect.Height - Bounds.Height;
            StratagemsScrollViewer.Height += Math.Min(diff1, diff2);
        }

        // Center the window
        var pixelRect = new PixelRect(Position, PixelSize.FromSize(Bounds.Size, DesktopScaling));
        Position = Screens.Primary.WorkingArea.CenterRect(pixelRect).Position;
    }


    private MainViewModel Model => (MainViewModel)DataContext!;
}
