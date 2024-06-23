using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;

public static class HotkeyGroupManager
{
    private static Dictionary<Key, EventHandler<HotkeyEventArgs>> _hotkeyGroup = new();

    public static void SetHotkeyGroup(Dictionary<Key, EventHandler<HotkeyEventArgs>> hotkeys)
    {
        if (Enabled)
            UnregisterHotkeys();

        _hotkeyGroup = hotkeys;

        if (Enabled)
            RegisterHotkeys();
    }

    public static void ClearHotkeyGroup()
    {
        if (Enabled)
            UnregisterHotkeys();

        _hotkeyGroup.Clear();
    }

    private static bool _enabled;

    public static bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value)
                return;

            _enabled = value;

            if (value)
                RegisterHotkeys();
            else
                UnregisterHotkeys();
        }
    }

    private static void RegisterHotkeys()
    {
        foreach (var (hotkey, handler) in _hotkeyGroup)
            HotkeyManager.Current.AddOrReplace(Enum.GetName(hotkey), hotkey, ModifierKeys.None, handler);
    }

    private static void UnregisterHotkeys()
    {
        foreach (var hotkey in _hotkeyGroup.Keys)
            HotkeyManager.Current.Remove(Enum.GetName(hotkey));
    }
}
