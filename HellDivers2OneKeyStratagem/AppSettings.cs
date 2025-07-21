global using static HellDivers2OneKeyStratagem.SettingsContainer;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using JeekTools;

namespace HellDivers2OneKeyStratagem;

public class AppSettings
{
    public static readonly string ExePath = Environment.ProcessPath!;
    public static readonly string ExeFileName = Path.GetFileNameWithoutExtension(ExePath);
    public static readonly string ExeDirectory = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string SettingsDirectory = Path.Combine(ExeDirectory, "Settings");
    public static readonly string SettingsFile = Path.Combine(SettingsDirectory, "Settings.json");
    public static readonly string DataDirectory = Path.Combine(ExeDirectory, "Data");
    public static readonly string IconsDirectory = Path.Combine(ExeDirectory, "Icons");
    public static readonly string RawIconsDirectory = Path.Combine(ExeDirectory, "RawIcons");

    private static readonly JsonFile<AppSettings> _settingsFile = new(SettingsFile);

    public static async Task Load()
    {
        var settings = await _settingsFile.Load();
        if (settings != null)
            Settings = settings;
    }

    public static async Task Save()
    {
        if (Design.IsDesignMode)
            return;

        await _settingsFile.Save(Settings);
    }

    public string TriggerKey { get; set; } = "LeftCtrl";
    public string OperateKeys { get; set; } = "WASD";
    public bool PlayVoice { get; set; } = true;
    public string VoiceName { get; set; } = "晓妮";
    public bool EnableSpeechTrigger { get; set; }
    public bool EnableSetKeyBySpeech { get; set; }
    public List<string> StratagemSets { get; set; } = [];
    public string SpeechLocale { get; set; } = "";

    [JsonIgnore]
    public string SpeechLanguage
    {
        get
        {
            var sepPos = SpeechLocale.IndexOf('-');
            return sepPos > 0
                ? SpeechLocale[..sepPos]
                : SpeechLocale;
        }
    }

    public string Locale { get; set; } = "";

    public double VoiceConfidence { get; set; } = 0.6f;
    public string WakeUpWord { get; set; } = "";
    public bool EnableHotkeyTrigger { get; set; } = true;
    public const string UpdateUrl = "https://github.com/tifish/HellDivers2OneKeyStratagem/releases/download/latest_release/HellDivers2OneKeyStratagem.7z";
    public bool DisableMirrorDownload { get; set; } = false;
}

public static class SettingsContainer
{
    public static AppSettings Settings = new();
}
