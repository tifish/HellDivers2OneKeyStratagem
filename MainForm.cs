using System.Diagnostics;
using System.Globalization;
using AutoHotkey.Interop;
using EdgeTTS;
using NAudio.Wave;

namespace HellDivers2OneKeyStratagem;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private bool _isLoading = true;

    private async void MainForm_Load(object sender, EventArgs e)
    {
        SuspendLayout();
        rootFlowLayoutPanel.SuspendLayout();

        try
        {
            await LoadSettings();

            InitLanguages();
            InitFKeys();

            await LoadByLanguage();

            LoadVoiceNames();
            await LoadScriptTemplate();

            LoadMicDevices();
        }
        finally
        {
            rootFlowLayoutPanel.ResumeLayout();
            ResumeLayout();
        }

        CenterToScreen();

        _isLoading = false;
    }

    private void LoadMicDevices()
    {
        for (var i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var device = WaveInEvent.GetCapabilities(i);
            micComboBox.Items.Add(device.ProductName);
        }
    }

    private void InitLanguages()
    {
        var speechLanguages = VoiceCommand.GetInstalledRecognizers();
        if (speechLanguages.Count == 0)
            return;

        languageComboBox.Items.Clear();
        foreach (var language in speechLanguages)
            languageComboBox.Items.Add(language);

        if (!speechLanguages.Contains(Settings.Language))
        {
            if (speechLanguages.Contains(CultureInfo.CurrentCulture.Name))
                Settings.Language = CultureInfo.CurrentCulture.Name;
            else
                Settings.Language = speechLanguages.First();
            _settingsChanged = true;
        }

        languageComboBox.SelectedItem = Settings.Language;
    }

    private async Task LoadByLanguage()
    {
        StratagemManager.Load();
        InitStratagemGroups();
        InitSettingsToUI();
        ResetVoiceCommand();
        await LoadGeneratingVoiceStyles();
    }

    private void ResetVoiceCommand()
    {
        _voiceCommand?.Stop();
        _voiceCommand?.Dispose();
        _voiceCommand = null;

        if (Settings.EnableVoiceTrigger)
            StartVoiceTrigger();
    }

    private bool _isActive;

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _isActive = true;
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        _isActive = false;
    }

    private VoiceCommand? _voiceCommand;

    private void StartVoiceTrigger()
    {
        if (_voiceCommand == null)
        {
            var languages = VoiceCommand.GetInstalledRecognizers();
            if (languages.Count == 0)
            {
                MessageBox.Show(@"No voice recognition engine installed");
                return;
            }

            if (!languages.Contains(Settings.Language))
            {
                MessageBox.Show($@"Voice recognition engine for {Settings.Language} not installed");
                return;
            }

            try
            {
                _voiceCommand = new VoiceCommand(Settings.Language, Settings.WakeupWord, [.. Stratagems.Keys]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"创建语音识别失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _voiceCommand.CommandRecognized += (_, command) =>
            {
                voiceRecognizeResultLabel.Text = $@"识别概率：{command.Score:F3} 文字：{command.Text}";

                if (command.Score < Settings.VoiceConfidence)
                    return;

                if (!Stratagems.TryGetValue(command.Text, out var stratagem))
                    return;

                if (_isActive)
                {
                    if (!Settings.EnableSetFKeyByVoice)
                        return;

                    SetFKeyStratagem(SelectedFKeyIndex, stratagem);
                }
                else if (WindowHelper.GetActiveWindowTitle() == "HELLDIVERS™ 2")
                {
                    if (Settings.PlayVoice)
                        PlayStratagemVoice(stratagem.Name);

                    stratagem.PressKeys();
                }
            };
        }

        _voiceCommand.UseMic(0);
        _voiceCommand.Start();
    }

    private void PlayStratagemVoice(string stratagemName)
    {
        PlayVoice(Path.Combine(VoiceRootPath, Settings.VoiceName, stratagemName + ".mp3"));
    }

    private void StopVoiceTrigger()
    {
        if (_voiceCommand == null)
            return;

        _voiceCommand.Stop();
    }

    private void LoadVoiceNames()
    {
        var styles = Directory.GetDirectories(VoiceRootPath)
            .Select(Path.GetFileName)
            .Where(style => style != null)
            .ToList();

        voiceNamesComboBox.Items.Clear();
        foreach (var style in styles)
            voiceNamesComboBox.Items.Add(style!);

        voiceNamesComboBox.SelectedItem =
            styles.Contains(Settings.VoiceName)
                ? Settings.VoiceName
                : styles.Contains("晓妮")
                    ? "晓妮"
                    : styles.FirstOrDefault();
    }

    private List<Voice> _voices = null!;

    private async Task LoadGeneratingVoiceStyles()
    {
        if (Settings.Language == "")
            return;

        var manager = await VoicesManager.Create();
        _voices = manager.Find(language: Settings.Language[..2]);

        foreach (var voice in _voices)
            generateVoiceStyleComboBox.Items.Add(voice.ShortName);

        generateVoiceStyleComboBox.SelectedIndex = generateVoiceStyleComboBox.Items.Count - 1;
    }

    private static readonly string ExeDirectory = Path.GetDirectoryName(Application.ExecutablePath)!;

    private static readonly string TemplateAhk1 = Path.Combine(ExeDirectory, "HellDivers2OneKey.template1.ahk");
    private string[] _template1Lines = [];
    private static readonly string TemplateAhk2 = Path.Combine(ExeDirectory, "HellDivers2OneKey.template2.ahk");
    private string[] _template2Lines = [];

    private async Task LoadScriptTemplate()
    {
        _template1Lines = await File.ReadAllLinesAsync(TemplateAhk1);
        _template2Lines = await File.ReadAllLinesAsync(TemplateAhk2);
    }

    private static readonly string SettingsFile = Path.Combine(ExeDirectory, "Settings.json");
    private readonly JsonFile<AppSettings> _settingsFile = new(SettingsFile);

    private async Task LoadSettings()
    {
        var settings = await _settingsFile.Load();
        if (settings != null)
            Settings = settings;
    }

    private void InitSettingsToUI()
    {
        switch (Settings.TriggerKey)
        {
            case "Ctrl":
                ctrlRadioButton.Checked = true;
                break;
            case "Alt":
                altRadioButton.Checked = true;
                break;
        }

        switch (Settings.OperateKeys)
        {
            case "WASD":
                wasdRadioButton.Checked = true;
                break;
            case "Arrow":
                arrowRadioButton.Checked = true;
                break;
        }

        playVoiceCheckBox.Checked = Settings.PlayVoice;

        if (Settings.StratagemSets.Count > 0)
        {
            SetFKeyStratagemString(Settings.StratagemSets[0]);

            stratagemSetsComboBox.Items.Clear();
            foreach (var stratagemSet in Settings.StratagemSets.Skip(1))
                stratagemSetsComboBox.Items.Add(stratagemSet);
        }

        enableSetFKeyByVoiceCheckBox.Checked = Settings.EnableSetFKeyByVoice;

        voiceConfidenceNumericUpDown.Value = (decimal)Settings.VoiceConfidence;
        wakeupWordTextBox.Text = Settings.WakeupWord;

        enableVoiceTriggerCheckBox.Checked = Settings.EnableVoiceTrigger;

        enableHotkeyTriggerCheckBox.Checked = Settings.EnableHotkeyTrigger;
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

            if (!Stratagems.TryGetValue(name, out var stratagem))
                continue;

            SetFKeyStratagem(i, stratagem, false);
        }
    }

    private async Task SaveSettings()
    {
        Settings.StratagemSets.Clear();
        Settings.StratagemSets.Add(GetFKeyStratagemString());
        foreach (var item in stratagemSetsComboBox.Items)
            Settings.StratagemSets.Add((string)item);

        await _settingsFile.Save(Settings);
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
                if (e.Button != MouseButtons.Left)
                    return;
                SelectedFKeyIndex = i1 - 1;

                if (!Settings.PlayVoice)
                    return;

                if (Settings.EnableSetFKeyByVoice)
                    return;

                var stratagem = _fKeyStratagems[SelectedFKeyIndex];
                if (stratagem != null)
                    PlayStratagemVoice(stratagem.Name);
            }
        }

        SelectedFKeyIndex = _fKeyRoots.Length - 1;
        _fKeyRoots.Last().BorderStyle = BorderStyle.FixedSingle;
    }

    private void SetFKeyStratagem(int index, Stratagem stratagem, bool playVoice = true)
    {
        var currentStratagem = _fKeyStratagems[index];
        if (currentStratagem != null)
            currentStratagem.CheckBox.Checked = false;

        _fKeyStratagems[index] = stratagem;
        stratagem.CheckBox.Checked = true;
        _fKeyLabels[index].Text = stratagem.Name;

        if (!_isLoading)
            _settingsChanged = true;

        if (Settings.PlayVoice && playVoice)
            PlayStratagemVoice(stratagem.Name);
    }

    private void InitStratagemGroups()
    {
        stratagemGroupsFlowLayoutPanel.Controls.Clear();

        foreach (var group in StratagemGroups)
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

    private void ctrlRadioButton_Click(object sender, EventArgs e)
    {
        Settings.TriggerKey = ctrlRadioButton.Checked ? "Ctrl" : "Alt";
        if (!_isLoading)
            _settingsChanged = true;
    }

    private void wasdRadioButton_Click(object sender, EventArgs e)
    {
        Settings.OperateKeys = wasdRadioButton.Checked ? "WASD" : "Arrow";
        if (!_isLoading)
            _settingsChanged = true;
    }

    private AutoHotkeyEngine? _autoHotkeyEngine;

    private void StartAutoHotkeyScript()
    {
        if (_isClosing)
            return;

        var lines = new List<string>(_template1Lines);
        GenerateKeys(lines);
        lines.AddRange(_template2Lines);
        GenerateScript(lines);

        _autoHotkeyEngine?.Terminate();
        _autoHotkeyEngine = new AutoHotkeyEngine();
        _autoHotkeyEngine.LoadScript(string.Join('\n', lines));
    }

    private void GenerateScript(List<string> lines)
    {
        var voiceName = voiceNamesComboBox.SelectedItem as string ?? "";

        for (var i = 0; i < _fKeyLabels.Length; i++)
        {
            var stratagem = _fKeyStratagems[i];
            if (stratagem == null)
                continue;

            lines.Add($$"""
                        F{{i + 1}}::
                            CallStratagem("{{stratagem.KeySequence}}")
                        """);
            if (Settings.PlayVoice)
                lines.Add($"""
                               SoundPlay, Voice\{voiceName}\{stratagem.Name}.mp3
                           """);
            lines.Add("return");
        }
    }

    private void GenerateKeys(List<string> lines)
    {
        lines.Add("");
        lines.Add($"""
                   TriggerKey := "{(ctrlRadioButton.Checked ? "Ctrl" : "Alt")}"
                   """);
        if (wasdRadioButton.Checked)
            lines.Add("""
                      UpKey := "w"
                      DownKey := "s"
                      LeftKey := "a"
                      RightKey := "d"
                      """);
        else
            lines.Add("""
                      UpKey := "up"
                      DownKey := "down"
                      LeftKey := "left"
                      RightKey := "right"
                      """);
    }

    private void saveStratagemSetButton_Click(object sender, EventArgs e)
    {
        var fKeyStratagemString = GetFKeyStratagemString();
        if (stratagemSetsComboBox.Items.Contains(fKeyStratagemString))
            return;

        stratagemSetsComboBox.Items.Add(fKeyStratagemString);
        stratagemSetsComboBox.SelectedIndex = stratagemSetsComboBox.Items.Count - 1;

        _settingsChanged = true;
    }

    private void deleteStratagemSetButton_Click(object sender, EventArgs e)
    {
        if (stratagemSetsComboBox.SelectedIndex == -1)
            return;

        stratagemSetsComboBox.Items.RemoveAt(stratagemSetsComboBox.SelectedIndex);

        _settingsChanged = true;
    }

    private void stratagemSetsComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedItem = (string?)stratagemSetsComboBox.SelectedItem;
        if (selectedItem == null)
            return;

        SetFKeyStratagemString(selectedItem);
        _settingsChanged = true;
    }

    private void playVoiceCheckBox_Click(object sender, EventArgs e)
    {
        Settings.PlayVoice = playVoiceCheckBox.Checked;
        _settingsChanged = true;
    }

    private static readonly string VoiceRootPath = Path.Combine(ExeDirectory, "Voice");

    private bool _isGeneratingVoice;

    private async void generateVoiceButton_Click(object sender, EventArgs e)
    {
        if (generateVoiceStyleComboBox.SelectedIndex == -1)
            return;

        if (_isGeneratingVoice)
        {
            _isGeneratingVoice = false;
            return;
        }

        _isGeneratingVoice = true;
        generateVoiceButton.Text = @"停止生成";

        try
        {
            var count = 0;
            var total = Stratagems.Count;
            foreach (var stratagem in Stratagems.Values)
            {
                if (!_isGeneratingVoice)
                    break;

                count++;

                var voiceName = (string)generateVoiceStyleComboBox.SelectedItem!;
                await GenerateVoiceFile(stratagem.Name, Path.Combine(VoiceRootPath, voiceName, stratagem.Name + ".mp3"));
                generateVoiceMessageLabel.Text = @$"正在生成民主语音（{count}/{total}）：{stratagem.Name}";
            }
        }
        catch (Exception)
        {
            generateVoiceMessageLabel.Text = @"民主语音生成失败...";
        }
        finally
        {
            generateVoiceMessageLabel.Text = _isGeneratingVoice
                ? @"民主语音生成完毕！"
                : @"民主语音进程中断...";

            _isGeneratingVoice = false;
            generateVoiceButton.Text = @"生成语音";
        }
    }

    private async void generateVoiceTypeComboBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
        await TryVoice();
    }

    private async Task TryVoice()
    {
        try
        {
            var text = _fKeyStratagems[SelectedFKeyIndex]?.Name ?? Stratagems.Last().Value.Name;
            var tmpMp3 = Path.GetTempFileName() + ".mp3";
            await GenerateVoiceFile(text, tmpMp3);
            PlayVoice(tmpMp3, true);
        }
        catch (Exception)
        {
            generateVoiceMessageLabel.Text = @"民主语音生成失败...";
        }
    }

    private void PlayVoice(string filePath, bool deleteAfterPlay = false)
    {
        if (_isLoading)
            return;

        if (!File.Exists(filePath))
            return;

        var reader = new Mp3FileReader(filePath);
        var waveOut = new WaveOutEvent();
        waveOut.Init(reader);
        waveOut.PlaybackStopped += (_, _) =>
        {
            reader.Dispose();
            waveOut.Dispose();
            if (deleteAfterPlay)
                File.Delete(filePath);
        };
        waveOut.Play();
    }

    private async Task GenerateVoiceFile(string text, string filePath)
    {
        var voiceDir = Path.GetDirectoryName(filePath);
        if (voiceDir == null)
            return;
        if (!Directory.Exists(voiceDir))
            Directory.CreateDirectory(voiceDir);

        var voiceName = (string)generateVoiceStyleComboBox.SelectedItem!;
        var communicate = new Communicate(text, voiceName, voiceRateTextBox.Text, voiceVolumeTextBox.Text, voicePitchTextBox.Text);
        await communicate.Save(filePath);
    }

    private async void tryVoiceButton_Click(object sender, EventArgs e)
    {
        await TryVoice();
    }

    private bool _settingsChanged;

    private async void MainForm_Deactivate(object sender, EventArgs e)
    {
        if (_isLoading)
            return;

        var shouldStart = Settings.EnableVoiceTrigger && (_settingsChanged || _autoHotkeyEngine == null);

        if (_settingsChanged)
        {
            await SaveSettings();
            _settingsChanged = false;
        }

        if (shouldStart)
            StartAutoHotkeyScript();
    }

    private void suggestionLabel_Click(object sender, EventArgs e)
    {
        generateVoiceFlowLayoutPanel.Visible = !generateVoiceFlowLayoutPanel.Visible;
    }

    private void voiceNamesComboBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
        Settings.VoiceName = voiceNamesComboBox.SelectedItem as string ?? "";
        _settingsChanged = true;

        PlayStratagemVoice(_fKeyStratagems[SelectedFKeyIndex]?.Name ?? Stratagems.Values.Last().Name);
    }

    private void refreshVoiceNamesButton_Click(object sender, EventArgs e)
    {
        Settings.VoiceName = voiceNamesComboBox.SelectedItem as string ?? "";
        LoadVoiceNames();
    }

    private void generateTxtButton_Click(object sender, EventArgs e)
    {
        var folder = "VoiceTxt";
        Directory.CreateDirectory(folder);
        foreach (var stratagem in Stratagems.Values)
            File.WriteAllText(Path.Combine(folder, stratagem.Name), stratagem.Name);
        generateVoiceMessageLabel.Text = @"txt 生成完毕";
    }

    private void enableVoiceTriggerCheckBox_Click(object sender, EventArgs e)
    {
        Settings.EnableVoiceTrigger = enableVoiceTriggerCheckBox.Checked;
        _settingsChanged = true;

        if (Settings.EnableVoiceTrigger)
            StartVoiceTrigger();
        else
            StopVoiceTrigger();
    }

    private bool _isClosing;

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        _isClosing = true;
        _autoHotkeyEngine?.Terminate();
    }

    private void enableSetFKeyByVoiceCheckBox_Click(object sender, EventArgs e)
    {
        Settings.EnableSetFKeyByVoice = enableSetFKeyByVoiceCheckBox.Checked;
        _settingsChanged = true;
    }

    private async void languageComboBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
        if (languageComboBox.SelectedItem is string language)
        {
            Settings.Language = language;
            _settingsChanged = true;

            await LoadByLanguage();
        }
    }

    private void voiceConfidenceNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        Settings.VoiceConfidence = (float)voiceConfidenceNumericUpDown.Value;
        _settingsChanged = true;
    }

    private void wakeupWordTextBox_TextChanged(object sender, EventArgs e)
    {
        Settings.WakeupWord = wakeupWordTextBox.Text.Trim();
        _settingsChanged = true;
        ResetVoiceCommand();
    }

    private void openSpeechRecognitionControlPanelButton_Click(object sender, EventArgs e)
    {
        Process.Start("control.exe", "/name Microsoft.SpeechRecognition");
    }

    private async void calibrateVoiceButton_Click(object sender, EventArgs e)
    {
        if (_voiceCommand == null)
        {
            MessageBox.Show(@"请先开启触发功能", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var scores = new float[3];
        var times = 0;
        var tipWindow = new ModalTip();

        try
        {
            _voiceCommand.CommandRecognized += TestEvent;

            tipWindow.SetTipString($"请朗读 “{wakeupWordTextBox.Text.Trim()}飞鹰空袭”");
            tipWindow.TryClosed = false;
            var task1 = Task.Run(() =>
            {
                while (times < 1 && !tipWindow.TryClosed)
                    Thread.Sleep(100);
                tipWindow.TryClosed = true;
            });
            var ret = tipWindow.ShowDialog();
            await task1;
            if (ret == DialogResult.Cancel)
                return;

            tipWindow.SetTipString($"请朗读 “{wakeupWordTextBox.Text.Trim()}轨道炮攻击”");
            tipWindow.TryClosed = false;
            var task2 = Task.Run(() =>
            {
                while (times < 2 && !tipWindow.TryClosed)
                    Thread.Sleep(100);
                tipWindow.TryClosed = true;
            });
            ret = tipWindow.ShowDialog();
            await task2;
            if (ret == DialogResult.Cancel)
                return;

            tipWindow.SetTipString($"请朗读 “{wakeupWordTextBox.Text.Trim()}消耗性反坦克武器”");
            tipWindow.TryClosed = false;
            var task3 = Task.Run(() =>
            {
                while (times < 3 && !tipWindow.TryClosed)
                    Thread.Sleep(100);
                tipWindow.TryClosed = true;
            });
            ret = tipWindow.ShowDialog();
            await task3;
            if (ret == DialogResult.Cancel)
                return;

            voiceConfidenceNumericUpDown.Value = (decimal)(scores.Average() - (scores.Max() - scores.Min()));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            _voiceCommand.CommandRecognized -= TestEvent;
        }

        return;

        void TestEvent(object? _, VoiceCommand.RecognitionResult command)
        {
            switch (command.Text)
            {
                case "飞鹰空袭" when times == 0:
                case "轨道炮攻击" when times == 1:
                case "消耗性反坦克武器" when times == 2:
                    scores[times] = command.Score;
                    times++;
                    break;
            }
        }
    }

    private void enableHotkeyTriggerCheckBox_Click(object sender, EventArgs e)
    {
        Settings.EnableHotkeyTrigger = enableHotkeyTriggerCheckBox.Checked;
        _settingsChanged = true;

        if (Settings.EnableHotkeyTrigger)
        {
            StartVoiceTrigger();
        }
        else if (_autoHotkeyEngine != null)
        {
            _autoHotkeyEngine.Terminate();
            _autoHotkeyEngine = null;
        }
    }

    private void micComboBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
        _voiceCommand?.SelectDevice(micComboBox.Text);
    }
}
