using Avalonia;
using Avalonia.Controls;

namespace HellDivers2OneKeyStratagem;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        ClientSizeProperty.Changed.Subscribe(
            x =>
            {
                if (!Model.HasResized)
                    return;

                var screen = Screens.Primary;
                if (screen == null)
                    return;

                var rect = new PixelRect(
                    Position,
                    PixelSize.FromSize(x.NewValue.Value, DesktopScaling));
                Position = screen.WorkingArea.CenterRect(rect).Position;

                Model.HasResized = false;
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

    private MainViewModel Model => (MainViewModel)DataContext!;
}
