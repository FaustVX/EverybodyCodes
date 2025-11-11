using ZLinq;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;

namespace Y2025.D07;

// https://everybody.codes/event/2025/quests/7
public sealed class Solution : ISolution
{
    // https://everybody.codes/event/2025/quests/7#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    {
        var nl = input.IndexOf("\n\n");
        var names = input[..nl];
        var rules = CreateRules(input[(nl + 2)..]);
        foreach (var nameR in names.Split(','))
        {
            var name = names[nameR];
            var found = true;
            for (var i = 1; i < name.Length; i++)
                if (!rules[name[i - 1]].Contains(name[i]))
                {
                    found = false;
                    break;
                }
            if (found)
                return name.ToString();
        }
        throw new NotImplementedException();
    }

    // https://everybody.codes/event/2025/quests/7#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }

    // https://everybody.codes/event/2025/quests/7#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }

    static Dictionary<char, HashSet<char>> CreateRules(ReadOnlySpan<char> input)
    {
        var rules = new Dictionary<char, HashSet<char>>(capacity: input.Count('\n') + 1);
        foreach (var line in input.Split('\n'))
        {
            var set = new HashSet<char>(capacity: input[line].Count(',') + 1);
            foreach (var c in input[line][4..])
                if (c is not ',')
                    set.Add(c);
            rules.Add(input[line][0], set);
        }
        return rules;
    }
}
