using System.Diagnostics;
using System.Reflection;

namespace HellDivers2OneKeyStratagem;

public static class AutoUpdate
{
    private const string UpdateUrl = "https://github.com/tifish/HellDivers2OneKeyStratagem/releases/download/latest_release/HellDivers2OneKeyStratagem.zip";

    public static async Task<bool> HasUpdate()
    {
        var updateTime = await HttpHelper.GetHttpFileTime(UpdateUrl);
        if (updateTime == null)
            return false;
        updateTime = updateTime.Value.ToUniversalTime();

        var exeTime = File.GetLastWriteTime(Assembly.GetEntryAssembly()!.Location);

        return updateTime - exeTime > TimeSpan.FromMinutes(1);
    }

    public static bool SelfUpdate()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = """
                        -ExecutionPolicy Bypass -File "SelfUpdate.ps1"
                        """,
            WorkingDirectory = AppSettings.ExeDirectory,
            UseShellExecute = true,
        });

        Application.Exit();
        return true;
    }
}
