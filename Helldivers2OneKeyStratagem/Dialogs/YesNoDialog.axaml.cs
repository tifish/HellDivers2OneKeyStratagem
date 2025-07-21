using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Helldivers2OneKeyStratagem;

public partial class YesNoDialog : Window
{
    public YesNoDialog()
    {
    }

    public YesNoDialog(string title, string message)
    {
        InitializeComponent();

        Title = title;
        MessageLabel.Content = message;
    }

    private void YesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void NoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
