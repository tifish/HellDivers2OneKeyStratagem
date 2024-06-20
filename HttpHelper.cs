using System.Net;
using System.Net.Http.Headers;

public class HttpHelper
{
    public class HttpHeaders(HttpResponseHeaders responseHeaders)
    {
        public DateTime? LastModified => GetDateTime("Last-Modified");
        public int FileSize => GetInt("Content-Length") ?? -1;

        public DateTime? GetDateTime(string header)
        {
            if (!responseHeaders.TryGetValues(header, out var values))
                return null;

            var dateString = values.FirstOrDefault();

            if (DateTime.TryParse(dateString, out var lastModified))
                return lastModified;

            return null;
        }

        public int? GetInt(string header)
        {
            if (!responseHeaders.TryGetValues(header, out var values))
                return null;

            var lengthString = values.FirstOrDefault();
            if (int.TryParse(lengthString, out var length))
                return length;

            return null;
        }
    }

    public static async Task<HttpHeaders?> GetHeaders(string url)
    {
        var respondHeaders = await GetHttpRespondHeaders(url);
        if (respondHeaders == null)
            return null;

        return new HttpHeaders(respondHeaders);
    }

    private static HttpClient GetHttpClient()
    {
        var client = new HttpClient();
        // set timeout
        client.Timeout = TimeSpan.FromSeconds(10);
        // act like a Chrome browser
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        return client;
    }

    private static async Task<HttpResponseHeaders?> GetHttpRespondHeaders(string url)
    {
        try
        {
            using var client = GetHttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                var redirectUrl = response.Headers.Location?.ToString();
                if (string.IsNullOrEmpty(redirectUrl))
                    return null;

                return await GetHttpRespondHeaders(redirectUrl);
            }

            return response.Headers;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public static async Task<string> DownloadFile(string url, string downloadDirectory)
    {
        using var client = GetHttpClient();
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
