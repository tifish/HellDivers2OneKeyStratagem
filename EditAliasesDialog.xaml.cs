using System.Windows.Threading;

namespace HellDivers2OneKeyStratagem;

public partial class EditAliasesDialog
{
    private readonly string _stratagemName;

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
    }

    public void Commit()
    {
        StratagemManager.SetUserAlias(_stratagemName, UserAliasesTextBox.Text.Trim());
    }
}
