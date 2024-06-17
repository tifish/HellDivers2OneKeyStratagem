public class HttpHelper
{
    public static async Task<DateTime?> GetHttpFileTime(string url)
    {
        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return null;

        if (!response.Content.Headers.TryGetValues("Last-Modified", out var values))
            return null;

        var dateString = values.FirstOrDefault();
        if (DateTime.TryParse(dateString, out var lastModified))
            return lastModified;

        return null;
    }

    public static async Task<string> DownloadFile(string url, string downloadDirectory)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);

        // get file name from respond
        var contentDisposition = response.Content.Headers.ContentDisposition;
        var fileName = contentDisposition?.FileName?.Trim('"');
        if (string.IsNullOrEmpty(fileName))
            fileName = Path.GetFileName(url);

        var filePath = Path.Combine(downloadDirectory, fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);

        return filePath;
    }
}
