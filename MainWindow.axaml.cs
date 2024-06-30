using Avalonia.Controls;

namespace HellDivers2OneKeyStratagem;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        MainViewModel.Instance.SetMainWindow(this);
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
}
