using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace HellDivers2OneKeyStratagem;

public static class AutoUpdate
{
    public static async Task<bool> HasUpdate()
    {
        var headers = await HttpHelper.GetHeaders(AppSettings.UpdateUrl);

        var updateTime = headers?.LastModified;
        if (updateTime == null)
            return false;

        var dllTime = File.GetLastWriteTime(AppSettings.ExePath);

        return updateTime - dllTime > TimeSpan.FromMinutes(1);
    }

    public static bool Update()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"""
                        -ExecutionPolicy Bypass -File "AutoUpdate.ps1" "{AppSettings.UpdateUrl}"
                        """,
            WorkingDirectory = AppSettings.ExeDirectory,
            UseShellExecute = true,
        });

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            lifetime.MainWindow?.Close();

        return true;
    }
}
