global using static HellDivers2OneKeyStratagem.SettingsContainer;
using System.Text.Json.Serialization;

namespace HellDivers2OneKeyStratagem;

public class AppSettings
{
    public static readonly string ExeDirectory = Path.GetDirectoryName(Application.ExecutablePath)!;
    public static readonly string SettingsDirectory = Path.Combine(ExeDirectory, "Settings");
    public static readonly string SettingsFile = Path.Combine(SettingsDirectory, "Settings.json");
    public static readonly string DataDirectory = Path.Combine(ExeDirectory, "Data");

    public string TriggerKey { get; set; } = "Ctrl";
    public string OperateKeys { get; set; } = "WASD";
    public bool PlayVoice { get; set; } = true;
    public string VoiceName { get; set; } = "晓妮";
    public bool EnableSpeechTrigger { get; set; } = true;
    public bool EnableSetFKeyBySpeech { get; set; } = true;
    public List<string> StratagemSets { get; set; } = [];
    public string Locale { get; set; } = "";

    [JsonIgnore]
    public string Language => Locale.Length > 2 ? Locale[..2] : Locale;

    public float VoiceConfidence { get; set; } = 0.6f;
    public string WakeupWord { get; set; } = "";
    public bool EnableHotkeyTrigger { get; set; } = true;
}

public static class SettingsContainer
{
    public static AppSettings Settings = new();
}
