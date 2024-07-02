using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;

namespace HellDivers2OneKeyStratagem;

public class Localizer : INotifyPropertyChanged
{
    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";
    private Dictionary<string, string[]> _languageStrings = [];

    private static readonly string LanguagesTabFile = Path.Combine(AppSettings.DataDirectory, "Languages.tab");

    private bool _hasLoaded;

    public List<string> Languages { get; } = [];

    public bool Load()
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

    private int _currentLanguageIndex = -1;

    public bool SetLanguage(string language)
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

    public string Language { get; private set; } = "";

    public string this[string key]
    {
        get
        {
            if (_languageStrings.TryGetValue(key, out var res))
                return res[_currentLanguageIndex].Replace("\\n", "\n");

            return $"{Language}:{key}";
        }
    }

    public static Localizer Instance { get; set; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Invalidate()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }
}
