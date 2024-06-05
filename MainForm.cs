using System.Diagnostics;

namespace HellDivers2OneKeyStratagem;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        SuspendLayout();
        rootFlowLayoutPanel.SuspendLayout();

        try
        {
            await LoadStratagems();
            InitFKeys();
            InitStratagemGroups();
            await LoadSettings();
            await LoadTemplate();
        }
        finally
        {
            rootFlowLayoutPanel.ResumeLayout();
            ResumeLayout();
        }

        CenterToScreen();
    }

    private static readonly string ExeDirectory = Path.GetDirectoryName(Application.ExecutablePath)!;
    private static readonly string AutoHotkeyDirectory = Path.Combine(ExeDirectory, "AutoHotkey");

    private static readonly string TemplateAhk = Path.Combine(AutoHotkeyDirectory, "HellDivers2OneKeyStratagem.template.ahk");
    private string[] _templateLines = [];

    private async Task LoadTemplate()
    {
        _templateLines = await File.ReadAllLinesAsync(TemplateAhk);
    }

    private class Stratagem
    {
        public string Name = "";
        public string Keys = "";
        public CheckBox CheckBox = null!;
    }

    private readonly Dictionary<string, List<Stratagem>> _stratagemGroups = [];
    private readonly Dictionary<string, Stratagem> _stratagems = [];
    private const string StratagemsFile = "Stratagems.tab";

    private async Task LoadStratagems()
    {
        if (!File.Exists(StratagemsFile))
            return;

        _stratagemGroups.Clear();
        _stratagems.Clear();
        List<Stratagem>? currentGroup = null;

        await foreach (var line in File.ReadLinesAsync(StratagemsFile))
            if (line.Contains('\t'))
            {
                var items = line.Split('\t');
                if (items.Length != 2)
                    throw new InvalidOperationException($"Invalid line: {line}");
                if (currentGroup == null)
                    throw new InvalidOperationException($"No group found for stratagem {items[0]}");
                var stratagem = new Stratagem { Name = items[0], Keys = items[1] };
                currentGroup.Add(stratagem);
                _stratagems.Add(stratagem.Name, stratagem);
            }
            else
            {
                currentGroup = [];
                _stratagemGroups.Add(line, currentGroup);
            }
    }

    private static readonly string SettingsFile = Path.Combine(ExeDirectory, "Settings.json");
    private readonly JsonFile<Settings> _settingsFile = new(SettingsFile);
    private Settings? _settings;

    private async Task LoadSettings()
    {
        _settings = await _settingsFile.Load();
        if (_settings == null)
            return;

        switch (_settings.TriggerKey)
        {
            case "Ctrl":
                ctrlRadioButton.Checked = true;
                break;
            case "Alt":
                altRadioButton.Checked = true;
                break;
        }

        switch (_settings.OperateKeys)
        {
            case "WASD":
                wasdRadioButton.Checked = true;
                break;
            case "Arrow":
                arrowRadioButton.Checked = true;
                break;
        }

        if (_settings.StratagemSets.Count > 0)
        {
            SetFKeyStratagemString(_settings.StratagemSets[0]);

            stratagemSetsComboBox.Items.Clear();
            foreach (var stratagemSet in _settings.StratagemSets.Skip(1))
                stratagemSetsComboBox.Items.Add(stratagemSet);
        }
    }

    private string GetFKeyStratagemString()
    {
        return string.Join(';', _fKeyStratagems.Select(s => s?.Name ?? ""));
    }

    private void SetFKeyStratagemString(string value)
    {
        var names = value.Split(';');
        var count = Math.Min(names.Length, 12);
        for (var i = 0; i < count; i++)
        {
            var name = names[i];
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (!_stratagems.TryGetValue(name, out var stratagem))
                continue;

            SetFKeyStratagem(i, stratagem);
        }
    }

    private async Task SaveSettings()
    {
        _settings ??= new Settings();

        _settings.TriggerKey = ctrlRadioButton.Checked ? "Ctrl" : "Alt";
        _settings.OperateKeys = wasdRadioButton.Checked ? "WASD" : "Arrow";

        _settings.StratagemSets.Clear();
        _settings.StratagemSets.Add(GetFKeyStratagemString());
        foreach (var item in stratagemSetsComboBox.Items)
            _settings.StratagemSets.Add((string)item);

        await _settingsFile.Save(_settings);
    }

    private readonly Label[] _fKeyLabels = new Label[12];
    private readonly Stratagem?[] _fKeyStratagems = new Stratagem?[12];
    private readonly FlowLayoutPanel[] _fKeyRoots = new FlowLayoutPanel[12];

    private int _selectedFKeyIndex = 11;

    private int SelectedFKeyIndex
    {
        get => _selectedFKeyIndex;
        set
        {
            if (value == _selectedFKeyIndex)
                return;

            rootFlowLayoutPanel.SuspendLayout();

            try
            {
                if (_selectedFKeyIndex >= 0)
                    _fKeyRoots[SelectedFKeyIndex].BorderStyle = BorderStyle.None;
                _fKeyRoots[value].BorderStyle = BorderStyle.FixedSingle;
                _selectedFKeyIndex = value;
            }
            finally
            {
                rootFlowLayoutPanel.ResumeLayout();
            }
        }
    }

    private const string NoStratagem = "无";

    private void InitFKeys()
    {
        fKeysFlowLayoutPanel.Controls.Clear();

        for (var i = 1; i <= 12; i++)
        {
            var root = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var fKeyLabel = new Label { Text = $@"F{i}", AutoSize = true, Anchor = AnchorStyles.Top };
            var stratagemLabel = new Label { Text = NoStratagem, AutoSize = true, Anchor = AnchorStyles.Top };
            root.Controls.Add(fKeyLabel);
            root.Controls.Add(stratagemLabel);
            fKeysFlowLayoutPanel.Controls.Add(root);

            var i1 = i;

            fKeyLabel.MouseDown += OnFKeyMouseDown;
            stratagemLabel.MouseDown += OnFKeyMouseDown;
            root.MouseDown += OnFKeyMouseDown;

            _fKeyLabels[i - 1] = stratagemLabel;
            _fKeyRoots[i - 1] = root;
            continue;

            void OnFKeyMouseDown(object? o, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                    SelectedFKeyIndex = i1 - 1;
            }
        }

        SelectedFKeyIndex = _fKeyRoots.Length - 1;
        _fKeyRoots.Last().BorderStyle = BorderStyle.FixedSingle;
    }

    private void SetFKeyStratagem(int index, Stratagem stratagem)
    {
        var currentStratagem = _fKeyStratagems[index];
        if (currentStratagem != null)
            currentStratagem.CheckBox.Checked = false;

        _fKeyStratagems[index] = stratagem;
        stratagem.CheckBox.Checked = true;
        _settingsChanged = true;

        _fKeyLabels[index].Text = stratagem.Name;
    }


    private void InitStratagemGroups()
    {
        stratagemGroupsFlowLayoutPanel.Controls.Clear();

        foreach (var group in _stratagemGroups)
        {
            var root = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var groupLabel = new Label { Text = group.Key, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font(Font, FontStyle.Bold) };
            root.Controls.Add(groupLabel);

            foreach (var stratagem in group.Value)
            {
                var stratagemCheckBox = new CheckBox { Text = stratagem.Name, AutoSize = true, Anchor = AnchorStyles.Left };
                stratagem.CheckBox = stratagemCheckBox;
                root.Controls.Add(stratagemCheckBox);

                stratagemCheckBox.Click += (_, _) =>
                {
                    if (stratagemCheckBox.Checked)
                    {
                        SetFKeyStratagem(SelectedFKeyIndex, stratagem);

                        if (SelectedFKeyIndex > 0)
                            if (_fKeyLabels[SelectedFKeyIndex - 1].Text == NoStratagem)
                                SelectedFKeyIndex--;
                    }
                    else
                    {
                        var fKeyIndex = Array.IndexOf(_fKeyStratagems, stratagem);
                        if (fKeyIndex > -1)
                        {
                            _fKeyStratagems[fKeyIndex] = null;
                            _settingsChanged = true;
                            _fKeyLabels[fKeyIndex].Text = NoStratagem;
                            SelectedFKeyIndex = fKeyIndex;
                        }
                    }
                };
            }

            stratagemGroupsFlowLayoutPanel.Controls.Add(root);
        }
    }

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F1:
                SelectedFKeyIndex = 0;
                break;
            case Keys.F2:
                SelectedFKeyIndex = 1;
                break;
            case Keys.F3:
                SelectedFKeyIndex = 2;
                break;
            case Keys.F4:
                SelectedFKeyIndex = 3;
                break;
            case Keys.F5:
                SelectedFKeyIndex = 4;
                break;
            case Keys.F6:
                SelectedFKeyIndex = 5;
                break;
            case Keys.F7:
                SelectedFKeyIndex = 6;
                break;
            case Keys.F8:
                SelectedFKeyIndex = 7;
                break;
            case Keys.F9:
                SelectedFKeyIndex = 8;
                break;
            case Keys.F10:
                SelectedFKeyIndex = 9;
                break;
            case Keys.F11:
                SelectedFKeyIndex = 10;
                break;
            case Keys.F12:
                SelectedFKeyIndex = 11;
                break;
        }
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        KillAhkProcess();
    }

    private void ctrlRadioButton_Click(object sender, EventArgs e)
    {
        _settingsChanged = true;
    }

    private void wasdRadioButton_Click(object sender, EventArgs e)
    {
        _settingsChanged = true;
    }

    private Process? _ahkProcess;

    private static readonly string Ahk2Exe = Path.Combine(AutoHotkeyDirectory, "Ahk2Exe.exe");
    private static readonly string ScriptFile = Path.Combine(AutoHotkeyDirectory, "HellDivers2OneKeyStratagem.ahk");

    private bool _settingsChanged;

    private async void startButton_Click(object sender, EventArgs e)
    {
        if (_settingsChanged)
        {
            await SaveSettings();
            _settingsChanged = false;
        }

        KillAhkProcess();

        await GenerateScript();
        await GenerateKeys();

        using var compileProcess = Process.Start(new ProcessStartInfo
        {
            FileName = Ahk2Exe,
            WorkingDirectory = AutoHotkeyDirectory,
            Arguments = $"""/in "{ScriptFile}" /base AutoHotkey64.exe /silent""",
            UseShellExecute = false,
        });

        if (compileProcess != null)
            await compileProcess.WaitForExitAsync();
        if (compileProcess is not { ExitCode: 0 })
        {
            MessageBox.Show(@"Failed to compile script");
            return;
        }

        _ahkProcess = Process.Start(new ProcessStartInfo
        {
            FileName = Path.ChangeExtension(ScriptFile, ".exe"),
            WorkingDirectory = AutoHotkeyDirectory,
            UseShellExecute = true,
        });

        if (_ahkProcess != null)
        {
            runningLabel.Text = @"为了民主！";
            startButton.Text = @"重启(&W)";
            stopButton.Enabled = true;
        }
    }

    private async Task GenerateScript()
    {
        var lines = new List<string>(_templateLines) { "" };

        for (var i = 0; i < _fKeyLabels.Length; i++)
        {
            var stratagem = _fKeyStratagems[i];
            if (stratagem == null)
                continue;

            lines.Add($"""F{i + 1}:: CallStratagem("{stratagem.Keys}")""");
        }

        await File.WriteAllLinesAsync(ScriptFile, lines);
    }

    private static readonly string KeysFile = Path.Combine(AutoHotkeyDirectory, "Keys.ahk");

    private async Task GenerateKeys()
    {
        await using var writer = new StreamWriter(KeysFile);
        await writer.WriteLineAsync("#Requires AutoHotkey v2.0");
        await writer.WriteLineAsync("");
        await writer.WriteLineAsync($"TriggerKey := \"{(ctrlRadioButton.Checked ? "Ctrl" : "Alt")}\"");
        if (wasdRadioButton.Checked)
            await writer.WriteLineAsync("""
                                        UpKey := "w"
                                        DownKey := "s"
                                        LeftKey := "a"
                                        RightKey := "d"
                                        """);
        else
            await writer.WriteLineAsync("""
                                        UpKey := "up"
                                        DownKey := "down"
                                        LeftKey := "left"
                                        RightKey := "right"
                                        """);
    }

    private void KillAhkProcess()
    {
        if (_ahkProcess == null)
            return;

        _ahkProcess.Kill();
        _ahkProcess = null;

        runningLabel.Text = @"已阵亡...";
        startButton.Text = @"启动(&W)";
        stopButton.Enabled = false;
    }

    private void stopButton_Click(object sender, EventArgs e)
    {
        KillAhkProcess();
    }

    private async void saveStratagemSetButton_Click(object sender, EventArgs e)
    {
        var fKeyStratagemString = GetFKeyStratagemString();
        if (stratagemSetsComboBox.Items.Contains(fKeyStratagemString))
            return;

        stratagemSetsComboBox.Items.Add(fKeyStratagemString);
        stratagemSetsComboBox.SelectedIndex = stratagemSetsComboBox.Items.Count - 1;
        await SaveSettings();
    }

    private async void deleteStratagemSetButton_Click(object sender, EventArgs e)
    {
        if (stratagemSetsComboBox.SelectedIndex == -1)
            return;

        stratagemSetsComboBox.Items.RemoveAt(stratagemSetsComboBox.SelectedIndex);
        await SaveSettings();
    }

    private void stratagemSetsComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedItem = (string?)stratagemSetsComboBox.SelectedItem;
        if (selectedItem == null)
            return;

        SetFKeyStratagemString(selectedItem);
        _settingsChanged = true;
    }
}
