using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

public class JsonFile<T> where T : class
{
    public string FilePath { get; set; }

    public JsonFile(string filePath)
    {
        FilePath = filePath;
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        IgnoreReadOnlyProperties = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public T? Load()
    {
        if (!File.Exists(FilePath))
            return null;

        using var fileStream = File.OpenRead(FilePath);
        return JsonSerializer.Deserialize<T>(fileStream, JsonSerializerOptions);
    }

    public async Task<T?> LoadAsync()
    {
        if (!File.Exists(FilePath))
            return null;

        await using var fileStream = File.OpenRead(FilePath);
        return await JsonSerializer.DeserializeAsync<T>(fileStream, JsonSerializerOptions);
    }

    public void Save(T obj)
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var fileStream = File.Create(FilePath);
        JsonSerializer.Serialize(fileStream, obj, JsonSerializerOptions);
    }

    public async Task SaveAsync(T obj)
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await using var fileStream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(fileStream, obj, JsonSerializerOptions);
    }
}
