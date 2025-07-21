using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Helldivers2OneKeyStratagem;

/// <summary>
///     Interaction logic for CalibrateVoiceDialogPage.xaml
/// </summary>
public partial class CalibrateVoiceDialog : Window
{
    public CalibrateVoiceDialog()
    {
        InitializeComponent();
    }

    public void SetMessage(string msg)
    {
        MessageLabel.Content = msg;
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
