using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdgeTTS;
using GlobalHotKeys;
using Jeek.Avalonia.Localization;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace HellDivers2OneKeyStratagem;

public partial class MainViewModel : ObservableObject
{
    public static MainViewModel Instance { get; } = new();

    private static readonly ILogger _logger = LogFactory.CreateLogger<MainViewModel>();

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

    private bool SettingsChanged { get; set; }

    private bool _keySettingsChanged;

    private bool KeySettingsChanged
    {
        get => _keySettingsChanged;
        set
        {
            _keySettingsChanged = value;
            if (value)
                SettingsChanged = true;
        }
    }

    private bool _speechSettingsChanged;

    private bool SpeechSettingsChanged
    {
        get => _speechSettingsChanged;
        set
        {
            _speechSettingsChanged = value;
            if (value)
                SettingsChanged = true;
        }
    }

    [ObservableProperty]
    private double _speechConfidence;

    partial void OnSpeechConfidenceChanged(double value)
    {
        if (IsLoading)
            return;

        Settings.VoiceConfidence = Math.Round(value, 3);
        SettingsChanged = true;
    }

    [ObservableProperty]
    private ObservableCollection<string> _speechLocales = [];

    [ObservableProperty]
    private string _currentSpeechLocale = "";

    partial void OnCurrentSpeechLocaleChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.SpeechLocale = value;
        if (Settings.PlayVoice)
            KeySettingsChanged = true;
        SpeechSettingsChanged = true;

        LoadBySpeechLanguage();
    }

    [ObservableProperty]
    private ObservableCollection<string> _triggerKeys = ["LeftCtrl", "LeftAlt", "Q"];

    [ObservableProperty]
    private string _triggerKey = "";

    partial void OnTriggerKeyChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.TriggerKey = value;
        KeySettingsChanged = true;
    }

    [ObservableProperty]
    private string _operateKeys = "";

    partial void OnOperateKeysChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.OperateKeys = value;
        KeySettingsChanged = true;
    }

    [ObservableProperty]
    private bool _playVoiceWhenCall;

    partial void OnPlayVoiceWhenCallChanged(bool value)
    {
        if (IsLoading)
            return;

        Settings.PlayVoice = value;
        KeySettingsChanged = true;
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
            KeySettingsChanged = true;
        else
            SettingsChanged = true;

        // The stratagem may be invalid after changing the language
        var stratagem = _keyStratagems[SelectedKeyIndex];
        var stratagemName = stratagem != null && StratagemManager.Stratagems.Contains(stratagem)
            ? stratagem.Name
            : StratagemManager.Stratagems.Last().Name;
        PlayStratagemVoice(stratagemName);
    }

    [RelayCommand]
    private async Task WindowOpened()
    {
        IsLoading = true;

        try
        {
            if (!KillOtherInstances())
            {
                await new MessageDialog(
                         Localizer.Get("Error"),
                         Localizer.Get("FailedToKillOtherInstances"))
                     .ShowDialog(_mainWindow);
                _mainWindow.Close();
                return;
            }

            InitUILanguages();
            InitSpeechLanguages();
            InitHotkeysUI();

            LoadBySpeechLanguage();
        }
        finally
        {
            IsLoading = false;
        }

        ActiveWindowMonitor.WindowTitleChanged += OnWindowTitleChanged;
        ActiveWindowMonitor.Start(TimeSpan.FromSeconds(1));
    }

    private static bool KillOtherInstances()
    {
        try
        {
            string currentProcessName = Process.GetCurrentProcess().ProcessName;
            int currentPid = Environment.ProcessId;

            foreach (var process in Process.GetProcessesByName(currentProcessName))
            {
                if (process.Id != currentPid)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        // 记录错误但继续执行
                        _logger.LogError("Kill process {ProcessId} error: {Message}", process.Id, ex.Message);
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 如果进程检查过程出错，记录错误但允许程序继续运行
            _logger.LogError("Check other instances error: {Message}", ex.Message);
            return false;
        }

        return true;
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
        else if (newProcessIsActive && SpeechSettingsChanged)
        {
            SpeechSettingsChanged = false;
            await ResetVoiceCommand();
        }

        if (e.OldWindowTitle == HellDivers2Title)
        {
            HotkeyGroupManager.Enabled = false;
        }
        else if (e.NewWindowTitle == HellDivers2Title && Settings.EnableHotkeyTrigger)
        {
            if (KeySettingsChanged)
            {
                KeySettingsChanged = false;
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

            hotkeys[_keys[i]] = (_, _) =>
            {
                if (Settings.PlayVoice)
                    PlayStratagemVoice(stratagem.Name);

                stratagem.PressKeys();
            };
        }

        HotkeyGroupManager.SetHotkeyGroup(hotkeys);
    }

    [ObservableProperty]
    private ObservableCollection<string> _locales = [];

    [ObservableProperty]
    private string _currentLocale = "";

    partial void OnCurrentLocaleChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.Locale = value;
        SettingsChanged = true;

        Localizer.Language = value;
    }

    private void InitUILanguages()
    {
        Locales.Clear();
        foreach (var locale in Localizer.Languages)
            Locales.Add(locale);

        if (!Locales.Contains(Settings.Locale))
            Settings.Locale = Locales.First();

        CurrentLocale = Settings.Locale;
    }

    private void InitSpeechLanguages()
    {
        var speechLocales = VoiceCommand.GetInstalledRecognizers();
        if (speechLocales.Count == 0)
            return;

        SpeechLocales.Clear();
        foreach (var speechLocale in speechLocales)
            SpeechLocales.Add(speechLocale);

        if (!speechLocales.Contains(Settings.SpeechLocale))
        {
            if (speechLocales.Contains(CultureInfo.CurrentCulture.Name))
                Settings.SpeechLocale = CultureInfo.CurrentCulture.Name;
            else
                Settings.SpeechLocale = speechLocales.First();

            KeySettingsChanged = true;
            SpeechSettingsChanged = true;
        }

        CurrentSpeechLocale = Settings.SpeechLocale;
    }

    private const int KeyCount = 17;

    private readonly Key[] _keys =
    [
        Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10, Key.F11,
        Key.Insert, Key.Delete, Key.Home, Key.End, Key.PageUp, Key.PageDown,
    ];

    private readonly HotkeyStratagemPanel[] _hotkeyPanels = new HotkeyStratagemPanel[KeyCount];
    private readonly Stratagem?[] _keyStratagems = new Stratagem?[KeyCount];

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
                    _hotkeyPanels[SelectedKeyIndex].IsBorderVisible = false;
                _hotkeyPanels[value].IsBorderVisible = true;
                _selectedKeyIndex = value;
            }
            finally
            {
                _mainWindow.EndInit();
            }
        }
    }

    private void InitHotkeysUI()
    {
        _mainWindow.KeysStackPanel1.Children.Clear();
        _mainWindow.KeysStackPanel2.Children.Clear();
        var keysStackPanel = _mainWindow.KeysStackPanel1;

        for (var i = 0; i < KeyCount; i++)
        {
            if (i == 11)
                keysStackPanel = _mainWindow.KeysStackPanel2;

            var hsPanel = new HotkeyStratagemPanel
            {
                HotkeyName = Enum.GetName(_keys[i]) ?? "Error",
            };
            hsPanel.ClearStratagem();
            keysStackPanel.Children.Add(hsPanel);

            var i1 = i;
            hsPanel.PointerPressed += (_, _) =>
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

            _hotkeyPanels[i] = hsPanel;
        }

        SelectedKeyIndex = _hotkeyPanels.Length - 1;
        _hotkeyPanels.Last().IsBorderVisible = true;
    }

    public bool HasResized { get; set; }

    public void LoadBySpeechLanguage()
    {
        StratagemManager.Load();
        InitStratagemGroupsUI();
        InitSettingsToUI();
        SetHotkeyGroup();
        LoadVoiceNames();

        HasResized = true;

        ResetVoiceCommand().ConfigureAwait(false);
        LoadGeneratingVoiceStyles().ConfigureAwait(false);
    }

    public void UpdateToolTip(Stratagem stratagem)
    {
        ToolTip.SetTip(stratagem.CheckBox,
            string.Format(
                Localizer.Get("StratagemToolTip"),
                StratagemManager.GetSystemAlias(stratagem.Name),
                StratagemManager.GetUserAlias(stratagem.Name)));
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

                UpdateToolTip(stratagem);

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

                    if (Settings.EnableHotkeyTrigger)
                    {
                        if (stratagemCheckBox.IsChecked == true)
                        {
                            SetKeyStratagem(SelectedKeyIndex, stratagem);

                            if (SelectedKeyIndex > 0)
                                if (!_hotkeyPanels[SelectedKeyIndex - 1].HasStratagem)
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
                    else
                    {
                        _isSettingStratagemCheckBoxChecked = true;
                        stratagemCheckBox.IsChecked = !stratagemCheckBox.IsChecked;
                        _isSettingStratagemCheckBoxChecked = false;
                    }

                    if (Settings.PlayVoice)
                        PlayStratagemVoice(stratagem.Name);

                }
            }
        }
    }

    private void InitSettingsToUI()
    {
        if (TriggerKeys.Contains(Settings.TriggerKey))
            TriggerKey = Settings.TriggerKey;
        else
            TriggerKey = Settings.TriggerKey = TriggerKeys.First();

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
        WakeUpWord = Settings.WakeUpWord;

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
                SetKeyStratagem(i, null);
            else
                SetKeyStratagem(i, stratagem);
        }
    }

    private bool _isSettingStratagemCheckBoxChecked;

    private List<Voice> _voices = null!;

    private async Task LoadGeneratingVoiceStyles()
    {
        if (Settings.SpeechLocale == "")
            return;

        IsLoading = true;

        try
        {
            var manager = await VoicesManager.Create();
            _voices = manager.Find(language: Settings.SpeechLanguage);
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
            var languageVoicePath = Path.Combine(VoiceRootPath, Settings.SpeechLanguage);

            var styles = Directory.GetDirectories(languageVoicePath)
                .Select(Path.GetFileName)
                .Where(style => style != null)
                .ToList();

            VoiceNames.Clear();
            foreach (var style in styles)
                VoiceNames.Add(style!);

            if (styles.Contains(Settings.VoiceName))
                CurrentVoiceName = Settings.VoiceName;
            else
                Settings.VoiceName = CurrentVoiceName = styles.FirstOrDefault() ?? "";
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

    private void SetKeyStratagem(int index, Stratagem? stratagem)
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
                    _hotkeyPanels[prevIndex].ClearStratagem();
                }
            }

            // Set the new hotkey
            _keyStratagems[index] = stratagem;
            _hotkeyPanels[index].StratagemName = stratagem.Name;

            _isSettingStratagemCheckBoxChecked = true;
            stratagem.CheckBox.IsChecked = true;
            _isSettingStratagemCheckBoxChecked = false;
        }
        else
        {
            _keyStratagems[index] = null;
            _hotkeyPanels[index].ClearStratagem();
        }

        if (!IsLoading)
            KeySettingsChanged = true;
    }

    private static readonly string VoiceRootPath = Path.Combine(AppSettings.ExeDirectory, "Voice");

    private void PlayStratagemVoice(string stratagemName)
    {
        PlayVoice(Path.Combine(VoiceRootPath, Settings.SpeechLanguage, Settings.VoiceName, stratagemName + ".mp3"));
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

        if (!SettingsChanged)
            return;

        Settings.StratagemSets.Clear();
        Settings.StratagemSets.Add(GetKeyStratagemString());
        foreach (var item in StratagemSets)
            Settings.StratagemSets.Add(item);
        await AppSettings.SaveAsync();

        SettingsChanged = false;
    }

    private async Task ResetVoiceCommand()
    {
        await StopSpeechTrigger();

        if (Settings.EnableSpeechTrigger)
            await StartSpeechTrigger();
    }

    [ObservableProperty]
    private string _speechRecognizeResult = "";

    private VoiceCommand? _voiceCommand;

    private async Task StartSpeechTrigger()
    {
        if (Design.IsDesignMode)
            return;

        if (_voiceCommand == null)
        {
            var languages = VoiceCommand.GetInstalledRecognizers();
            if (languages.Count == 0)
            {
                await new MessageDialog(
                        Localizer.Get("Error"),
                        Localizer.Get("SpeechRecognitionEngineNotInstalled"))
                    .ShowDialog(_mainWindow);
                return;
            }

            if (!languages.Contains(Settings.SpeechLocale))
            {
                await new MessageDialog(
                        Localizer.Get("Error"),
                        string.Format(Localizer.Get("SpeechRecognitionEngineOfLanguageNotInstalled"), Settings.SpeechLocale))
                    .ShowDialog(_mainWindow);
                return;
            }

            try
            {
                _voiceCommand = new VoiceCommand(Settings.SpeechLocale, Settings.WakeUpWord, [.. StratagemManager.StratagemAlias]);
            }
            catch (Exception)
            {
                await new MessageDialog(
                        Localizer.Get("Error"),
                        Localizer.Get("CreatingSpeechRecognitionFailed"))
                    .ShowDialog(_mainWindow);
                return;
            }

            _voiceCommand.CommandRecognized += (_, command) =>
            {
                var failed = command.Score < Settings.VoiceConfidence;
                var result = failed ? Localizer.Get("Failed") : Localizer.Get("Success");
                var info = string.Format(Localizer.Get("RecognitionResult"), result, command.Score, command.Text);
                SpeechRecognizeResult = info;

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
        SettingsChanged = true;
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
                if (await new YesNoDialog(
                            Localizer.Get("Info"),
                            Localizer.Get("NewVersionDetectedWantUpdate"))
                        .ShowDialog<bool>(_mainWindow))
                    AutoUpdate.Update();
            }
            else
            {
                await new MessageDialog(
                        Localizer.Get("Info"),
                        Localizer.Get("YouHaveTheLatestVersion"))
                    .ShowDialog<bool>(_mainWindow);
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
        SettingsChanged = true;

        if (Settings.EnableSpeechTrigger)
            await StartSpeechTrigger();
        else
            await StopSpeechTrigger();
    }

    [ObservableProperty]
    private string _wakeUpWord = "";

    async partial void OnWakeUpWordChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.WakeUpWord = value.Trim();
        SettingsChanged = true;
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
        SettingsChanged = true;

        HotkeyGroupManager.Enabled = Settings.EnableHotkeyTrigger;
    }

    [ObservableProperty]
    private bool _enableSetKeyBySpeech;

    partial void OnEnableSetKeyBySpeechChanged(bool value)
    {
        if (IsLoading)
            return;

        Settings.EnableSetKeyBySpeech = value;
        SettingsChanged = true;
    }

    [RelayCommand]
    private void SaveStratagemSet()
    {
        var keyStratagemString = GetKeyStratagemString();
        if (StratagemSets.Contains(keyStratagemString))
            return;

        StratagemSets.Add(keyStratagemString);
        CurrentStratagemSetIndex = StratagemSets.Count - 1;

        SettingsChanged = true;
    }

    [RelayCommand]
    private void DeleteStratagemSet()
    {
        if (CurrentStratagemSetIndex == -1)
            return;

        StratagemSets.RemoveAt(CurrentStratagemSetIndex);

        SettingsChanged = true;
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
                    Path.Combine(VoiceRootPath, Settings.SpeechLanguage, voiceName, stratagem.Name + ".mp3"));
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
            await new MessageDialog(
                    Localizer.Get("Info"),
                    Localizer.Get("PleaseTurnOnMicrophoneCallOut"))
                .ShowDialog(_mainWindow);
            return;
        }

        var scores = new float[5];
        var stratagemNames = Enumerable.Range(0, 5)
            .Select(_ => StratagemManager.Stratagems[Random.Shared.Next(StratagemManager.Stratagems.Count)].Name)
            .ToArray();
        var times = 0;
        var dialog = new CalibrateVoiceDialog();

        _voiceCommand.CommandRecognized += TestEvent;

        try
        {
            var currentTime = times;
            _ = dialog.ShowDialog(_mainWindow);

            while (dialog.IsVisible)
            {
                var message = string.Format(
                    Localizer.Get("PleaseReadThis"), Settings.WakeUpWord, stratagemNames[currentTime]);
                dialog.SetMessage(message);

                while (times == currentTime && dialog.IsVisible)
                    await Task.Delay(100);
                currentTime = times;
                if (currentTime < scores.Length)
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
            if (command.Text == stratagemNames[times] && times < scores.Length)
            {
                scores[times] = command.Score;
                times++;
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
        KeySettingsChanged = true;
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

        _infoWindow = new InfoWindow();
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

    [ObservableProperty]
    private bool _isSpeechRecognitionInfoWindowClickThrough;

    partial void OnIsSpeechRecognitionInfoWindowClickThroughChanged(bool value)
    {
        if (_infoWindow != null)
            _infoWindow.IsClickThrough = value;
    }

    [RelayCommand]
    private async Task Cleanup()
    {
        HideInfoWindow();
        HotkeyGroupManager.ClearHotkeyGroup();
        await StopSpeechTrigger();
    }
}
