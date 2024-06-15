namespace HellDivers2OneKeyStratagem;

public partial class EditAliasesForm : Form
{
    private string _stratagemName;

    public EditAliasesForm(string stratagemName)
    {
        InitializeComponent();

        _stratagemName = stratagemName;
        systemAliasesTextBox.Text = StratagemManager.GetSystemAlias(stratagemName);
        userAliasesTextBox.Text = StratagemManager.GetUserAlias(stratagemName);
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
