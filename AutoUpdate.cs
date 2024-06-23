using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace HellDivers2OneKeyStratagem;

public static class AutoUpdate
{
    public static async Task<bool> HasUpdate()
    {
        var headers = await HttpHelper.GetHeaders(Settings.UpdateUrl);

        var updateTime = headers?.LastModified;
        if (updateTime == null)
            return false;

        var dllTime = File.GetLastWriteTime(Assembly.GetEntryAssembly()!.Location);

        return updateTime - dllTime > TimeSpan.FromMinutes(1);
    }

    public static bool Update()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"""
                        -ExecutionPolicy Bypass -File "AutoUpdate.ps1" "{Settings.UpdateUrl}"
                        """,
            WorkingDirectory = AppSettings.ExeDirectory,
            UseShellExecute = true,
        });

        Application.Current.Shutdown();
        return true;
    }
}
