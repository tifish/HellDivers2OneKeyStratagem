using System.Diagnostics;
using System.Globalization;
using EdgeTTS;
using HellDivers2OneKeyStratagem.Tools;
using NAudio.Wave;
using NHotkey;

namespace HellDivers2OneKeyStratagem;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        SetTooltipFont();
    }

    private void SetTooltipFont()
    {
        toolTip.OwnerDraw = true;
        toolTip.Popup += (_, args) =>
        {
            var text = toolTip.GetToolTip(args.AssociatedControl);
            args.ToolTipSize = TextRenderer.MeasureText(text, Font);
        };
        toolTip.Draw += (_, args) =>
        {
            args.DrawBackground();
            args.DrawBorder();
            args.Graphics.DrawString(args.ToolTipText, Font, Brushes.Black, new PointF(2, 2));
        };
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
            InitKeysUI();

            await LoadByLanguage();

            LoadMicDevices("");
        }
        finally
        {
            rootFlowLayoutPanel.ResumeLayout();
            ResumeLayout();
        }

        CenterToScreen();

        ActiveWindowMonitor.WindowTitleChanged += OnWindowTitleChanged;
        ActiveWindowMonitor.Start(1000);

        _isLoading = false;
    }

    private const string HellDivers2Title = "HELLDIVERS™ 2";

    private async void OnWindowTitleChanged(object? sender, WindowTitleChangedEventArgs e)
    {
        var oldProcessIsActive = e.OldWindowTitle == Text || e.OldWindowTitle == HellDivers2Title;
        var newProcessIsActive = e.NewWindowTitle == Text || e.NewWindowTitle == HellDivers2Title;

        if (oldProcessIsActive && !newProcessIsActive)
        {
            await StopSpeechTrigger();
        }
        else if (!oldProcessIsActive && newProcessIsActive)
        {
            if (Settings.EnableSpeechTrigger)
                await StartSpeechTrigger();
        }
        else if (newProcessIsActive && _speechSettingsChanged)
        {
            _speechSettingsChanged = false;
            await ResetVoiceCommand();
        }

        if (e.OldWindowTitle == HellDivers2Title)
        {
            HotkeyGroupManager.Enabled = false;
        }
        else if (e.NewWindowTitle == HellDivers2Title && Settings.EnableHotkeyTrigger)
        {
            if (_keySettingsChanged)
            {
                _keySettingsChanged = false;
                SetHotkeyGroup();
            }

            HotkeyGroupManager.Enabled = true;
        }
    }

    private void SetHotkeyGroup()
    {
        var hotkeys = new Dictionary<Keys, EventHandler<HotkeyEventArgs>>();
        for (var i = 0; i < _keyStratagems.Length; i++)
        {
            var stratagem = _keyStratagems[i];
            if (stratagem == null)
                continue;

            hotkeys[_keys[i]] = (_, _) =>
            {
                if (Settings.PlayVoice)
                    PlayStratagemVoice(stratagem.Name);

                stratagem.PressKeys();
            };
        }

        HotkeyGroupManager.SetHotkeyGroup(hotkeys);
    }

    private void LoadMicDevices(string lastSelected)
    {
        micComboBox.Items.Clear();

        var selectedIndex = -1;
        for (var i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var device = WaveInEvent.GetCapabilities(i);
            micComboBox.Items.Add(device.ProductName);
            selectedIndex = device.ProductName == lastSelected ? i : selectedIndex;
        }

        micComboBox.SelectedIndex = selectedIndex;
    }

    private void InitLanguages()
    {
        var speechLocales = VoiceCommand.GetInstalledRecognizers();
        if (speechLocales.Count == 0)
            return;

        localeComboBox.Items.Clear();
        foreach (var locale in speechLocales)
            localeComboBox.Items.Add(locale);

        if (!speechLocales.Contains(Settings.Locale))
        {
            if (speechLocales.Contains(CultureInfo.CurrentCulture.Name))
                Settings.Locale = CultureInfo.CurrentCulture.Name;
            else
                Settings.Locale = speechLocales.First();

            _keySettingsChanged = true;
            _speechSettingsChanged = true;
        }

        localeComboBox.SelectedItem = Settings.Locale;
    }

    private async Task LoadByLanguage()
    {
        StratagemManager.Load();
        InitStratagemGroupsUI();
        InitSettingsToUI();
        SetHotkeyGroup();

        await ResetVoiceCommand();

        await LoadGeneratingVoiceStyles();

        LoadVoiceNames();
    }

    private async Task ResetVoiceCommand()
    {
        await StopSpeechTrigger();

        if (Settings.EnableSpeechTrigger)
            await StartSpeechTrigger();
    }

    private VoiceCommand? _voiceCommand;

    private async Task StartSpeechTrigger()
    {
        if (_voiceCommand == null)
        {
            var languages = VoiceCommand.GetInstalledRecognizers();
            if (languages.Count == 0)
            {
                MessageBox.Show(@"No voice recognition engine installed");
                return;
            }

            if (!languages.Contains(Settings.Locale))
            {
                MessageBox.Show($@"Voice recognition engine for {Settings.Locale} not installed");
                return;
            }

            try
            {
                _voiceCommand = new VoiceCommand(Settings.Locale, Settings.WakeupWord, [.. StratagemManager.StratagemAlias]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"创建语音识别失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _voiceCommand.CommandRecognized += (_, command) =>
            {
                var failed = command.Score < Settings.VoiceConfidence;
                voiceRecognizeResultLabel.Text = $@"【{(failed ? "失败" : "成功")}】识别阈值：{command.Score:F3} 识别文字：{command.Text} 当前程序：{ActiveWindowMonitor.CurrentProcessFileName}";

                if (failed)
                    return;

                if (!StratagemManager.TryGet(command.Text, out var stratagem))
                    return;

                if (ActiveWindowMonitor.CurrentWindowTitle == Text && Settings is { EnableHotkeyTrigger: true, EnableSetKeyBySpeech: true })
                {
                    SetKeyStratagem(SelectedKeyIndex, stratagem);
                }
                else if (ActiveWindowMonitor.CurrentWindowTitle == HellDivers2Title)
                {
                    if (Settings.PlayVoice)
                        PlayStratagemVoice(stratagem.Name);

                    stratagem.PressKeys();
                }
            };

            if (micComboBox.Text != "")
                await _voiceCommand.UseMic(micComboBox.Text);
        }

        _voiceCommand.Start();
    }

    private void PlayStratagemVoice(string stratagemName)
    {
        PlayVoice(Path.Combine(VoiceRootPath, Settings.Language, Settings.VoiceName, stratagemName + ".mp3"));
    }

    private async Task StopSpeechTrigger()
    {
        if (_voiceCommand != null)
        {
            await _voiceCommand.Stop();
            _voiceCommand.Dispose();
            _voiceCommand = null;
        }
    }

    private void LoadVoiceNames()
    {
        var languageVoicePath = Path.Combine(VoiceRootPath, Settings.Language);

        var styles = Directory.GetDirectories(languageVoicePath)
            .Select(Path.GetFileName)
            .Where(style => style != null)
            .ToList();

        voiceNamesComboBox.Items.Clear();
        foreach (var style in styles)
            voiceNamesComboBox.Items.Add(style!);

        voiceNamesComboBox.SelectedItem =
            styles.Contains(Settings.VoiceName)
                ? Settings.VoiceName
                : styles.FirstOrDefault();
    }

    private List<Voice> _voices = null!;

    private async Task LoadGeneratingVoiceStyles()
    {
        if (Settings.Locale == "")
            return;

        var manager = await VoicesManager.Create();
        _voices = manager.Find(language: Settings.Language);

        generateVoiceStyleComboBox.Items.Clear();
        foreach (var voice in _voices)
            generateVoiceStyleComboBox.Items.Add(voice.ShortName);

        generateVoiceStyleComboBox.SelectedIndex = generateVoiceStyleComboBox.Items.Count - 1;
    }

    private readonly JsonFile<AppSettings> _settingsFile = new(AppSettings.SettingsFile);

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
            SetKeyStratagemString(Settings.StratagemSets[0]);

            stratagemSetsComboBox.Items.Clear();
            foreach (var stratagemSet in Settings.StratagemSets.Skip(1))
                stratagemSetsComboBox.Items.Add(stratagemSet);
        }

        speechConfidenceNumericUpDown.Value = (decimal)Settings.VoiceConfidence;
        wakeupWordTextBox.Text = Settings.WakeupWord;

        enableSpeechTriggerCheckBox.Checked = Settings.EnableSpeechTrigger;
        enableSpeechTriggerCheckBox_Click(enableSpeechTriggerCheckBox, EventArgs.Empty);

        enableHotkeyTriggerCheckBox.Checked = Settings.EnableHotkeyTrigger;
        enableHotkeyTriggerCheckBox_Click(enableHotkeyTriggerCheckBox, EventArgs.Empty);

        enableSetKeyBySpeechCheckBox.Checked = Settings.EnableSetKeyBySpeech;

        updateUrlTextBox.Text = Settings.UpdateUrl;
    }

    private string GetKeyStratagemString()
    {
        return string.Join(';', _keyStratagems.Select(s => s?.Name ?? ""));
    }

    private void SetKeyStratagemString(string value)
    {
        var names = value.Split(';');
        var count = Math.Min(names.Length, KeyCount);
        for (var i = 0; i < count; i++)
        {
            var name = names[i];
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (!StratagemManager.TryGet(name, out var stratagem))
                continue;

            SetKeyStratagem(i, stratagem, false);
        }
    }

    private async Task SaveSettings()
    {
        Settings.StratagemSets.Clear();
        Settings.StratagemSets.Add(GetKeyStratagemString());
        foreach (var item in stratagemSetsComboBox.Items)
            Settings.StratagemSets.Add((string)item);

        await _settingsFile.Save(Settings);
    }

    private const int KeyCount = 17;

    private readonly Keys[] _keys =
    [
        Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11,
        Keys.Insert, Keys.Delete, Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown,
    ];

    private readonly Label[] _keyLabels = new Label[KeyCount];
    private readonly Stratagem?[] _keyStratagems = new Stratagem?[KeyCount];
    private readonly FlowLayoutPanel[] _keyContains = new FlowLayoutPanel[KeyCount];

    private int _selectedKeyIndex = KeyCount - 1;

    private int SelectedKeyIndex
    {
        get => _selectedKeyIndex;
        set
        {
            if (value == _selectedKeyIndex)
                return;

            rootFlowLayoutPanel.SuspendLayout();

            try
            {
                if (_selectedKeyIndex >= 0)
                    _keyContains[SelectedKeyIndex].BorderStyle = BorderStyle.None;
                _keyContains[value].BorderStyle = BorderStyle.FixedSingle;
                _selectedKeyIndex = value;
            }
            finally
            {
                rootFlowLayoutPanel.ResumeLayout();
            }
        }
    }

    private const string NoStratagem = "无";

    private void InitKeysUI()
    {
        keysFlowLayoutPanel1.Controls.Clear();
        keysFlowLayoutPanel2.Controls.Clear();
        var keysFlowLayoutPanel = keysFlowLayoutPanel1;

        for (var i = 0; i < KeyCount; i++)
        {
            if (i == 11)
                keysFlowLayoutPanel = keysFlowLayoutPanel2;

            var root = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var keyLabel = new Label { Text = Enum.GetName(_keys[i]), AutoSize = true, Anchor = AnchorStyles.Top };
            var stratagemLabel = new Label { Text = NoStratagem, AutoSize = true, Anchor = AnchorStyles.Top };
            root.Controls.Add(keyLabel);
            root.Controls.Add(stratagemLabel);
            keysFlowLayoutPanel.Controls.Add(root);

            var i1 = i;
            keyLabel.MouseDown += OnKeyMouseDown;
            stratagemLabel.MouseDown += OnKeyMouseDown;
            root.MouseDown += OnKeyMouseDown;

            _keyLabels[i] = stratagemLabel;
            _keyContains[i] = root;
            continue;

            void OnKeyMouseDown(object? o, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return;
                SelectedKeyIndex = i1;

                if (!Settings.PlayVoice)
                    return;

                if (Settings.EnableSetKeyBySpeech)
                    return;

                var stratagem = _keyStratagems[SelectedKeyIndex];
                if (stratagem != null)
                    PlayStratagemVoice(stratagem.Name);
            }
        }

        SelectedKeyIndex = _keyContains.Length - 1;
        _keyContains.Last().BorderStyle = BorderStyle.FixedSingle;
    }

    private void SetKeyStratagem(int index, Stratagem? stratagem, bool playVoice = true)
    {
        var currentStratagem = _keyStratagems[index];
        if (currentStratagem != null)
            currentStratagem.CheckBox.Checked = false;

        _keyStratagems[index] = stratagem;
        if (stratagem != null)
        {
            stratagem.CheckBox.Checked = true;
            _keyLabels[index].Text = stratagem.Name;

            if (Settings.PlayVoice && playVoice)
                PlayStratagemVoice(stratagem.Name);
        }
        else
        {
            _keyLabels[index].Text = NoStratagem;
        }

        if (!_isLoading)
            _keySettingsChanged = true;
    }

    private void InitStratagemGroupsUI()
    {
        stratagemGroupsFlowLayoutPanel.Controls.Clear();

        foreach (var (groupName, stratagems) in StratagemManager.Groups)
        {
            var groupContainer = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var groupLabel = new Label { Text = groupName, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font(Font, FontStyle.Bold) };
            groupContainer.Controls.Add(groupLabel);

            foreach (var stratagem in stratagems)
            {
                var stratagemCheckBox = new CheckBox
                {
                    Text = stratagem.Name,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                };
                stratagem.CheckBox = stratagemCheckBox;
                groupContainer.Controls.Add(stratagemCheckBox);

                toolTip.SetToolTip(stratagemCheckBox,
                    $"""
                     {StratagemManager.GetSystemAlias(stratagem.Name)}
                         自定义名称：{StratagemManager.GetUserAlias(stratagem.Name)}
                         按右键编辑自定义名称。
                     """);

                stratagemCheckBox.MouseUp += async (_, args) =>
                {
                    switch (args.Button)
                    {
                        case MouseButtons.Left:
                            {
                                if (!Settings.EnableHotkeyTrigger)
                                {
                                    stratagemCheckBox.Checked = !stratagemCheckBox.Checked;
                                    return;
                                }

                                if (stratagemCheckBox.Checked)
                                {
                                    SetKeyStratagem(SelectedKeyIndex, stratagem);

                                    if (SelectedKeyIndex > 0)
                                        if (_keyLabels[SelectedKeyIndex - 1].Text == NoStratagem)
                                            SelectedKeyIndex--;
                                }
                                else
                                {
                                    var keyIndex = Array.IndexOf(_keyStratagems, stratagem);
                                    if (keyIndex > -1)
                                        SetKeyStratagem(keyIndex, null);
                                }

                                break;
                            }

                        case MouseButtons.Right:
                            var result = new EditAliasesDialog(stratagemCheckBox.Text).ShowDialog();
                            if (result == DialogResult.OK)
                                await ResetVoiceCommand();
                            break;
                    }
                };
            }

            stratagemGroupsFlowLayoutPanel.Controls.Add(groupContainer);
        }
    }

    private void ctrlRadioButton_Click(object sender, EventArgs e)
    {
        Settings.TriggerKey = ctrlRadioButton.Checked ? "Ctrl" : "Alt";
        if (!_isLoading)
            _keySettingsChanged = true;
    }

    private void wasdRadioButton_Click(object sender, EventArgs e)
    {
        Settings.OperateKeys = wasdRadioButton.Checked ? "WASD" : "Arrow";
        if (!_isLoading)
            _keySettingsChanged = true;
    }

    private void saveStratagemSetButton_Click(object sender, EventArgs e)
    {
        var keyStratagemString = GetKeyStratagemString();
        if (stratagemSetsComboBox.Items.Contains(keyStratagemString))
            return;

        stratagemSetsComboBox.Items.Add(keyStratagemString);
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

        SetKeyStratagemString(selectedItem);
        _keySettingsChanged = true;
    }

    private void playVoiceCheckBox_Click(object sender, EventArgs e)
    {
        Settings.PlayVoice = playVoiceCheckBox.Checked;
        _keySettingsChanged = true;
    }

    private static readonly string VoiceRootPath = Path.Combine(AppSettings.ExeDirectory, "Voice");

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
            var total = StratagemManager.Count;
            var voiceName = (string)generateVoiceStyleComboBox.SelectedItem!;

            foreach (var stratagem in StratagemManager.Stratagems)
            {
                if (!_isGeneratingVoice)
                    break;

                count++;

                await GenerateVoiceFile(stratagem.Name,
                    Path.Combine(VoiceRootPath, Settings.Language, voiceName, stratagem.Name + ".mp3"));
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
            var text = _keyStratagems[SelectedKeyIndex]?.Name ?? StratagemManager.Stratagems.Last().Name;
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
    private bool _keySettingsChanged;
    private bool _speechSettingsChanged;

    private async void MainForm_Deactivate(object sender, EventArgs e)
    {
        if (_isLoading)
            return;

        if (!_settingsChanged)
            return;

        await SaveSettings();
        _settingsChanged = false;
    }

    private void suggestionLabel_Click(object sender, EventArgs e)
    {
        generateVoiceFlowLayoutPanel.Visible = !generateVoiceFlowLayoutPanel.Visible;
    }

    private void voiceNamesComboBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
        Settings.VoiceName = voiceNamesComboBox.SelectedItem as string ?? "";
        if (Settings.PlayVoice)
            _keySettingsChanged = true;
        else
            _settingsChanged = true;

        // The stratagem may be invalid after changing the language
        var stratagem = _keyStratagems[SelectedKeyIndex];
        var stratagemName = stratagem != null && StratagemManager.Stratagems.Contains(stratagem)
            ? stratagem.Name
            : StratagemManager.Stratagems.Last().Name;
        PlayStratagemVoice(stratagemName);
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
        foreach (var stratagem in StratagemManager.Stratagems)
            File.WriteAllText(Path.Combine(folder, stratagem.Name), stratagem.Name);
        generateVoiceMessageLabel.Text = @"txt 生成完毕";
    }

    private async void enableSpeechTriggerCheckBox_Click(object sender, EventArgs e)
    {
        if (!_isLoading)
        {
            Settings.EnableSpeechTrigger = enableSpeechTriggerCheckBox.Checked;
            _settingsChanged = true;

            if (Settings.EnableSpeechTrigger)
                await StartSpeechTrigger();
            else
                await StopSpeechTrigger();
        }

        speechSubSettingsFlowLayoutPanel.Visible = Settings.EnableSpeechTrigger;
        enableSetKeyBySpeechCheckBox.Enabled = Settings.EnableSpeechTrigger;
        stratagemsFlowLayoutPanel.Visible = Settings.EnableSpeechTrigger || Settings.EnableHotkeyTrigger;
    }

    private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        HotkeyGroupManager.ClearHotkeyGroup();
        await StopSpeechTrigger();
    }

    private void EnableSetKeyBySpeechCheckBoxClick(object sender, EventArgs e)
    {
        Settings.EnableSetKeyBySpeech = enableSetKeyBySpeechCheckBox.Checked;
        _settingsChanged = true;
    }

    private async void LocaleComboBoxSelectionChangeCommitted(object sender, EventArgs e)
    {
        if (localeComboBox.SelectedItem is string locale)
        {
            Settings.Locale = locale;
            if (Settings.PlayVoice)
                _keySettingsChanged = true;
            _speechSettingsChanged = true;

            await LoadByLanguage();
        }
    }

    private void speechConfidenceNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        Settings.VoiceConfidence = (float)speechConfidenceNumericUpDown.Value;
        _settingsChanged = true;
    }

    private async void wakeupWordTextBox_TextChanged(object sender, EventArgs e)
    {
        Settings.WakeupWord = wakeupWordTextBox.Text.Trim();
        _settingsChanged = true;
        await ResetVoiceCommand();
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
        var tipWindow = new CalibrateVoiceDialog();

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

            speechConfidenceNumericUpDown.Value = (decimal)(scores.Average() - (scores.Max() - scores.Min()));
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
        if (!_isLoading)
        {
            Settings.EnableHotkeyTrigger = enableHotkeyTriggerCheckBox.Checked;
            _settingsChanged = true;

            HotkeyGroupManager.Enabled = Settings.EnableHotkeyTrigger;
        }

        enableSetKeyBySpeechCheckBox.Visible = Settings.EnableHotkeyTrigger;
        hotKeyFlowLayoutPanel.Visible = Settings.EnableHotkeyTrigger;
        stratagemsFlowLayoutPanel.Visible = Settings.EnableSpeechTrigger || Settings.EnableHotkeyTrigger;
    }

    private void micComboBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
        _voiceCommand?.UseMic(micComboBox.Text);
    }

    private void micComboBox_DropDown(object sender, EventArgs e)
    {
        LoadMicDevices(micComboBox.Text);
    }

    private async void checkForUpdateButton_Click(object sender, EventArgs e)
    {
        checkForUpdateButton.Enabled = false;

        try
        {
            if (await AutoUpdate.HasUpdate())
            {
                if (MessageBox.Show(@"发现新版本，是否更新？", @"提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    AutoUpdate.Update();
            }
            else
            {
                MessageBox.Show(@"已经是最新版本", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        finally
        {
            checkForUpdateButton.Enabled = true;
        }
    }

    private void openSettingsButton_Click(object sender, EventArgs e)
    {
        settingsFlowLayoutPanel.Show();
        openSettingsButton.Visible = false;
        closeSettingsButton.Visible = true;
    }

    private void closeSettingsButton_Click(object sender, EventArgs e)
    {
        settingsFlowLayoutPanel.Hide();
        openSettingsButton.Visible = true;
        closeSettingsButton.Visible = false;
    }

    private void updateUrlTextBox_TextChanged(object sender, EventArgs e)
    {
        Settings.UpdateUrl = updateUrlTextBox.Text;
        _settingsChanged = true;
    }
}
