using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdgeTtsSharp;
using EdgeTtsSharp.Structures;
using GlobalHotKeys;
using Jeek.Avalonia.Localization;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using ZLogger;

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
    public partial double SpeechConfidence { get; set; }

    partial void OnSpeechConfidenceChanged(double value)
    {
        if (IsLoading)
            return;

        Settings.VoiceConfidence = Math.Round(value, 3);
        SettingsChanged = true;
    }

    [ObservableProperty]
    public partial ObservableCollection<string> SpeechLocales { get; set; } = [];

    [ObservableProperty]
    public partial string CurrentSpeechLocale { get; set; } = "";

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
    public partial ObservableCollection<string> TriggerKeys { get; set; } = ["LeftCtrl", "LeftAlt", "Q"];

    [ObservableProperty]
    public partial string TriggerKey { get; set; } = "";

    partial void OnTriggerKeyChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.TriggerKey = value;
        KeySettingsChanged = true;
    }

    [ObservableProperty]
    public partial string OperateKeys { get; set; } = "";

    partial void OnOperateKeysChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.OperateKeys = value;
        KeySettingsChanged = true;
    }

    [ObservableProperty]
    public partial bool PlayVoiceWhenCall { get; set; }

    partial void OnPlayVoiceWhenCallChanged(bool value)
    {
        if (IsLoading)
            return;

        Settings.PlayVoice = value;
        KeySettingsChanged = true;
    }

    [ObservableProperty]
    public partial ObservableCollection<string> VoiceNames { get; set; } = [];

    [ObservableProperty]
    public partial string CurrentVoiceName { get; set; } = "";

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

        if (!Directory.Exists(AppSettings.IconsDirectory))
        {
            IconManager.ConvertAllIcons();
            _mainWindow.Close();
        }
    }

    private bool _isClosing;

    [RelayCommand]
    private void WindowClosing()
    {
        if (_isClosing)
            return;
        _isClosing = true;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
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
    public partial ObservableCollection<string> Locales { get; set; } = [];

    [ObservableProperty]
    public partial string CurrentLocale { get; set; } = "";

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

            var hotkeyPanel = new HotkeyStratagemPanel
            {
                HotkeyName = Enum.GetName(_keys[i]) ?? "Error",
            };
            keysStackPanel.Children.Add(hotkeyPanel);
            hotkeyPanel.ApplyTemplate();

            var i1 = i;
            hotkeyPanel.PointerPressed += (_, args) =>
            {
                switch (args.GetCurrentPoint(null).Properties.PointerUpdateKind)
                {
                    // Select the key
                    case PointerUpdateKind.LeftButtonPressed:
                        SelectedKeyIndex = i1;

                        if (!Settings.PlayVoice)
                            return;

                        if (Settings.EnableSetKeyBySpeech)
                            return;

                        var stratagem = _keyStratagems[SelectedKeyIndex];
                        if (stratagem != null)
                            PlayStratagemVoice(stratagem.Name);
                        break;

                    // Remove the stratagem from the key
                    case PointerUpdateKind.RightButtonPressed:
                        SetKeyStratagem(i1, null);
                        break;
                }
            };

            _hotkeyPanels[i] = hotkeyPanel;
        }

        SelectedKeyIndex = _hotkeyPanels.Length - 1;
        _hotkeyPanels.Last().IsBorderVisible = true;
    }

    public void LoadBySpeechLanguage()
    {
        StratagemManager.Load();
        InitStratagemGroupsUI();
        InitSettingsToUI();
        SetHotkeyGroup();
        LoadVoiceNames();

        ResetVoiceCommand().ConfigureAwait(false);
        LoadGeneratingVoiceStyles().ConfigureAwait(false);
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
                Orientation = Orientation.Horizontal,
            };
            border.Child = groupContainer;

            var groupLabel = new Label { Content = groupName, HorizontalAlignment = HorizontalAlignment.Center };
            groupContainer.Children.Add(groupLabel);

            foreach (var stratagem in stratagems)
            {
                var stratagemControl = new StratagemControl();
                groupContainer.Children.Add(stratagemControl);
                stratagemControl.ApplyTemplate();
                stratagemControl.Stratagem = stratagem;
                stratagem.Control = stratagemControl;

                stratagemControl.PointerPressed += async (_, args) =>
                {
                    switch (args.GetCurrentPoint(null).Properties.PointerUpdateKind)
                    {
                        case PointerUpdateKind.LeftButtonPressed:
                            {
                                // Set the stratagem to the selected key
                                if (Settings.EnableHotkeyTrigger)
                                {
                                    SetKeyStratagem(SelectedKeyIndex, stratagem);
                                }

                                // Play the voice of the stratagem
                                if (Settings.PlayVoice)
                                    PlayStratagemVoice(stratagem.Name);
                            }
                            break;

                        case PointerUpdateKind.RightButtonPressed:
                            {
                                // Edit the aliases of the stratagem
                                var dialog = new EditAliasesDialog(stratagem.Name);

                                if (await dialog.ShowDialog<bool>(_mainWindow))
                                {
                                    dialog.Commit();
                                    await ResetVoiceCommand();
                                }
                            }
                            break;
                    }
                };
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

    private async Task LoadGeneratingVoiceStyles()
    {
        if (Settings.SpeechLocale == "")
            return;

        IsLoading = true;

        try
        {
            var voices = await EdgeTts.GetVoices();
            voices = [.. voices.Where(v => v.Locale.StartsWith(Settings.SpeechLanguage))];

            GenerateVoiceStyles.Clear();
            foreach (var voice in voices)
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

        if (stratagem != null)
        {
            // Set the stratagem to the key
            _keyStratagems[index] = stratagem;
            _hotkeyPanels[index].Stratagem = stratagem;
        }
        else
        {
            _keyStratagems[index] = null;
            _hotkeyPanels[index].Stratagem = null;
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
    public partial string SpeechRecognizeResult { get; set; } = "";

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
    public partial string UpdateUrl { get; set; } = "";

    partial void OnUpdateUrlChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.UpdateUrl = value;
        SettingsChanged = true;
    }


    [ObservableProperty]
    public partial bool IsCheckingForUpdate { get; set; }

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
    public partial bool EnableSpeechTrigger { get; set; }

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
    public partial string WakeUpWord { get; set; } = "";

    async partial void OnWakeUpWordChanged(string value)
    {
        if (IsLoading)
            return;

        Settings.WakeUpWord = value.Trim();
        SettingsChanged = true;
        await ResetVoiceCommand();
    }

    [ObservableProperty]
    public partial bool IsGenerateVoicePanelVisible { get; set; }

    [RelayCommand]
    private void ToggleGenerateVoicePanel()
    {
        IsGenerateVoicePanelVisible = !IsGenerateVoicePanelVisible;
    }

    [ObservableProperty]
    public partial bool EnableHotkeyTrigger { get; set; }

    partial void OnEnableHotkeyTriggerChanged(bool value)
    {
        if (IsLoading)
            return;

        Settings.EnableHotkeyTrigger = value;
        SettingsChanged = true;

        HotkeyGroupManager.Enabled = Settings.EnableHotkeyTrigger;
    }

    [ObservableProperty]
    public partial bool EnableSetKeyBySpeech { get; set; }

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
    public partial bool IsGeneratingVoice { get; set; }

    private static readonly Dictionary<string, string> _voiceNameMap = new()
    {
        ["zh-CN-liaoning-XiaobeiNeural"] = "晓北（东北）",
        ["zh-HK-HiuMaanNeural"] = "晓曼（香港）",
        ["zh-CN-shaanxi-XiaoniNeural"] = "晓妮（陕西）",
        ["zh-CN-XiaoxiaoNeural"] = "晓晓",
        ["zh-CN-XiaoyiNeural"] = "晓伊",
        ["zh-TW-HsiaoChenNeural"] = "晓臻（台湾）",
        ["en-US-AnaNeural"] = "Ana",
        ["en-IE-EmilyNeural"] = "Emily",
        ["en-US-GuyNeural"] = "Guy",
    };

    [RelayCommand]
    private async Task GenerateSelectedVoice()
    {
        if (CurrentGenerateVoiceStyleIndex == -1)
            return;

        IsGeneratingVoice = true;

        try
        {
            var count = 0;
            var total = StratagemManager.Count;

            count = await GenerateVoice(CurrentGenerateVoiceStyle, count, total);
        }
        catch (OperationCanceledException)
        {
            // Has been logged, do nothing here
        }
        catch (Exception)
        {
            GenerateVoiceMessage = "民主语音生成失败...";
        }
        finally
        {
            GenerateVoiceMessage = IsGeneratingVoice
                ? "民主语音生成完毕！"
                : "民主语音进程中断...";

            IsGeneratingVoice = false;
        }
    }

    [RelayCommand]
    private async Task GenerateDefaultVoices()
    {
        IsGeneratingVoice = true;

        try
        {
            var count = 0;
            var total = _voiceNameMap.Count * StratagemManager.Count;

            foreach (var voiceStyle in _voiceNameMap.Keys)
                count = await GenerateVoice(voiceStyle, count, total);
        }
        catch (OperationCanceledException)
        {
            // Has been logged, do nothing here
        }
        catch (Exception)
        {
            GenerateVoiceMessage = "民主语音生成失败...";
        }
        finally
        {
            GenerateVoiceMessage = IsGeneratingVoice
                ? "民主语音生成完毕！"
                : "民主语音进程中断...";

            IsGeneratingVoice = false;
        }
    }

    [RelayCommand]
    private void StopGeneratingVoice()
    {
        IsGeneratingVoice = false;
    }

    private async Task<int> GenerateVoice(string voiceStyle, int count, int total)
    {
        var voice = await EdgeTts.GetVoice(voiceStyle);

        foreach (var stratagem in StratagemManager.Stratagems)
        {
            if (!IsGeneratingVoice)
                break;

            count++;

            var voiceLanguage = voiceStyle[..2];

            if (!_voiceNameMap.TryGetValue(voiceStyle, out var voiceFolderName))
                voiceFolderName = voiceStyle;

            var stratagemName = stratagem.Name;
            if (voiceLanguage == "en")
                stratagemName = stratagem.Id;

            var filePath = Path.Combine(VoiceRootPath, voiceLanguage, voiceFolderName, stratagemName + ".mp3");

            if (File.Exists(filePath))
                continue;

            await GenerateVoiceFile(stratagem.Name, filePath, voice);

            GenerateVoiceMessage = $"正在生成民主语音（{count}/{total}）：{stratagem.Name}";
        }

        return count;
    }

    private async Task GenerateVoiceFile(string text, string filePath, Voice? voice = null)
    {
        var voiceDir = Path.GetDirectoryName(filePath) ?? throw new Exception("Failed to extract voice directory");
        if (!Directory.Exists(voiceDir))
            Directory.CreateDirectory(voiceDir);

        voice ??= await EdgeTts.GetVoice(CurrentGenerateVoiceStyle);
        if (voice == null)
            throw new Exception($"Failed to get voice: {CurrentGenerateVoiceStyle}");

        int retryCount = 0;
        const int maxRetries = 3;

        while (retryCount < maxRetries)
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await voice.SaveAudioToFile(text, filePath, null, cts.Token);
                return;
            }
            catch (OperationCanceledException ex)
            {
                retryCount++;
                GenerateVoiceMessage = $"生成民主语音超时（第{retryCount}次重试）：{filePath}";
                _logger.ZLogError(ex, $"生成民主语音超时（第{retryCount}次重试）：{filePath}");

                if (retryCount < maxRetries)
                {
                    // Wait for 1 minute
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                else
                {
                    throw;
                }
            }
        }
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
        catch (OperationCanceledException)
        {
            // Has been logged, do nothing here
        }
        catch (Exception ex)
        {
            GenerateVoiceMessage = "民主语音生成失败...";
            _logger.ZLogError(ex, $"民主语音生成失败");
        }
    }

    [ObservableProperty]
    public partial ObservableCollection<string> GenerateVoiceStyles { get; set; } = [];

    [ObservableProperty]
    public partial string CurrentGenerateVoiceStyle { get; set; } = "";

    async partial void OnCurrentGenerateVoiceStyleChanged(string value)
    {
        if (IsLoading)
            return;

        await TryVoice();
    }

    [ObservableProperty]
    public partial int CurrentGenerateVoiceStyleIndex { get; set; }

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
    public partial ObservableCollection<string> StratagemSets { get; set; } = [];

    [ObservableProperty]
    public partial int CurrentStratagemSetIndex { get; set; }

    partial void OnCurrentStratagemSetIndexChanged(int value)
    {
        if (value < 0 || value >= StratagemSets.Count)
            return;

        SetKeyStratagemString(StratagemSets[value]);
        KeySettingsChanged = true;
    }

    [ObservableProperty]
    public partial ObservableCollection<string> Mics { get; set; } = [];

    [ObservableProperty]
    public partial string CurrentMic { get; set; } = "";

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
    public partial string GenerateVoiceMessage { get; set; } = "未生成";

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
    public partial bool ShowSpeechInfoWindow { get; set; }

    partial void OnShowSpeechInfoWindowChanged(bool value)
    {
        if (value)
            ShowInfoWindow();
        else
            HideInfoWindow();
    }

    [ObservableProperty]
    public partial bool IsSpeechRecognitionInfoWindowClickThrough { get; set; }

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
