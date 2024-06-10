global using static HellDivers2OneKeyStratagem.StratagemManager;

namespace HellDivers2OneKeyStratagem;

public static class StratagemManager
{
    public static readonly Dictionary<string, List<Stratagem>> StratagemGroups = [];
    public static readonly Dictionary<string, Stratagem> Stratagems = [];
    private const string StratagemsFile = "Stratagems.tab";

    public static async Task Load()
    {
        if (!File.Exists(StratagemsFile))
            return;

        StratagemGroups.Clear();
        Stratagems.Clear();
        List<Stratagem>? currentGroup = null;

        await foreach (var line in File.ReadLinesAsync(StratagemsFile))
        {
            var items = line.Split('\t');
            if (items.Length != 3)
                throw new InvalidOperationException($"Invalid line: {line}");

            if (items[2] != "")
            {
                if (currentGroup == null)
                    throw new InvalidOperationException($"No group found for stratagem {items[0]}");

                var stratagem = new Stratagem { Name = items[0], KeySequence = items[2] };
                currentGroup.Add(stratagem);

                Stratagems.Add(stratagem.Name, stratagem);
                foreach (var name in items[1].Split('|'))
                    if (name != "")
                        Stratagems.Add(name, stratagem);
            }
            else
            {
                currentGroup = [];
                StratagemGroups.Add(line, currentGroup);
            }
        }
    }
}
