using Avalonia.Input;

namespace HellDivers2OneKeyStratagem;

public enum StratagemColor
{
    Unknown,
    R,
    B,
    G,
    Y,
}

public enum StratagemType
{
    Unknown,
    Gun,
    Cannon,
    Orbital,
    Eagle,
    Pack,
    Vehicle,
    Sentry,
    Mines,
    Mission,
    Melee,
}

public class Stratagem
{
    public string Name = "";
    public string KeySequence = "";
    public string RawIconName = "";
    public StratagemColor Color = StratagemColor.Unknown;
    public StratagemType Type = StratagemType.Unknown;
    public string Id = "";

    public StratagemControl? Control;

    public void PressKeys()
    {
        if (!Enum.TryParse<Key>(Settings.TriggerKey, out var triggerKey))
        {
            Settings.TriggerKey = "LeftCtrl";
            triggerKey = Key.LeftCtrl;
        }

        SendKey.Down(triggerKey);

        foreach (var key in KeySequence)
        {
            switch (key)
            {
                case '↑':
                    SendKey.Press(Settings.OperateKeys == "WASD" ? Key.W : Key.Up);
                    break;
                case '↓':
                    SendKey.Press(Settings.OperateKeys == "WASD" ? Key.S : Key.Down);
                    break;
                case '←':
                    SendKey.Press(Settings.OperateKeys == "WASD" ? Key.A : Key.Left);
                    break;
                case '→':
                    SendKey.Press(Settings.OperateKeys == "WASD" ? Key.D : Key.Right);
                    break;
            }
        }

        SendKey.Up(triggerKey);
    }
}
