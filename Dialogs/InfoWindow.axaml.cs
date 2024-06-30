using Avalonia.Controls;
using Avalonia.Input;

namespace HellDivers2OneKeyStratagem;

public partial class InfoWindow : Window
{
    private readonly MainWindow _mainWindow;

    public InfoWindow()
    {
        _mainWindow = null!;
    }

    public InfoWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;

        DataContext = MainViewModel.Instance;

        M.IsLoading = true;

        try
        {
            InitializeComponent();
        }
        finally
        {
            M.IsLoading = false;
        }
    }

    private MainViewModel M => (MainViewModel)DataContext!;

    private void InfoWindow_OnClosed(object? sender, EventArgs e)
    {
        MainViewModel.Instance.ShowSpeechInfoWindow = false;
    }

    private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}
