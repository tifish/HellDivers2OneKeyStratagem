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

    public bool HasContentSizeChanged { get; set; }

    private void ClientSizeChanged(AvaloniaPropertyChangedEventArgs<Size> args)
    {
        if (!HasContentSizeChanged)
            return;
        HasContentSizeChanged = false;

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
        var windowHeight = FrameSize!.Value.Height;

        // If not all stratagems are visible, add the height of the missing stratagems.
        // But not more than the height of the desktop.
        if (StratagemsStackPanel.Bounds.Height > StratagemsScrollViewer.Bounds.Height)
        {
            var diff1 = StratagemsStackPanel.Bounds.Height - StratagemsScrollViewer.Bounds.Height;
            // If the window is too tall, diff2 can be negative, which will reduce the height to the desktop.
            var diff2 = desktopRect.Height - windowHeight;
            StratagemsScrollViewer.Height += Math.Min(diff1, diff2);
        }

        // Center the window after the height is set.
        Dispatcher.UIThread.Post(() =>
        {
            var pixelRect = new PixelRect(Position, PixelSize.FromSize(FrameSize!.Value, DesktopScaling));
            Position = Screens.Primary.WorkingArea.CenterRect(pixelRect).Position;
        });
    }

    private MainViewModel Model => (MainViewModel)DataContext!;
}
