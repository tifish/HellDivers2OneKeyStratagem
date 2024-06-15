using System.Diagnostics.CodeAnalysis;

namespace HellDivers2OneKeyStratagem;

public static class StratagemManager
{
    public static readonly Dictionary<string, List<Stratagem>> Groups = [];

    private static readonly Dictionary<string, Stratagem> _stratagemDictionary = [];
    public static IEnumerable<Stratagem> Stratagems => _stratagemDictionary.Values;
    private static readonly Dictionary<string, string> _systemAliasesDictionary = [];
    public static IEnumerable<string> StratagemAlias => _stratagemDictionary.Keys.Concat(_userAliasStratagemDictionary.Keys);
    public static int Count => _stratagemDictionary.Count;

    private static readonly string StratagemsFile = Path.Combine(AppSettings.ExeDirectory, "Stratagems.tab");

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

        var langName = Settings.Language[..2];

        var nameColumn = langName switch
        {
            "zh" => 1,
            "en" => 2,
            _ => 2,
        };

        Groups.Clear();
        _stratagemDictionary.Clear();
        _systemAliasesDictionary.Clear();
        List<Stratagem>? currentGroup = null;

        foreach (var line in File.ReadLines(StratagemsFile).Skip(1))
        {
            var items = line.Split('\t');
            if (items.Length != 3)
                throw new InvalidOperationException($"Invalid line: {line}");

            if (items[0] != "")
            {
                if (currentGroup == null)
                    throw new InvalidOperationException($"No group found for stratagem {items[nameColumn]}");

                var stratagem = new Stratagem { Name = items[nameColumn], KeySequence = items[0] };
                var names = stratagem.Name.Split('|');
                stratagem.Name = names[0];
                _systemAliasesDictionary[stratagem.Name] = items[nameColumn];

                currentGroup.Add(stratagem);

                foreach (var name in names)
                    if (name != "")
                        _stratagemDictionary.Add(name, stratagem);
            }
            else
            {
                currentGroup = [];
                Groups.Add(items[nameColumn], currentGroup);
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
