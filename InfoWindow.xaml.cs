namespace HellDivers2OneKeyStratagem;

public partial class InfoWindow()
{
    private readonly MainWindow _mainWindow = null!;

    public InfoWindow(MainWindow mainWindow): this()
    {
        _mainWindow = mainWindow;

        InitializeComponent();
    }

    public void SetInfo(string info)
    {
        InfoLabel.Content = info;
    }

    private void InfoWindow_OnClosed(object? sender, EventArgs e)
    {
        _mainWindow.NotifyInfoWindowClosed();
    }
}
