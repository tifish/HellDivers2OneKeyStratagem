using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace HellDivers2OneKeyStratagem;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        ClientSizeProperty.Changed.Subscribe(
            x =>
            {
                if (!Model.HasContentSizeChanged)
                    return;

                var screen = Screens.Primary;
                if (screen == null)
                    return;

                var rect = new PixelRect(
                    Position,
                    PixelSize.FromSize(x.NewValue.Value, DesktopScaling));
                Position = screen.WorkingArea.CenterRect(rect).Position;

                Model.HasContentSizeChanged = false;
            });

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

    private void StretchWindowHeight()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        if (desktop.MainWindow == null)
            return;
        var desktopHeight = desktop.MainWindow.Bounds.Height;

        if (StratagemsScrollViewer.Bounds.Height < StratagemsStackPanel.Bounds.Height)
        {
            var diff1 = StratagemsStackPanel.Bounds.Height - StratagemsScrollViewer.Bounds.Height;
            var diff2 = desktopHeight - Bounds.Height;
            StratagemsScrollViewer.Height += Math.Min(diff1, diff2);
        }
    }


    private MainViewModel Model => (MainViewModel)DataContext!;
}
