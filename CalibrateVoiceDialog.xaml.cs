namespace HellDivers2OneKeyStratagem;

/// <summary>
///     Interaction logic for CalibrateVoiceDialogPage.xaml
/// </summary>
public partial class CalibrateVoiceDialog
{
    public CalibrateVoiceDialog()
    {
        InitializeComponent();
    }

    public void SetMessage(string msg)
    {
        MessageLabel.Content = msg;
    }
}
