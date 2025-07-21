using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HellDivers2OneKeyStratagem;

public partial class MessageDialog : Window
{
    public MessageDialog()
    {
    }

    public MessageDialog(string title, string message)
    {
        InitializeComponent();

        Title = title;
        MessageLabel.Content = message;
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
