using System.Diagnostics;
using JeekTools;

namespace Helldivers2OneKeyStratagem;

public static class AutoUpdate
{
    public static string DownloadUrl { get; private set; } = "";
    public static DateTime? RemoteTime { get; private set; } = null;
    public static DateTime? LocalTime { get; private set; } = null;

    public static async Task<bool> HasUpdate()
    {
        try
        {
            DownloadUrl = "";
            RemoteTime = null;
            LocalTime = null;

            if (Settings.DisableMirrorDownload)
            {
                DownloadUrl = AppSettings.UpdateUrl;
            }
            else
            {
                // Get the fastest mirror
                var mirror = await GitHubMirrors.GetFastestMirror(AppSettings.UpdateUrl);
                if (mirror == "")
                    return false;

                DownloadUrl = mirror;
            }

            // Try to get the headers from the mirror
            var headers = await HttpHelper.GetHeaders(DownloadUrl);
            if (headers == null)
                return false;

            RemoteTime = headers.LastModified;

            LocalTime = File.GetLastWriteTime(AppSettings.ExePath);

            return RemoteTime - LocalTime > TimeSpan.FromMinutes(1);
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> Update(bool hideMainWindow)
    {
        try
        {
            if (DownloadUrl == "")
                return false;

            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"""
                        -ExecutionPolicy Bypass -File "AutoUpdate.ps1" "{DownloadUrl}" {(hideMainWindow ? "/hide" : "")}
                        """,
                WorkingDirectory = AppSettings.ExeDirectory,
                UseShellExecute = true,
            });

            await MainViewModel.Instance.ExitApplication();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
