namespace HellDivers2OneKeyStratagem;

public class Stratagem
{
    public string Name = "";
    public string KeySequence = "";
    public CheckBox CheckBox = null!;

    public void PressKeys()
    {
        SendKey.Down(Settings.TriggerKey == "Ctrl" ? Keys.ControlKey : Keys.Alt);

        foreach (var key in KeySequence)
        {
            switch (key)
            {
                case '↑': SendKey.Press(Settings.OperateKeys == "WASD" ? Keys.W :Keys.Up); break;
                case '↓': SendKey.Press(Settings.OperateKeys == "WASD" ? Keys.S :Keys.Down); break;
                case '←': SendKey.Press(Settings.OperateKeys == "WASD" ? Keys.A :Keys.Left); break;
                case '→': SendKey.Press(Settings.OperateKeys == "WASD" ? Keys.D :Keys.Right); break;
            }
        }

        SendKey.Up(Settings.TriggerKey == "Ctrl" ? Keys.ControlKey : Keys.Alt);
    }
}
