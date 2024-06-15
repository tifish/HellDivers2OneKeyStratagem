global using static HellDivers2OneKeyStratagem.SettingsContainer;

namespace HellDivers2OneKeyStratagem;

public class AppSettings
{
    public static readonly string ExeDirectory = Path.GetDirectoryName(Application.ExecutablePath)!;
    public static readonly string SettingsDirectory = Path.Combine(ExeDirectory, "Settings");
    public static readonly string SettingsFile = Path.Combine(SettingsDirectory, "Settings.json");

    public string TriggerKey { get; set; } = "Ctrl";
    public string OperateKeys { get; set; } = "WASD";
    public bool PlayVoice { get; set; } = true;
    public string VoiceName { get; set; } = "晓妮";
    public bool EnableVoiceTrigger { get; set; } = true;
    public bool EnableSetFKeyByVoice { get; set; } = true;
    public List<string> StratagemSets { get; set; } = [];
    public string Language { get; set; } = "";
    public float VoiceConfidence { get; set; } = 0.6f;
    public string WakeupWord { get; set; } = "";
    public bool EnableHotkeyTrigger { get; set; } = true;
}

public static class SettingsContainer
{
    public static AppSettings Settings = new();
}
