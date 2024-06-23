using System.Runtime.InteropServices;
using System.Windows.Input;

public static class SendKey
{
    // keybd_event
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

    private const uint KEYEVENTF_KEYDOWN = 0x0000;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public static void Down(Key key)
    {
        var vk = KeyInterop.VirtualKeyFromKey(key);
        keybd_event((byte)vk, 0, KEYEVENTF_KEYDOWN, 0);
        Thread.Sleep(50);
    }

    public static void Up(Key key)
    {
        var vk = KeyInterop.VirtualKeyFromKey(key);
        keybd_event((byte)vk, 0, KEYEVENTF_KEYUP, 0);
        Thread.Sleep(40);
    }

    public static void Press(Key key)
    {
        var vk = KeyInterop.VirtualKeyFromKey(key);
        keybd_event((byte)vk, 0, KEYEVENTF_KEYDOWN, 0);
        Thread.Sleep(30);
        keybd_event((byte)vk, 0, KEYEVENTF_KEYUP, 0);
        Thread.Sleep(40);
    }
}
