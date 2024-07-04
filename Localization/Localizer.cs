using System.Text;

namespace HellDivers2OneKeyStratagem;

public static class Localizer
{
    private static readonly Dictionary<string, string[]> _languageStrings = [];

    private static readonly string LanguagesTabFile = Path.Combine(AppSettings.DataDirectory, "Languages.tab");

    private static bool _hasLoaded;

    public static List<string> Languages { get; } = [];

    public static bool Load()
    {
        _languageStrings.Clear();

        if (!File.Exists(LanguagesTabFile))
            return false;

        using var reader = new StreamReader(LanguagesTabFile, Encoding.UTF8);
        var headerLine = reader.ReadLine();
        if (headerLine == null)
            return false;

        var headers = headerLine.Split('\t');
        Languages.Clear();
        Languages.AddRange(headers.Skip(1).ToList());

        var line = reader.ReadLine();
        while (line != null)
        {
            var values = line.Split('\t');
            _languageStrings[values[0]] = values.Skip(1).ToArray();

            line = reader.ReadLine();
        }

        _hasLoaded = true;
        return true;
    }

    private static int _currentLanguageIndex = -1;

    public static bool SetLanguage(string language)
    {
        if (_hasLoaded)
            Load();

        _currentLanguageIndex = Languages.IndexOf(language);
        if (_currentLanguageIndex == -1)
            return false;

        Language = language;

        Invalidate();

        return true;
    }

    public static string Language { get; private set; } = "";

    public static string Get(string key)
    {
        if (_languageStrings.TryGetValue(key, out var res))
            return res[_currentLanguageIndex].Replace("\\n", "\n");

        return $"{Language}:{key}";
    }

    public static event EventHandler? LanguageChanged;

    public static void Invalidate()
    {
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }
}
