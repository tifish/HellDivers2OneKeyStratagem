using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace Helldivers2OneKeyStratagem;

public partial class MainWindow : Window
{
    public MainWindow()
    {
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

    public void AddWindowHeightAndCenterWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime)
            return;

        if (Screens.Primary == null)
            return;

        // Add the window height to show more content
        var desktopRect = Screens.Primary.WorkingArea.ToRect(DesktopScaling);
        var windowHeight = FrameSize!.Value.Height;

        var moreHeight = StratagemsStackPanel.Bounds.Height - StratagemsScrollViewer.Bounds.Height;
        var maxHeight = desktopRect.Height - (windowHeight - Height);
        Height = Math.Min(maxHeight, Height + moreHeight);

        // Center the window after the height is set.
        Dispatcher.UIThread.Post(() =>
        {
            var pixelRect = new PixelRect(Position, PixelSize.FromSize(FrameSize!.Value, DesktopScaling));
            Position = Screens.Primary.WorkingArea.CenterRect(pixelRect).Position;
        });
    }

    private MainViewModel Model => (MainViewModel)DataContext!;
}
