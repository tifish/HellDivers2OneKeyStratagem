using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EdgeTTS;
using iNKORE.UI.WPF.Modern.Controls;
using NAudio.Wave;
using NHotkey;
using Brushes = System.Windows.Media.Brushes;
using CheckBox = System.Windows.Controls.CheckBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;

namespace HellDivers2OneKeyStratagem;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private int _isLoadingCounter;

    private bool IsLoading
    {
        get => _isLoadingCounter > 0;
        set
        {
            if (value)
                _isLoadingCounter++;
            else
                _isLoadingCounter--;
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        IsLoading = true;
        BeginInit();

        try
        {
            await AppSettings.LoadSettings();

            InitLanguages();
            InitKeysUI();

            await LoadByLanguage();
        }
        finally
        {
            EndInit();
            IsLoading = false;
        }

        ActiveWindowMonitor.WindowTitleChanged += OnWindowTitleChanged;
        ActiveWindowMonitor.Start(TimeSpan.FromSeconds(1));
    }

    private const string HellDivers2Title = "HELLDIVERS™ 2";

    private async void OnWindowTitleChanged(object? sender, WindowTitleChangedEventArgs e)
    {
        var oldProcessIsActive = e.OldWindowTitle == Title || e.OldWindowTitle == HellDivers2Title;
        var newProcessIsActive = e.NewWindowTitle == Title || e.NewWindowTitle == HellDivers2Title;

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
        var hotkeys = new Dictionary<Key, EventHandler<HotkeyEventArgs>>();
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

    private void InitLanguages()
    {
        var speechLocales = VoiceCommand.GetInstalledRecognizers();
        if (speechLocales.Count == 0)
            return;

        LocaleComboBox.Items.Clear();
        foreach (var locale in speechLocales)
            LocaleComboBox.Items.Add(locale);

        if (!speechLocales.Contains(Settings.Locale))
        {
            if (speechLocales.Contains(CultureInfo.CurrentCulture.Name))
                Settings.Locale = CultureInfo.CurrentCulture.Name;
            else
                Settings.Locale = speechLocales.First();

            _keySettingsChanged = true;
            _speechSettingsChanged = true;
        }

        LocaleComboBox.SelectedItem = Settings.Locale;
    }

    private const int KeyCount = 17;

    private readonly Key[] _keys =
    [
        Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10, Key.F11,
        Key.Insert, Key.Delete, Key.Home, Key.End, Key.PageUp, Key.PageDown,
    ];

    private readonly Label[] _keyLabels = new Label[KeyCount];
    private readonly Stratagem?[] _keyStratagems = new Stratagem?[KeyCount];
    private readonly Border[] _keyBorders = new Border[KeyCount];

    private int _selectedKeyIndex = KeyCount - 1;

    private int SelectedKeyIndex
    {
        get => _selectedKeyIndex;
        set
        {
            if (value == _selectedKeyIndex)
                return;

            BeginInit();

            try
            {
                if (_selectedKeyIndex >= 0)
                    _keyBorders[SelectedKeyIndex].BorderBrush = Brushes.Transparent;
                _keyBorders[value].BorderBrush = Brushes.Gray;
                _selectedKeyIndex = value;
            }
            finally
            {
                EndInit();
            }
        }
    }

    private const string NoStratagem = "无";

    private void InitKeysUI()
    {
        KeysStackPanel1.Children.Clear();
        KeysStackPanel2.Children.Clear();
        var keysStackPanel = KeysStackPanel1;

        for (var i = 0; i < KeyCount; i++)
        {
            if (i == 11)
                keysStackPanel = KeysStackPanel2;

            var border = new Border { BorderThickness = new Thickness(1), Padding = new Thickness(5) };
            keysStackPanel.Children.Add(border);
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical, MinWidth = 64, Background = Brushes.Transparent };
            border.Child = stackPanel;

            var keyLabel = new Label { Content = Enum.GetName(_keys[i]), HorizontalAlignment = HorizontalAlignment.Center };
            var stratagemLabel = new Label { Content = NoStratagem, HorizontalAlignment = HorizontalAlignment.Center };
            stackPanel.Children.Add(keyLabel);
            stackPanel.Children.Add(stratagemLabel);

            var i1 = i;
            stackPanel.MouseDown += (_, e) =>
            {
                if (e.ChangedButton == MouseButton.Right)
                    return;
                SelectedKeyIndex = i1;

                if (!Settings.PlayVoice)
                    return;

                if (Settings.EnableSetKeyBySpeech)
                    return;

                var stratagem = _keyStratagems[SelectedKeyIndex];
                if (stratagem != null)
                    PlayStratagemVoice(stratagem.Name);
            };

            _keyLabels[i] = stratagemLabel;
            _keyBorders[i] = border;
        }

        SelectedKeyIndex = _keyBorders.Length - 1;
        _keyBorders.Last().BorderBrush = Brushes.Gray;
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

    private void InitStratagemGroupsUI()
    {
        StratagemGroupsStackPanel.Children.Clear();

        foreach (var (groupName, stratagems) in StratagemManager.Groups)
        {
            var groupContainer = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Vertical,
            };
            var groupLabel = new Label { Content = groupName, HorizontalAlignment = HorizontalAlignment.Center };
            groupContainer.Children.Add(groupLabel);

            foreach (var stratagem in stratagems)
            {
                var stratagemCheckBox = new CheckBox
                {
                    Content = stratagem.Name,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                stratagem.CheckBox = stratagemCheckBox;
                groupContainer.Children.Add(stratagemCheckBox);

                stratagemCheckBox.ToolTip = $"""
                                             {StratagemManager.GetSystemAlias(stratagem.Name)}
                                                 自定义名称：{StratagemManager.GetUserAlias(stratagem.Name)}
                                                 按右键编辑自定义名称。
                                             """;

                stratagemCheckBox.Checked += OnStratagemCheckBoxCheckedAndUnchecked;
                stratagemCheckBox.Unchecked += OnStratagemCheckBoxCheckedAndUnchecked;

                stratagemCheckBox.MouseDown += async (_, args) =>
                {
                    if (args.ChangedButton != MouseButton.Right)
                        return;

                    var page = new EditAliasesDialog((string)stratagemCheckBox.Content);
                    var dialog = new ContentDialog
                    {
                        Title = "编辑自定义名称",
                        Content = page,
                        PrimaryButtonText = "确定",
                        SecondaryButtonText = "取消",
                    };

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        page.Commit();
                        await ResetVoiceCommand();
                    }
                };

                continue;

                void OnStratagemCheckBoxCheckedAndUnchecked(object o, RoutedEventArgs routedEventArgs)
                {
                    if (_isSettingStratagemCheckBoxChecked)
                        return;

                    if (!Settings.EnableHotkeyTrigger)
                    {
                        _isSettingStratagemCheckBoxChecked = true;
                        routedEventArgs.Handled = true;
                        _isSettingStratagemCheckBoxChecked = false;
                        return;
                    }

                    if (stratagemCheckBox.IsChecked == true)
                    {
                        SetKeyStratagem(SelectedKeyIndex, stratagem);

                        if (SelectedKeyIndex > 0)
                            if ((string)_keyLabels[SelectedKeyIndex - 1].Content == NoStratagem)
                                SelectedKeyIndex--;
                    }
                    else
                    {
                        var keyIndex = Array.IndexOf(_keyStratagems, stratagem);
                        if (keyIndex > -1)
                        {
                            SetKeyStratagem(keyIndex, null);
                            SelectedKeyIndex = keyIndex;
                        }
                    }
                }
            }

            StratagemGroupsStackPanel.Children.Add(groupContainer);
        }
    }

    private void InitSettingsToUI()
    {
        switch (Settings.TriggerKey)
        {
            case "Ctrl":
                CtrlRadioButton.IsChecked = true;
                break;
            case "Alt":
                AltRadioButton.IsChecked = true;
                break;
        }

        switch (Settings.OperateKeys)
        {
            case "WASD":
                WasdRadioButton.IsChecked = true;
                break;
            case "Arrow":
                ArrowRadioButton.IsChecked = true;
                break;
        }

        PlayVoiceCheckBox.IsChecked = Settings.PlayVoice;

        if (Settings.StratagemSets.Count > 0)
        {
            SetKeyStratagemString(Settings.StratagemSets[0]);

            StratagemSetsComboBox.Items.Clear();
            foreach (var stratagemSet in Settings.StratagemSets.Skip(1))
                StratagemSetsComboBox.Items.Add(stratagemSet);
        }

        SpeechConfidenceNumberBox.Value = MathF.Round(Settings.VoiceConfidence, 3);
        WakeupWordTextBox.Text = Settings.WakeupWord;

        EnableSpeechTriggerCheckBox.IsChecked = Settings.EnableSpeechTrigger;

        EnableHotkeyTriggerCheckBox.IsChecked = Settings.EnableHotkeyTrigger;

        EnableSetKeyBySpeechCheckBox.IsChecked = Settings.EnableSetKeyBySpeech;

        UpdateUrlTextBox.Text = Settings.UpdateUrl;
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
            if (string.IsNullOrWhiteSpace(name) || !StratagemManager.TryGet(name, out var stratagem))
                SetKeyStratagem(i, null, false);
            else
                SetKeyStratagem(i, stratagem, false);
        }
    }

    private bool _isSettingStratagemCheckBoxChecked;

    private List<Voice> _voices = null!;

    private async Task LoadGeneratingVoiceStyles()
    {
        if (Settings.Locale == "")
            return;

        IsLoading = true;

        try
        {
            var manager = await VoicesManager.Create();
            _voices = manager.Find(language: Settings.Language);

            GenerateVoiceStyleComboBox.Items.Clear();
            foreach (var voice in _voices)
                GenerateVoiceStyleComboBox.Items.Add(voice.ShortName);

            GenerateVoiceStyleComboBox.SelectedIndex = GenerateVoiceStyleComboBox.Items.Count - 1;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadVoiceNames()
    {
        IsLoading = true;

        try
        {
            var languageVoicePath = Path.Combine(VoiceRootPath, Settings.Language);

            var styles = Directory.GetDirectories(languageVoicePath)
                .Select(Path.GetFileName)
                .Where(style => style != null)
                .ToList();

            VoiceNamesComboBox.Items.Clear();
            foreach (var style in styles)
                VoiceNamesComboBox.Items.Add(style!);

            VoiceNamesComboBox.SelectedItem =
                styles.Contains(Settings.VoiceName)
                    ? Settings.VoiceName
                    : styles.FirstOrDefault();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadMicDevices(string lastSelected)
    {
        MicComboBox.Items.Clear();

        var selectedIndex = -1;
        for (var i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var device = WaveInEvent.GetCapabilities(i);
            MicComboBox.Items.Add(device.ProductName);
            selectedIndex = device.ProductName == lastSelected ? i : selectedIndex;
        }

        MicComboBox.SelectedIndex = selectedIndex;
    }

    private void SetKeyStratagem(int index, Stratagem? stratagem, bool playVoice = true)
    {
        if (index is < 0 or > KeyCount - 1)
            return;

        // Uncheck the previous stratagem
        var currentStratagem = _keyStratagems[index];
        if (currentStratagem != null)
        {
            _isSettingStratagemCheckBoxChecked = true;
            currentStratagem.CheckBox.IsChecked = false;
            _isSettingStratagemCheckBoxChecked = false;
        }

        if (stratagem != null)
        {
            // Remove the previous hotkey
            if (stratagem.CheckBox.IsChecked == true)
            {
                var prevIndex = Array.IndexOf(_keyStratagems, stratagem);
                if (prevIndex > -1)
                {
                    _keyStratagems[prevIndex] = null;
                    _keyLabels[prevIndex].Content = NoStratagem;
                }
            }

            // Set the new hotkey
            _keyStratagems[index] = stratagem;
            _keyLabels[index].Content = stratagem.Name;

            _isSettingStratagemCheckBoxChecked = true;
            stratagem.CheckBox.IsChecked = true;
            _isSettingStratagemCheckBoxChecked = false;

            if (Settings.PlayVoice && playVoice)
                PlayStratagemVoice(stratagem.Name);
        }
        else
        {
            _keyStratagems[index] = null;
            _keyLabels[index].Content = NoStratagem;
        }

        if (!IsLoading)
            _keySettingsChanged = true;
    }

    private static readonly string VoiceRootPath = Path.Combine(AppSettings.ExeDirectory, "Voice");

    private void PlayStratagemVoice(string stratagemName)
    {
        PlayVoice(Path.Combine(VoiceRootPath, Settings.Language, Settings.VoiceName, stratagemName + ".mp3"));
    }

    private void PlayVoice(string filePath, bool deleteAfterPlay = false)
    {
        if (IsLoading)
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


    private bool _settingsChanged;
    private bool _keySettingsChanged;
    private bool _speechSettingsChanged;

    private async void Window_Deactivated(object sender, EventArgs e)
    {
        if (IsLoading)
            return;

        if (!_settingsChanged)
            return;

        Settings.StratagemSets.Clear();
        Settings.StratagemSets.Add(GetKeyStratagemString());
        foreach (var item in StratagemSetsComboBox.Items)
            Settings.StratagemSets.Add((string)item);
        await AppSettings.SaveSettings();

        _settingsChanged = false;
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
                await new ContentDialog
                {
                    Title = "错误",
                    Content = new Label { Content = "没有安装语音识别引擎" },
                    PrimaryButtonText = "确定",
                }.ShowAsync();
                return;
            }

            if (!languages.Contains(Settings.Locale))
            {
                await new ContentDialog
                {
                    Title = "错误",
                    Content = new Label { Content = $"没有安装 {Settings.Locale} 的语音识别引擎" },
                    PrimaryButtonText = "确定",
                }.ShowAsync();
                return;
            }

            try
            {
                _voiceCommand = new VoiceCommand(Settings.Locale, Settings.WakeupWord, [.. StratagemManager.StratagemAlias]);
            }
            catch (Exception)
            {
                await new ContentDialog
                {
                    Title = "错误",
                    Content = new Label { Content = "创建语音识别失败" },
                    PrimaryButtonText = "确定",
                }.ShowAsync();
                return;
            }

            _voiceCommand.CommandRecognized += (_, command) =>
            {
                var failed = command.Score < Settings.VoiceConfidence;
                VoiceRecognizeResultLabel.Content = $@"【{(failed ? "失败" : "成功")}】识别阈值：{command.Score:F3} 识别文字：{command.Text} 当前程序：{ActiveWindowMonitor.CurrentProcessFileName}";

                if (failed)
                    return;

                if (!StratagemManager.TryGet(command.Text, out var stratagem))
                    return;

                if (ActiveWindowMonitor.CurrentWindowTitle == Title && Settings is { EnableHotkeyTrigger: true, EnableSetKeyBySpeech: true })
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

            if (MicComboBox.Text != "")
                await _voiceCommand.UseMic(MicComboBox.Text);
        }

        _voiceCommand.Start();
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

    private async void LocaleComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoading)
            return;

        if (LocaleComboBox.SelectedItem is string locale)
        {
            Settings.Locale = locale;
            if (Settings.PlayVoice)
                _keySettingsChanged = true;
            _speechSettingsChanged = true;

            await LoadByLanguage();
        }
    }

    private void VoiceNamesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoading)
            return;

        Settings.VoiceName = VoiceNamesComboBox.SelectedItem as string ?? "";
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

    private void RefreshVoiceNamesButton_OnClick(object sender, RoutedEventArgs e)
    {
        Settings.VoiceName = VoiceNamesComboBox.SelectedItem as string ?? "";
        LoadVoiceNames();
    }

    private void CtrlRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (IsLoading)
            return;

        Settings.TriggerKey = "Ctrl";
        _keySettingsChanged = true;
    }

    private void AltRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (IsLoading)
            return;

        Settings.TriggerKey = "Alt";
        _keySettingsChanged = true;
    }

    private void WasdRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (IsLoading)
            return;

        Settings.OperateKeys = "WASD";
        _keySettingsChanged = true;
    }

    private void ArrowRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (IsLoading)
            return;

        Settings.OperateKeys = "Arrow";
        _keySettingsChanged = true;
    }

    private void UpdateUrlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoading)
            return;

        Settings.UpdateUrl = UpdateUrlTextBox.Text;
        _settingsChanged = true;
    }

    private async void CheckForUpdateButton_OnClick(object sender, RoutedEventArgs e)
    {
        CheckForUpdateButton.IsEnabled = false;

        try
        {
            if (await AutoUpdate.HasUpdate())
            {
                var dialogResult = await new ContentDialog
                {
                    Title = "提示",
                    Content = new Label { Content = "发现新版本，是否更新？" },
                    PrimaryButtonText = "是",
                    SecondaryButtonText = "否",
                }.ShowAsync();
                if (dialogResult == ContentDialogResult.Primary)
                    AutoUpdate.Update();
            }
            else
            {
                await new ContentDialog
                {
                    Title = "提示",
                    Content = new Label { Content = "已经是最新版本" },
                    PrimaryButtonText = "确定",
                }.ShowAsync();
            }
        }
        finally
        {
            CheckForUpdateButton.IsEnabled = true;
        }
    }

    private async void EnableSpeechTriggerCheckBox_OnCheckedUnchecked(object sender, RoutedEventArgs e)
    {
        if (!IsLoading)
        {
            Settings.EnableSpeechTrigger = EnableSpeechTriggerCheckBox.IsChecked == true;
            _settingsChanged = true;

            if (Settings.EnableSpeechTrigger)
                await StartSpeechTrigger();
            else
                await StopSpeechTrigger();
        }

        SpeechSubSettingsStackPanel.Visibility = Settings.EnableSpeechTrigger ? Visibility.Visible : Visibility.Collapsed;
        EnableSetKeyBySpeechCheckBox.IsEnabled = Settings.EnableSpeechTrigger;
        StratagemsStackPanel.Visibility = Settings.EnableSpeechTrigger || Settings.EnableHotkeyTrigger ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OpenSpeechRecognitionControlPanelButton_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("control.exe", "/name Microsoft.SpeechRecognition");
    }

    private async void WakeupWordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoading)
            return;

        Settings.WakeupWord = WakeupWordTextBox.Text.Trim();
        _settingsChanged = true;
        await ResetVoiceCommand();
    }

    private void SpeechConfidenceNumberBox_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (IsLoading)
            return;

        Settings.VoiceConfidence = MathF.Round((float)SpeechConfidenceNumberBox.Value, 3);
        _settingsChanged = true;
    }

    private void MicLabel_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        GenerateVoiceStackPanel.Visibility = GenerateVoiceStackPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void EnableHotkeyTriggerCheckBox_OnCheckedUnchecked(object sender, RoutedEventArgs e)
    {
        if (!IsLoading)
        {
            Settings.EnableHotkeyTrigger = EnableHotkeyTriggerCheckBox.IsChecked == true;
            _settingsChanged = true;

            HotkeyGroupManager.Enabled = Settings.EnableHotkeyTrigger;
        }

        EnableSetKeyBySpeechCheckBox.Visibility = Settings.EnableHotkeyTrigger ? Visibility.Visible : Visibility.Collapsed;
        HotKeyStackPanel.Visibility = Settings.EnableHotkeyTrigger ? Visibility.Visible : Visibility.Collapsed;
        StratagemsStackPanel.Visibility = Settings.EnableSpeechTrigger || Settings.EnableHotkeyTrigger ? Visibility.Visible : Visibility.Collapsed;
    }

    private void EnableSetKeyBySpeechCheckBox_OnCheckedUnchecked(object sender, RoutedEventArgs e)
    {
        if (IsLoading)
            return;

        Settings.EnableSetKeyBySpeech = EnableSetKeyBySpeechCheckBox.IsChecked == true;
        _settingsChanged = true;
    }

    private void SaveStratagemSetButton_OnClick(object sender, RoutedEventArgs e)
    {
        var keyStratagemString = GetKeyStratagemString();
        if (StratagemSetsComboBox.Items.Contains(keyStratagemString))
            return;

        StratagemSetsComboBox.Items.Add(keyStratagemString);
        StratagemSetsComboBox.SelectedIndex = StratagemSetsComboBox.Items.Count - 1;

        _settingsChanged = true;
    }

    private void DeleteStratagemSetButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (StratagemSetsComboBox.SelectedIndex == -1)
            return;

        StratagemSetsComboBox.Items.RemoveAt(StratagemSetsComboBox.SelectedIndex);

        _settingsChanged = true;
    }

    private bool _isGeneratingVoice;

    private async void GenerateVoiceButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (GenerateVoiceStyleComboBox.SelectedIndex == -1)
            return;

        if (_isGeneratingVoice)
        {
            _isGeneratingVoice = false;
            return;
        }

        _isGeneratingVoice = true;
        GenerateVoiceButton.Content = @"停止生成";

        try
        {
            var count = 0;
            var total = StratagemManager.Count;
            var voiceName = (string)GenerateVoiceStyleComboBox.SelectedItem!;

            foreach (var stratagem in StratagemManager.Stratagems)
            {
                if (!_isGeneratingVoice)
                    break;

                count++;

                await GenerateVoiceFile(stratagem.Name,
                    Path.Combine(VoiceRootPath, Settings.Language, voiceName, stratagem.Name + ".mp3"));
                GenerateVoiceMessageLabel.Content = @$"正在生成民主语音（{count}/{total}）：{stratagem.Name}";
            }
        }
        catch (Exception)
        {
            GenerateVoiceMessageLabel.Content = @"民主语音生成失败...";
        }
        finally
        {
            GenerateVoiceMessageLabel.Content = _isGeneratingVoice
                ? @"民主语音生成完毕！"
                : @"民主语音进程中断...";

            _isGeneratingVoice = false;
            GenerateVoiceButton.Content = @"生成语音";
        }
    }

    private async Task GenerateVoiceFile(string text, string filePath)
    {
        var voiceDir = Path.GetDirectoryName(filePath);
        if (voiceDir == null)
            return;
        if (!Directory.Exists(voiceDir))
            Directory.CreateDirectory(voiceDir);

        var voiceName = (string)GenerateVoiceStyleComboBox.SelectedItem!;
        var communicate = new Communicate(text, voiceName, VoiceRateTextBox.Text, VoiceVolumeTextBox.Text, VoicePitchTextBox.Text);
        await communicate.Save(filePath);
    }

    private async void TryVoiceButton_OnClick(object sender, RoutedEventArgs e)
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
            GenerateVoiceMessageLabel.Content = @"民主语音生成失败...";
        }
    }

    private async void GenerateVoiceStyleComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoading)
            return;

        await TryVoice();
    }

    private async void CalibrateVoiceButton_Click(object sender, RoutedEventArgs e)
    {
        if (_voiceCommand == null)
        {
            await new ContentDialog
            {
                Title = "提示",
                Content = new Label { Content = "请先开启语音触发功能" },
                PrimaryButtonText = "确定",
            }.ShowAsync();
            return;
        }

        var scores = new float[3];
        var messages = new[]
        {
            $"请朗读 “{Settings.WakeupWord}飞鹰空袭”",
            $"请朗读 “{Settings.WakeupWord}轨道炮攻击”",
            $"请朗读 “{Settings.WakeupWord}消耗性反坦克武器”",
        };
        var times = 0;
        var page = new CalibrateVoiceDialog();
        var dialog = new ContentDialog
        {
            Title = "校准语音识别",
            Content = page,
            PrimaryButtonText = "关闭",
        };

        _voiceCommand.CommandRecognized += TestEvent;

        try
        {
            var currentTime = times;
            _ = dialog.ShowAsync();

            while (dialog.IsVisible)
            {
                page.SetMessage(messages[currentTime]);

                while (times == currentTime && dialog.IsVisible)
                    await Task.Delay(100);
                currentTime = times;
                if (currentTime < 3)
                    continue;

                dialog.Hide();
                SpeechConfidenceNumberBox.Value = scores.Average() - (scores.Max() - scores.Min());
                break;
            }
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

    private void StratagemSetsComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = (string?)StratagemSetsComboBox.SelectedItem;
        if (selectedItem == null)
            return;

        SetKeyStratagemString(selectedItem);
        _keySettingsChanged = true;
    }

    private void PlayVoiceCheckBox_OnCheckedUnchecked(object sender, RoutedEventArgs e)
    {
        if (IsLoading)
            return;

        Settings.PlayVoice = PlayVoiceCheckBox.IsChecked == true;
        _keySettingsChanged = true;
    }

    private void MicComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoading)
            return;

        _voiceCommand?.UseMic(MicComboBox.SelectedItem as string ?? "");
    }

    private void MicComboBox_OnDropDownOpened(object? sender, EventArgs e)
    {
        LoadMicDevices(MicComboBox.Text);
    }
}
