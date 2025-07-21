using Avalonia.Threading;
using JeekTools;

namespace Helldivers2OneKeyStratagem;

public static class ActiveWindowMonitor
{
    private static DispatcherTimer _timer = new();

    public static void Start(TimeSpan timeSpan)
    {
        _timer.Interval = timeSpan;
        _timer.Tick += TimerOnTick;
        _timer.Start();
    }

    private static string _currentProcessFileName = "";
    public static string CurrentProcessFileName => _currentProcessFileName;

    private static string _currentWindowTitle = "";
    public static string CurrentWindowTitle => _currentWindowTitle;

    private static void TimerOnTick(object? sender, EventArgs e)
    {
        var newProcessFileName = WindowHelper.GetActiveProcessFileName();
        if (newProcessFileName != "" && newProcessFileName != _currentProcessFileName)
        {
            ProcessChanged?.Invoke(null, new ProcessChangedEventArgs(_currentProcessFileName, newProcessFileName));
            _currentProcessFileName = newProcessFileName;
        }

        var newWindowTitle = WindowHelper.GetActiveWindowTitle();
        if (newWindowTitle != null && newWindowTitle != _currentWindowTitle)
        {
            WindowTitleChanged?.Invoke(null, new WindowTitleChangedEventArgs(_currentWindowTitle, newWindowTitle));
            _currentWindowTitle = newWindowTitle;
        }
    }

    public static event EventHandler<ProcessChangedEventArgs>? ProcessChanged;
    public static event EventHandler<WindowTitleChangedEventArgs>? WindowTitleChanged;

    public static void Stop()
    {
        _timer.Stop();
        _timer.Tick -= TimerOnTick;
    }
}

public class ProcessChangedEventArgs(string oldProcessFileName, string newProcessFileName) : EventArgs
{
    public string OldProcessFileName { get; } = oldProcessFileName;
    public string NewProcessFileName { get; } = newProcessFileName;
}

public class WindowTitleChangedEventArgs(string oldWindowTitle, string newWindowTitle) : EventArgs
{
    public string OldWindowTitle { get; } = oldWindowTitle;
    public string NewWindowTitle { get; } = newWindowTitle;
}
