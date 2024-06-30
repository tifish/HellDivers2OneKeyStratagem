using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdgeTTS;
using GlobalHotKeys;
using NAudio.Wave;

namespace HellDivers2OneKeyStratagem;

public partial class MainViewModel : ObservableObject
{
    public static MainViewModel Instance { get; } = new();

    private MainWindow _mainWindow = null!;

    public void SetMainWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    private int _isLoadingCounter;

    public bool IsLoading
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

    private bool _settingsChanged;
    private bool _keySettingsChanged;
    private bool _speechSettingsChanged;

    [ObservableProperty]
    private double _speechConfidence;

    partial void OnSpeechConfidenceChanged(double value)
    {
        if (IsLoading)
            return;

        Settings.VoiceConfidence = Math.Round(value, 3);
        _settingsChanged = true;
    }

    [ObservableProperty]
    private ObservableCollection<string> _locales = [];

    [ObservableProperty]
    private string _currentLocale = "";

    async partial void OnCurrentLocaleChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.Locale = value;
        if (Settings.PlayVoice)
            _keySettingsChanged = true;
        _speechSettingsChanged = true;

        await LoadByLanguage();
    }

    [ObservableProperty]
    private string _triggerKey = "";

    partial void OnTriggerKeyChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.TriggerKey = value;
        _keySettingsChanged = true;
    }

    [ObservableProperty]
    private string _operateKeys = "";

    partial void OnOperateKeysChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.OperateKeys = value;
        _keySettingsChanged = true;
    }

    [ObservableProperty]
    private bool _playVoiceWhenCall;

    partial void OnPlayVoiceWhenCallChanged(bool value)
    {
        if (IsLoading)
            return;

        Settings.PlayVoice = value;
        _keySettingsChanged = true;
    }

    [ObservableProperty]
    private ObservableCollection<string> _voiceNames = [];

    [ObservableProperty]
    private string _currentVoiceName = "";

    partial void OnCurrentVoiceNameChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.VoiceName = value;
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

    [RelayCommand]
    private async Task Load()
    {
        IsLoading = true;

        try
        {
            await AppSettings.LoadSettings();

            InitLanguages();
            InitHotkeysUI();

            await LoadByLanguage();
        }
        finally
        {
            IsLoading = false;
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

        Locales.Clear();
        foreach (var locale in speechLocales)
            Locales.Add(locale);

        if (!speechLocales.Contains(Settings.Locale))
        {
            if (speechLocales.Contains(CultureInfo.CurrentCulture.Name))
                Settings.Locale = CultureInfo.CurrentCulture.Name;
            else
                Settings.Locale = speechLocales.First();

            _keySettingsChanged = true;
            _speechSettingsChanged = true;
        }

        CurrentLocale = Settings.Locale;
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

            _mainWindow.BeginInit();

            try
            {
                if (_selectedKeyIndex >= 0)
                    _keyBorders[SelectedKeyIndex].BorderBrush = Brushes.Transparent;
                _keyBorders[value].BorderBrush = Brushes.Gray;
                _selectedKeyIndex = value;
            }
            finally
            {
                _mainWindow.EndInit();
            }
        }
    }

    private const string NoStratagem = "无";

    private void InitHotkeysUI()
    {
        _mainWindow.KeysStackPanel1.Children.Clear();
        _mainWindow.KeysStackPanel2.Children.Clear();
        var keysStackPanel = _mainWindow.KeysStackPanel1;

        for (var i = 0; i < KeyCount; i++)
        {
            if (i == 11)
                keysStackPanel = _mainWindow.KeysStackPanel2;

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

    public async Task LoadByLanguage()
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
        _mainWindow.StratagemGroupsContainer.Children.Clear();

        foreach (var (groupName, stratagems) in StratagemManager.Groups)
        {
            var border = new Border
            {
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(1, 1, 1, 1),
                Width = double.NaN,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            border.Bind(Border.BackgroundProperty, _mainWindow.Resources.GetResourceObservable("SystemControlBackgroundListLowBrush"));
            _mainWindow.StratagemGroupsContainer.Children.Add(border);

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

                    if (await dialog.ShowDialog<bool>(_mainWindow))
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
        TriggerKey = Settings.TriggerKey;
        OperateKeys = Settings.OperateKeys;

        PlayVoiceWhenCall = Settings.PlayVoice;

        if (Settings.StratagemSets.Count > 0)
        {
            SetKeyStratagemString(Settings.StratagemSets[0]);

            StratagemSets.Clear();
            foreach (var stratagemSet in Settings.StratagemSets.Skip(1))
                StratagemSets.Add(stratagemSet);
        }

        SpeechConfidence = Math.Round(Settings.VoiceConfidence, 3);
        WakeupWord = Settings.WakeupWord;

        EnableSpeechTrigger = Settings.EnableSpeechTrigger;

        EnableHotkeyTrigger = Settings.EnableHotkeyTrigger;

        EnableSetKeyBySpeech = Settings.EnableSetKeyBySpeech;

        UpdateUrl = Settings.UpdateUrl;
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

            GenerateVoiceStyles.Clear();
            foreach (var voice in _voices)
                GenerateVoiceStyles.Add(voice.ShortName);

            CurrentGenerateVoiceStyleIndex = GenerateVoiceStyles.Count - 1;
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

            VoiceNames.Clear();
            foreach (var style in styles)
                VoiceNames.Add(style!);

            CurrentVoiceName =
                styles.Contains(Settings.VoiceName)
                    ? Settings.VoiceName
                    : styles.FirstOrDefault() ?? "";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadMicDevices(string lastSelected)
    {
        Mics.Clear();

        for (var i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var device = WaveInEvent.GetCapabilities(i);
            Mics.Add(device.ProductName);
        }

        CurrentMic = Mics.Contains(lastSelected)
            ? lastSelected
            : "";
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

    [RelayCommand]
    private async Task CheckAndSaveSettings()
    {
        if (IsLoading)
            return;

        if (!_settingsChanged)
            return;

        Settings.StratagemSets.Clear();
        Settings.StratagemSets.Add(GetKeyStratagemString());
        foreach (var item in StratagemSets)
            Settings.StratagemSets.Add(item);
        await AppSettings.SaveSettings();

        _settingsChanged = false;
    }

    private async Task ResetVoiceCommand()
    {
        await StopSpeechTrigger();

        if (Settings.EnableSpeechTrigger)
            await StartSpeechTrigger();
    }

    [ObservableProperty]
    private string _voiceRecognizeResult = "暂无识别结果";

    private VoiceCommand? _voiceCommand;

    private async Task StartSpeechTrigger()
    {
        if (_voiceCommand == null)
        {
            var languages = VoiceCommand.GetInstalledRecognizers();
            if (languages.Count == 0)
            {
                await new MessageDialog("错误", "没有安装语音识别引擎").ShowDialog(_mainWindow);
                return;
            }

            if (!languages.Contains(Settings.Locale))
            {
                await new MessageDialog("错误", $"没有安装 {Settings.Locale} 的语音识别引擎").ShowDialog(_mainWindow);
                return;
            }

            try
            {
                _voiceCommand = new VoiceCommand(Settings.Locale, Settings.WakeupWord, [.. StratagemManager.StratagemAlias]);
            }
            catch (Exception)
            {
                await new MessageDialog("错误", "创建语音识别失败").ShowDialog(_mainWindow);
                return;
            }

            _voiceCommand.CommandRecognized += (_, command) =>
            {
                var failed = command.Score < Settings.VoiceConfidence;
                var info = $"【{(failed ? "失败" : "成功")}】识别阈值：{command.Score:F3} 识别文字：{command.Text}";
                VoiceRecognizeResult = info;
                _infoWindow?.SetInfo(info);

                if (failed)
                    return;

                if (!StratagemManager.TryGet(command.Text, out var stratagem))
                    return;

                if (ActiveWindowMonitor.CurrentWindowTitle == _mainWindow.Title && Settings is { EnableHotkeyTrigger: true, EnableSetKeyBySpeech: true })
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

            if (!string.IsNullOrEmpty(CurrentMic))
                await _voiceCommand.UseMic(CurrentMic);
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

    [RelayCommand]
    private void RefreshVoiceNames()
    {
        Settings.VoiceName = CurrentVoiceName;
        LoadVoiceNames();
    }

    [ObservableProperty]
    private string _updateUrl = "";

    partial void OnUpdateUrlChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.UpdateUrl = value;
        _settingsChanged = true;
    }


    [ObservableProperty]
    private bool _isCheckingForUpdate;

    [RelayCommand]
    private async Task CheckForUpdate()
    {
        IsCheckingForUpdate = true;

        try
        {
            if (await AutoUpdate.HasUpdate())
            {
                if (await new YesNoDialog("提示", "发现新版本，是否更新？").ShowDialog<bool>(_mainWindow))
                    AutoUpdate.Update();
            }
            else
            {
                await new MessageDialog("提示", "已经是最新版本").ShowDialog<bool>(_mainWindow);
            }
        }
        finally
        {
            IsCheckingForUpdate = false;
        }
    }

    [ObservableProperty]
    private bool _enableSpeechTrigger;

    async partial void OnEnableSpeechTriggerChanged(bool value)
    {
        if (IsLoading)
            return;

        Settings.EnableSpeechTrigger = value;
        _settingsChanged = true;

        if (Settings.EnableSpeechTrigger)
            await StartSpeechTrigger();
        else
            await StopSpeechTrigger();
    }

    [RelayCommand]
    private void OpenSpeechRecognitionControlPanel()
    {
        Process.Start("control.exe", "/name Microsoft.SpeechRecognition");
    }

    [ObservableProperty]
    private string _wakeupWord = "";

    async partial void OnWakeupWordChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.WakeupWord = value.Trim();
        _settingsChanged = true;
        await ResetVoiceCommand();
    }

    [ObservableProperty]
    private bool _isGenerateVoicePanelVisible;

    [RelayCommand]
    private void ToggleGenerateVoicePanel()
    {
        IsGenerateVoicePanelVisible = !IsGenerateVoicePanelVisible;
    }

    [ObservableProperty]
    private bool _enableHotkeyTrigger;

    partial void OnEnableHotkeyTriggerChanged(bool value)
    {
        if (IsLoading)
            return;

        Settings.EnableHotkeyTrigger = value;
        _settingsChanged = true;

        HotkeyGroupManager.Enabled = Settings.EnableHotkeyTrigger;
    }

    [ObservableProperty]
    private bool _enableSetKeyBySpeech;

    partial void OnEnableSetKeyBySpeechChanged(bool value)
    {
        if (IsLoading)
            return;

        Settings.EnableSetKeyBySpeech = value;
        _settingsChanged = true;
    }

    [RelayCommand]
    private void SaveStratagemSet()
    {
        var keyStratagemString = GetKeyStratagemString();
        if (StratagemSets.Contains(keyStratagemString))
            return;

        StratagemSets.Add(keyStratagemString);
        CurrentStratagemSetIndex = StratagemSets.Count - 1;

        _settingsChanged = true;
    }

    [RelayCommand]
    private void DeleteStratagemSet()
    {
        if (CurrentStratagemSetIndex == -1)
            return;

        StratagemSets.RemoveAt(CurrentStratagemSetIndex);

        _settingsChanged = true;
    }

    [ObservableProperty]
    private string _generateVoiceButtonContent = "生成语音";

    private bool _isGeneratingVoice;

    [RelayCommand]
    private async Task GenerateVoice()
    {
        if (CurrentGenerateVoiceStyleIndex == -1)
            return;

        if (_isGeneratingVoice)
        {
            _isGeneratingVoice = false;
            return;
        }

        _isGeneratingVoice = true;
        GenerateVoiceButtonContent = "停止生成";

        try
        {
            var count = 0;
            var total = StratagemManager.Count;
            var voiceName = CurrentGenerateVoiceStyle;

            foreach (var stratagem in StratagemManager.Stratagems)
            {
                if (!_isGeneratingVoice)
                    break;

                count++;

                await GenerateVoiceFile(stratagem.Name,
                    Path.Combine(VoiceRootPath, Settings.Language, voiceName, stratagem.Name + ".mp3"));
                GenerateVoiceMessage = $"正在生成民主语音（{count}/{total}）：{stratagem.Name}";
            }
        }
        catch (Exception)
        {
            GenerateVoiceMessage = "民主语音生成失败...";
        }
        finally
        {
            GenerateVoiceMessage = _isGeneratingVoice
                ? "民主语音生成完毕！"
                : "民主语音进程中断...";

            _isGeneratingVoice = false;
            GenerateVoiceButtonContent = "生成语音";
        }
    }

    [ObservableProperty]
    private string _generateVoiceRate = "+0%";

    [ObservableProperty]
    private string _generateVoiceVolume = "+0%";

    [ObservableProperty]
    private string _generateVoicePitch = "+0Hz";

    private async Task GenerateVoiceFile(string text, string filePath)
    {
        var voiceDir = Path.GetDirectoryName(filePath);
        if (voiceDir == null)
            return;
        if (!Directory.Exists(voiceDir))
            Directory.CreateDirectory(voiceDir);

        var communicate = new Communicate(text, CurrentGenerateVoiceStyle,
            GenerateVoiceRate, GenerateVoiceVolume, GenerateVoicePitch);
        await communicate.Save(filePath);
    }

    [RelayCommand]
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
            GenerateVoiceMessage = "民主语音生成失败...";
        }
    }

    [ObservableProperty]
    private ObservableCollection<string> _generateVoiceStyles = [];

    [ObservableProperty]
    private string _currentGenerateVoiceStyle = "";

    async partial void OnCurrentGenerateVoiceStyleChanged(string value)
    {
        if (IsLoading)
            return;

        await TryVoice();
    }

    [ObservableProperty]
    private int _currentGenerateVoiceStyleIndex;

    [RelayCommand]
    private async Task CalibrateVoice()
    {
        if (_voiceCommand == null)
        {
            await new MessageDialog("提示", "请先开启语音触发功能").ShowDialog(_mainWindow);
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
            _ = dialog.ShowDialog(_mainWindow);

            while (dialog.IsVisible)
            {
                dialog.SetMessage(messages[currentTime]);

                while (times == currentTime && dialog.IsVisible)
                    await Task.Delay(100);
                currentTime = times;
                if (currentTime < 3)
                    continue;

                dialog.Hide();
                SpeechConfidence = Math.Round(scores.Average() - (scores.Max() - scores.Min()), 3);
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

    [ObservableProperty]
    private ObservableCollection<string> _stratagemSets = [];

    [ObservableProperty]
    private int _currentStratagemSetIndex;

    partial void OnCurrentStratagemSetIndexChanged(int value)
    {
        if (value < 0 || value >= StratagemSets.Count)
            return;

        SetKeyStratagemString(StratagemSets[value]);
        _keySettingsChanged = true;
    }

    [ObservableProperty]
    private ObservableCollection<string> _mics = [];

    [ObservableProperty]
    private string _currentMic = "";

    partial void OnCurrentMicChanged(string value)
    {
        if (IsLoading)
            return;

        _voiceCommand?.UseMic(value);
    }

    [RelayCommand]
    private void LoadMics()
    {
        LoadMicDevices(CurrentMic);
    }

    [ObservableProperty]
    private string _generateVoiceMessage = "未生成";

    [RelayCommand]
    private void GenerateTxt()
    {
        var folder = "VoiceTxt";
        Directory.CreateDirectory(folder);
        foreach (var stratagem in StratagemManager.Stratagems)
            File.WriteAllText(Path.Combine(folder, stratagem.Name), stratagem.Name);
        GenerateVoiceMessage = @"txt 生成完毕";
    }

    private InfoWindow? _infoWindow;

    private void ShowInfoWindow()
    {
        if (_infoWindow != null)
            return;

        _infoWindow = new InfoWindow(_mainWindow);
        _infoWindow.Show();
    }

    private void HideInfoWindow()
    {
        _infoWindow?.Close();
        _infoWindow = null;
    }

    [ObservableProperty]
    private bool _showSpeechInfoWindow;

    partial void OnShowSpeechInfoWindowChanged(bool value)
    {
        if (value)
            ShowInfoWindow();
        else
            HideInfoWindow();
    }

    [RelayCommand]
    private async Task Cleanup()
    {
        HideInfoWindow();
        HotkeyGroupManager.ClearHotkeyGroup();
        await StopSpeechTrigger();
    }
}
