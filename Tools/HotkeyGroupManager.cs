using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Win32.Input;
using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using HotKeyManager = GlobalHotKeys.HotKeyManager;

public static class HotkeyGroupManager
{
    private static HotKeyManager? _hotKeyManager;

    static HotkeyGroupManager()
    {
    }

    private static void OnHotkeyPressed(HotKey hotkey)
    {
        var key = KeyInterop.KeyFromVirtualKey((int)hotkey.Key, 0);
        if (_hotkeyGroup.TryGetValue(key, out var handler))
            handler.Invoke(_hotKeyManager, hotkey);
    }

    private static Dictionary<Key, EventHandler<HotKey>> _hotkeyGroup = new();

    public static void SetHotkeyGroup(Dictionary<Key, EventHandler<HotKey>> hotkeys)
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
        if (Design.IsDesignMode)
            return;

        if (_hotKeyManager != null)
        {
            UnregisterHotkeys();
            return;
        }

        _hotKeyManager = new HotKeyManager();
        _hotKeyManager.HotKeyPressed.Subscribe(OnHotkeyPressed);

        foreach (var (hotkey, handler) in _hotkeyGroup)
            _hotKeyManager.Register((VirtualKeyCode)KeyInterop.VirtualKeyFromKey(hotkey), Modifiers.NoRepeat);
    }

    private static void UnregisterHotkeys()
    {
        if (_hotKeyManager == null)
            return;

        _hotKeyManager.Dispose();
        _hotKeyManager = null;
    }
}
