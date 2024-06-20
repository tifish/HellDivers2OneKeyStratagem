using System.Diagnostics;
using System.Reflection;

namespace HellDivers2OneKeyStratagem;

public static class AutoUpdate
{
    public static async Task<bool> HasUpdate()
    {
        var headers = await HttpHelper.GetHeaders(Settings.UpdateUrl);
        if (headers == null)
            return false;

        // Get the CloudFlare cache time
        var updateTime = headers.LastModified
                         ?? headers.GetDateTime("x-ms-creation-time");
        if (updateTime == null)
            return false;

        var dllTime = File.GetLastWriteTime(Assembly.GetEntryAssembly()!.Location);
        // The dll's local time is actually the UTC time
        dllTime = dllTime.ToLocalTime();

        return updateTime - dllTime > TimeSpan.FromMinutes(1);
    }

    public static bool Update()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = """
                        -ExecutionPolicy Bypass -File "AutoUpdate.ps1"
                        """,
            WorkingDirectory = AppSettings.ExeDirectory,
            UseShellExecute = true,
        });

        Application.Exit();
        return true;
    }
}
