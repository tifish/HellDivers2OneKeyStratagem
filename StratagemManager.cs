global using static HellDivers2OneKeyStratagem.StratagemManager;
using System.Globalization;
using System.Speech.Recognition;

namespace HellDivers2OneKeyStratagem;

public static class StratagemManager
{
    public static readonly Dictionary<string, List<Stratagem>> StratagemGroups = [];
    public static readonly Dictionary<string, Stratagem> Stratagems = [];
    private const string StratagemsFile = "Stratagems.tab";

    public static void Load()
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

        StratagemGroups.Clear();
        Stratagems.Clear();
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

                currentGroup.Add(stratagem);

                foreach (var name in names)
                    if (name != "")
                        Stratagems.Add(name, stratagem);
            }
            else
            {
                currentGroup = [];
                StratagemGroups.Add(items[nameColumn], currentGroup);
            }
        }
    }
}
