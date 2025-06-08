using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace HellDivers2OneKeyStratagem;

public partial class EditAliasesDialog : Window
{
    private readonly string _stratagemName;

    public EditAliasesDialog()
    {
        _stratagemName = "";
    }

    public EditAliasesDialog(string stratagemName)
    {
        InitializeComponent();

        _stratagemName = stratagemName;
        SystemAliasesTextBox.Text = StratagemManager.GetSystemAlias(stratagemName);
        UserAliasesTextBox.Text = StratagemManager.GetUserAlias(stratagemName);

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            UserAliasesTextBox.Focus();
        };
        timer.Start();

        UserAliasesTextBox.KeyDown += UserAliasesTextBox_OnKeyDown;
    }

    private void UserAliasesTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Close(true);
        }
    }

    public void Commit()
    {
        StratagemManager.SetUserAlias(_stratagemName, UserAliasesTextBox.Text?.Trim() ?? "");

        if (StratagemManager.TryGet(_stratagemName, out var stratagem))
        {
            stratagem.Control?.UpdateToolTip();
        }
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
