global using static HellDivers2OneKeyStratagem.AppSettings;

namespace HellDivers2OneKeyStratagem;

public class AppSettings
{
    public static AppSettings Settings = new();

    public string TriggerKey { get; set; } = "Ctrl";
    public string OperateKeys { get; set; } = "WASD";
    public bool PlayVoice { get; set; } = true;
    public string VoiceName { get; set; } = "晓妮";
    public bool EnableVoiceTrigger { get; set; } = true;
    public string VoiceTriggerKey { get; set; } = "`";
    public bool EnableSetFKeyByVoice { get; set; } = true;
    public List<string> StratagemSets { get; set; } = [];
    public string Language { get; set; } = "";
    public float VoiceConfidence { get; set; } = 0.99f;
    public string WakeupWord { get; set; } = "";
}
