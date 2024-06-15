namespace HellDivers2OneKeyStratagem;

public partial class EditAliasesDialog : Form
{
    private string _stratagemName;

    public EditAliasesDialog(string stratagemName)
    {
        InitializeComponent();

        _stratagemName = stratagemName;
        systemAliasesTextBox.Text = StratagemManager.GetSystemAlias(stratagemName);
        userAliasesTextBox.Text = StratagemManager.GetUserAlias(stratagemName);

        ActiveControl = userAliasesTextBox;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        StratagemManager.SetUserAlias(_stratagemName, userAliasesTextBox.Text.Trim());
        DialogResult = DialogResult.OK;
        Close();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
