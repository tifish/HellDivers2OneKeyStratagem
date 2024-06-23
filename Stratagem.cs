using System.Windows.Controls;
using System.Windows.Input;

namespace HellDivers2OneKeyStratagem;

public class Stratagem
{
    public string Name = "";
    public string KeySequence = "";
    public CheckBox CheckBox = null!;

    public void PressKeys()
    {
        SendKey.Down(Settings.TriggerKey == "Ctrl" ? Key.LeftCtrl : Key.LeftAlt);

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

        SendKey.Up(Settings.TriggerKey == "Ctrl" ? Key.LeftCtrl : Key.LeftAlt);
    }
}
