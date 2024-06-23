using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public static class WindowHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    public static string? GetActiveWindowTitle()
    {
        const int nChars = 256;
        var buff = new StringBuilder(nChars);
        var handle = GetForegroundWindow();

        if (GetWindowText(handle, buff, nChars) > 0)
            return buff.ToString();
        return null;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    public static string GetActiveProcessPath()
    {
        try
        {
            // Get the handle of the currently active window
            var hWnd = GetForegroundWindow();

            if (hWnd == IntPtr.Zero)
                return "";

            // Get the process ID of the active window
            if (GetWindowThreadProcessId(hWnd, out var processId) == 0)
                return "";

            // Get the process by ID
            var process = Process.GetProcessById((int)processId);

            // Get the executable path of the process
            if (process.MainModule == null)
                return "";

            return process.MainModule.FileName;
        }
        catch (Exception)
        {
            return "";
        }
    }

    public static string GetActiveProcessFileName()
    {
        var filePath = GetActiveProcessPath();
        return filePath == "" ? "" : Path.GetFileName(filePath);
    }
}
