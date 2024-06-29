using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using EdgeTTS;
using GlobalHotKeys;
using NAudio.Wave;

namespace HellDivers2OneKeyStratagem;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = MainViewModel.Instance;

        M.IsLoading = true;

        try
        {
            InitializeComponent();
        }
        finally
        {
            M.IsLoading = false;
        }
    }

    private MainViewModel M => (MainViewModel)DataContext!;

    private async void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        M.IsLoading = true;

        try
        {
            await AppSettings.LoadSettings();

            InitLanguages();
            InitHotkeysUI();

            await LoadByLanguage();
        }
        finally
        {
            M.IsLoading = false;
        }

        ActiveWindowMonitor.WindowTitleChanged += OnWindowTitleChanged;
        ActiveWindowMonitor.Start(TimeSpan.FromSeconds(1));
    }

    private const string HellDivers2Title = "HELLDIVERS™ 2";

    private async void OnWindowTitleChanged(object? sender, WindowTitleChangedEventArgs e)
    {
        var oldProcessIsActive = e.OldWindowTitle == HellDivers2Title;
        var newProcessIsActive = e.NewWindowTitle == HellDivers2Title || ActiveWindowMonitor.CurrentProcessFileName == AppSettings.ExeFileName;

        if (oldProcessIsActive && !newProcessIsActive)
        {
            await StopSpeechTrigger();
        }
        else if (!oldProcessIsActive && newProcessIsActive)
        {
            if (Settings.EnableSpeechTrigger)
                await StartSpeechTrigger();
        }
        else if (newProcessIsActive && M.SpeechSettingsChanged)
        {
            M.SpeechSettingsChanged = false;
            await ResetVoiceCommand();
        }

        if (e.OldWindowTitle == HellDivers2Title)
        {
            HotkeyGroupManager.Enabled = false;
        }
        else if (e.NewWindowTitle == HellDivers2Title && Settings.EnableHotkeyTrigger)
        {
            if (M.KeySettingsChanged)
            {
                M.KeySettingsChanged = false;
                SetHotkeyGroup();
            }

            HotkeyGroupManager.Enabled = true;
        }
    }

    private void SetHotkeyGroup()
    {
        var hotkeys = new Dictionary<Key, EventHandler<HotKey>>();
        for (var i = 0; i < _keyStratagems.Length; i++)
        {
            var stratagem = _keyStratagems[i];
            if (stratagem == null)
                continue;

            hotkeys[_keys[i]] = (_, e) =>
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

            M.KeySettingsChanged = true;
            M.SpeechSettingsChanged = true;
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

    private void InitHotkeysUI()
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
            stackPanel.PointerPressed += (_, e) =>
            {
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
        StratagemGroupsContainer.Children.Clear();

        foreach (var (groupName, stratagems) in StratagemManager.Groups)
        {
            var border = new Border
            {
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(1, 1, 1, 1),
                Width = double.NaN,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            border.Bind(Border.BackgroundProperty, Resources.GetResourceObservable("SystemControlBackgroundListLowBrush"));
            StratagemGroupsContainer.Children.Add(border);

            var groupContainer = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Vertical,
            };
            border.Child = groupContainer;

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

                ToolTip.SetTip(stratagemCheckBox, $"""
                                                   {StratagemManager.GetSystemAlias(stratagem.Name)}
                                                       自定义名称：{StratagemManager.GetUserAlias(stratagem.Name)}
                                                       按右键编辑自定义名称。
                                                   """);

                stratagemCheckBox.IsCheckedChanged += StratagemCheckBoxOnIsCheckedChanged;

                stratagemCheckBox.PointerPressed += async (_, args) =>
                {
                    if (args.GetCurrentPoint(null).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed)
                        return;

                    var dialog = new EditAliasesDialog((string)stratagemCheckBox.Content);

                    if (await dialog.ShowDialog<bool>(this))
                    {
                        dialog.Commit();
                        await ResetVoiceCommand();
                    }
                };

                continue;

                void StratagemCheckBoxOnIsCheckedChanged(object? o, RoutedEventArgs routedEventArgs)
                {
                    if (_isSettingStratagemCheckBoxChecked)
                        return;

                    if (!Settings.EnableHotkeyTrigger)
                    {
                        _isSettingStratagemCheckBoxChecked = true;
                        stratagemCheckBox.IsChecked = !stratagemCheckBox.IsChecked;
                        _isSettingStratagemCheckBoxChecked = false;
                        return;
                    }

                    if (stratagemCheckBox.IsChecked == true)
                    {
                        SetKeyStratagem(SelectedKeyIndex, stratagem);

                        if (SelectedKeyIndex > 0)
                            if (_keyLabels[SelectedKeyIndex - 1].Content as string == NoStratagem)
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

        M.SpeechConfidence = Math.Round(Settings.VoiceConfidence, 3);
        WakeupWordTextBox.Text = Settings.WakeupWord;

        EnableSpeechTriggerCheckBox.IsChecked = Settings.EnableSpeechTrigger;
        RefreshAfterEnableSpeechTriggerChanged();

        EnableHotkeyTriggerCheckBox.IsChecked = Settings.EnableHotkeyTrigger;
        RefreshAfterEnableHotkeyTriggerChanged();

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

        M.IsLoading = true;

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
            M.IsLoading = false;
        }
    }

    private void LoadVoiceNames()
    {
        M.IsLoading = true;

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
            M.IsLoading = false;
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

        if (!M.IsLoading)
            M.KeySettingsChanged = true;
    }

    private static readonly string VoiceRootPath = Path.Combine(AppSettings.ExeDirectory, "Voice");

    private void PlayStratagemVoice(string stratagemName)
    {
        PlayVoice(Path.Combine(VoiceRootPath, Settings.Language, Settings.VoiceName, stratagemName + ".mp3"));
    }

    private void PlayVoice(string filePath, bool deleteAfterPlay = false)
    {
        if (M.IsLoading)
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

    private async void Window_Deactivated(object sender, EventArgs e)
    {
        if (M.IsLoading)
            return;

        if (!M.SettingsChanged)
            return;

        Settings.StratagemSets.Clear();
        Settings.StratagemSets.Add(GetKeyStratagemString());
        foreach (var item in StratagemSetsComboBox.Items)
            Settings.StratagemSets.Add(item as string ?? "");
        await AppSettings.SaveSettings();

        M.SettingsChanged = false;
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
                await new MessageDialog("错误", "没有安装语音识别引擎").ShowDialog(this);
                return;
            }

            if (!languages.Contains(Settings.Locale))
            {
                await new MessageDialog("错误", $"没有安装 {Settings.Locale} 的语音识别引擎").ShowDialog(this);
                return;
            }

            try
            {
                _voiceCommand = new VoiceCommand(Settings.Locale, Settings.WakeupWord, [.. StratagemManager.StratagemAlias]);
            }
            catch (Exception)
            {
                await new MessageDialog("错误", "创建语音识别失败").ShowDialog(this);
                return;
            }

            _voiceCommand.CommandRecognized += (_, command) =>
            {
                var failed = command.Score < Settings.VoiceConfidence;
                var info = $"【{(failed ? "失败" : "成功")}】识别阈值：{command.Score:F3} 识别文字：{command.Text}";
                VoiceRecognizeResultLabel.Content = info;
                _infoWindow?.SetInfo(info);

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

            if (MicComboBox.SelectedValue is string mic && mic != "")
                await _voiceCommand.UseMic(mic);
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
        if (M.IsLoading)
            return;

        if (LocaleComboBox.SelectedItem is string locale)
        {
            Settings.Locale = locale;
            if (Settings.PlayVoice)
                M.KeySettingsChanged = true;
            M.SpeechSettingsChanged = true;

            await LoadByLanguage();
        }
    }

    private void VoiceNamesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (M.IsLoading)
            return;

        Settings.VoiceName = VoiceNamesComboBox.SelectedItem as string ?? "";
        if (Settings.PlayVoice)
            M.KeySettingsChanged = true;
        else
            M.SettingsChanged = true;

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

    private void CtrlAltRadioButton_OnIsCheckedChanged(object? sender, RoutedEventArgs routedEventArgs)
    {
        if (M.IsLoading)
            return;

        Settings.TriggerKey = CtrlRadioButton.IsChecked!.Value ? "Ctrl" : "Alt";
        M.KeySettingsChanged = true;
    }

    private void WasdArrowRadioButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (M.IsLoading)
            return;

        Settings.OperateKeys = WasdRadioButton.IsChecked!.Value ? "WASD" : "Arrow";
        M.KeySettingsChanged = true;
    }

    private void UpdateUrlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (M.IsLoading)
            return;

        Settings.UpdateUrl = UpdateUrlTextBox.Text ?? "";
        M.SettingsChanged = true;
    }

    private async void CheckForUpdateButton_OnClick(object sender, RoutedEventArgs e)
    {
        CheckForUpdateButton.IsEnabled = false;

        try
        {
            if (await AutoUpdate.HasUpdate())
            {
                if (await new YesNoDialog("提示", "发现新版本，是否更新？").ShowDialog<bool>(this))
                    AutoUpdate.Update();
            }
            else
            {
                await new MessageDialog("提示", "已经是最新版本").ShowDialog<bool>(this);
            }
        }
        finally
        {
            CheckForUpdateButton.IsEnabled = true;
        }
    }

    private async void EnableSpeechTriggerCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (M.IsLoading)
            return;

        Settings.EnableSpeechTrigger = EnableSpeechTriggerCheckBox.IsChecked == true;
        M.SettingsChanged = true;

        if (Settings.EnableSpeechTrigger)
            await StartSpeechTrigger();
        else
            await StopSpeechTrigger();

        RefreshAfterEnableSpeechTriggerChanged();
    }

    private void RefreshAfterEnableSpeechTriggerChanged()
    {
        SpeechSubSettingsContainer.IsVisible = Settings.EnableSpeechTrigger;
        EnableSetKeyBySpeechCheckBox.IsEnabled = Settings.EnableSpeechTrigger;
        StratagemsContainer.IsVisible = Settings.EnableSpeechTrigger || Settings.EnableHotkeyTrigger;
        ShowSpeechInfoWindowCheckBoxContainer.IsVisible = Settings.EnableSpeechTrigger;
    }

    private void OpenSpeechRecognitionControlPanelButton_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("control.exe", "/name Microsoft.SpeechRecognition");
    }

    private async void WakeupWordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (M.IsLoading)
            return;

        Settings.WakeupWord = WakeupWordTextBox.Text?.Trim() ?? "";
        M.SettingsChanged = true;
        await ResetVoiceCommand();
    }

    private void MicLabel_OnMouseDoubleClick(object? sender, TappedEventArgs tappedEventArgs)
    {
        GenerateVoiceContainer.IsVisible = !GenerateVoiceContainer.IsVisible;
    }

    private void EnableHotkeyTriggerCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (M.IsLoading)
            return;

        Settings.EnableHotkeyTrigger = EnableHotkeyTriggerCheckBox.IsChecked == true;
        M.SettingsChanged = true;

        HotkeyGroupManager.Enabled = Settings.EnableHotkeyTrigger;

        RefreshAfterEnableHotkeyTriggerChanged();
    }

    private void RefreshAfterEnableHotkeyTriggerChanged()
    {
        EnableSetKeyBySpeechCheckBox.IsVisible = Settings.EnableHotkeyTrigger;
        HotKeysStackPanel.IsVisible = Settings.EnableHotkeyTrigger;
        StratagemsContainer.IsVisible = Settings.EnableSpeechTrigger || Settings.EnableHotkeyTrigger;
    }

    private void EnableSetKeyBySpeechCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (M.IsLoading)
            return;

        Settings.EnableSetKeyBySpeech = EnableSetKeyBySpeechCheckBox.IsChecked == true;
        M.SettingsChanged = true;
    }

    private void SaveStratagemSetButton_OnClick(object sender, RoutedEventArgs e)
    {
        var keyStratagemString = GetKeyStratagemString();
        if (StratagemSetsComboBox.Items.Contains(keyStratagemString))
            return;

        StratagemSetsComboBox.Items.Add(keyStratagemString);
        StratagemSetsComboBox.SelectedIndex = StratagemSetsComboBox.Items.Count - 1;

        M.SettingsChanged = true;
    }

    private void DeleteStratagemSetButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (StratagemSetsComboBox.SelectedIndex == -1)
            return;

        StratagemSetsComboBox.Items.RemoveAt(StratagemSetsComboBox.SelectedIndex);

        M.SettingsChanged = true;
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
        var communicate = new Communicate(text, voiceName, VoiceRateTextBox.Text!, VoiceVolumeTextBox.Text!, VoicePitchTextBox.Text!);
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
        if (M.IsLoading)
            return;

        await TryVoice();
    }

    private async void CalibrateVoiceButton_Click(object sender, RoutedEventArgs e)
    {
        if (_voiceCommand == null)
        {
            await new MessageDialog("提示", "请先开启语音触发功能").ShowDialog(this);
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
        var dialog = new CalibrateVoiceDialog();

        _voiceCommand.CommandRecognized += TestEvent;

        try
        {
            var currentTime = times;
            _ = dialog.ShowDialog(this);

            while (dialog.IsVisible)
            {
                dialog.SetMessage(messages[currentTime]);

                while (times == currentTime && dialog.IsVisible)
                    await Task.Delay(100);
                currentTime = times;
                if (currentTime < 3)
                    continue;

                dialog.Hide();
                M.SpeechConfidence = Math.Round(scores.Average() - (scores.Max() - scores.Min()), 3);
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
        M.KeySettingsChanged = true;
    }

    private void PlayVoiceCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (M.IsLoading)
            return;

        Settings.PlayVoice = PlayVoiceCheckBox.IsChecked == true;
        M.KeySettingsChanged = true;
    }

    private void MicComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (M.IsLoading)
            return;

        _voiceCommand?.UseMic(MicComboBox.SelectedItem as string ?? "");
    }

    private void MicComboBox_OnDropDownOpened(object? sender, EventArgs e)
    {
        LoadMicDevices(MicComboBox.SelectedValue as string ?? "");
    }

    private void GenerateTxtButton_Click(object sender, RoutedEventArgs e)
    {
        var folder = "VoiceTxt";
        Directory.CreateDirectory(folder);
        foreach (var stratagem in StratagemManager.Stratagems)
            File.WriteAllText(Path.Combine(folder, stratagem.Name), stratagem.Name);
        GenerateVoiceMessageLabel.Content = @"txt 生成完毕";
    }

    private InfoWindow? _infoWindow;

    private void ShowInfoWindow()
    {
        if (_infoWindow != null)
            return;

        _infoWindow = new InfoWindow(this);
        _infoWindow.Show();
    }

    private void HideInfoWindow()
    {
        _infoWindow?.Close();
        _infoWindow = null;
    }

    public void NotifyInfoWindowClosed()
    {
        _infoWindow = null;
        ShowSpeechInfoWindowCheckBox.IsChecked = false;
    }

    private void ShowSpeechInfoWindowCheckBox_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (ShowSpeechInfoWindowCheckBox.IsChecked == true)
            ShowInfoWindow();
        else
            HideInfoWindow();
    }

    private async void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        HideInfoWindow();
        HotkeyGroupManager.ClearHotkeyGroup();
        await StopSpeechTrigger();
    }

}
