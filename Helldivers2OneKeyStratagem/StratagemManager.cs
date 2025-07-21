using System.Diagnostics.CodeAnalysis;

namespace Helldivers2OneKeyStratagem;

public static class StratagemManager
{
    public static readonly Dictionary<StratagemType, List<Stratagem>> Groups = [];

    private static readonly Dictionary<string, Stratagem> _stratagemDictionary = [];
    public static List<Stratagem> Stratagems { get; private set; } = [];
    private static readonly Dictionary<string, string> _systemAliasesDictionary = [];
    public static IEnumerable<string> StratagemAlias => _stratagemDictionary.Keys.Concat(_userAliasStratagemDictionary.Keys);
    public static int Count => Stratagems.Count;

    private static readonly string StratagemsFile = Path.Combine(AppSettings.DataDirectory, "Stratagems.tab");

    public static void Load()
    {
        LoadStratagems();
        LoadUserAliases();
    }

    private static readonly Dictionary<string, string> _userAliasesDictionary = [];
    private static readonly Dictionary<string, Stratagem> _userAliasStratagemDictionary = [];

    private static void LoadStratagems()
    {
        if (!File.Exists(StratagemsFile))
            return;

        var nameColumn = Settings.SpeechLanguage switch
        {
            "zh" => 1,
            "en" => 2,
            _ => 2,
        };

        Groups.Clear();
        Stratagems.Clear();
        _stratagemDictionary.Clear();
        _systemAliasesDictionary.Clear();

        foreach (var line in File.ReadLines(StratagemsFile).Skip(1))
        {
            var items = line.Split('\t');
            if (items.Length != 6)
                throw new InvalidOperationException($"Invalid line: {line}");

            if (items[0] == "")
                continue;

            var stratagem = new Stratagem
            {
                KeySequence = items[0],
                RawIconName = items[3],
            };
            if (Enum.TryParse<StratagemColor>(items[4], out var color))
                stratagem.Color = color;
            if (Enum.TryParse<StratagemType>(items[5], out var type))
                stratagem.Type = type;

            var names = items[nameColumn].Split('|');
            stratagem.Name = names[0];

            var enNames = items[2].Split('|');
            stratagem.Id = enNames[0];

            Stratagems.Add(stratagem);
            _systemAliasesDictionary[stratagem.Name] = items[nameColumn];

            foreach (var name in names)
                if (name != "")
                    _stratagemDictionary.Add(name, stratagem);
        }

        GroupStratagems();
    }

    private static void GroupStratagems()
    {
        Groups.Clear();

        foreach (var stratagemColor in Enum.GetValues<StratagemColor>())
        {
            foreach (var stratagem in Stratagems)
            {
                if (stratagem.Color != stratagemColor)
                    continue;

                if (Groups.TryGetValue(stratagem.Type, out var group))
                    group.Add(stratagem);
                else
                    Groups[stratagem.Type] = [stratagem];
            }
        }
    }

    private static readonly string UserAliasesFile = Path.Combine(AppSettings.SettingsDirectory, "UserAliases.tab");

    private static void LoadUserAliases()
    {
        _userAliasesDictionary.Clear();

        if (!File.Exists(UserAliasesFile))
            return;

        foreach (var line in File.ReadLines(UserAliasesFile).Skip(1))
        {
            var items = line.Split('\t');
            if (items.Length != 2)
                throw new InvalidOperationException($"Invalid line: {line}");

            _userAliasesDictionary[items[0]] = items[1];
        }

        UpdateUserAliases();
    }

    private static void UpdateUserAliases()
    {
        _userAliasStratagemDictionary.Clear();

        foreach (var (name, aliasesString) in _userAliasesDictionary)
        {
            if (!TryGet(name, out var stratagem))
                continue;

            var aliases = aliasesString.Split('|').Where(item => item != "");
            foreach (var alias in aliases)
                _userAliasStratagemDictionary[alias] = stratagem;
        }
    }

    private static void SaveUserAliases()
    {
        var lines = new List<string> { "Name\tAliases" };
        foreach (var (name, alias) in _userAliasesDictionary)
            lines.Add($"{name}\t{alias}");

        var dir = Path.GetDirectoryName(UserAliasesFile);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllLines(UserAliasesFile, lines);
    }

    public static bool TryGet(string nameOrAlias, [NotNullWhen(true)] out Stratagem? stratagem)
    {
        return _stratagemDictionary.TryGetValue(nameOrAlias, out stratagem)
               || _userAliasStratagemDictionary.TryGetValue(nameOrAlias, out stratagem);
    }

    public static string GetSystemAlias(string stratagemName)
    {
        return _systemAliasesDictionary.GetValueOrDefault(stratagemName, "");
    }

    public static string GetUserAlias(string stratagemName)
    {
        return _userAliasesDictionary.GetValueOrDefault(stratagemName, "");
    }

    public static void SetUserAlias(string stratagemName, string alias)
    {
        if (alias == "")
            _userAliasesDictionary.Remove(stratagemName);
        else
            _userAliasesDictionary[stratagemName] = alias;

        UpdateUserAliases();
        SaveUserAliases();
    }
}
