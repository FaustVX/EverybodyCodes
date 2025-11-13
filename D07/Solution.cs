using System.Collections.Immutable;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using ZLinq;

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
        foreach (var name in names.Split(',').ToSpans())
            if (IsValidName(rules, name))
                return name.ToString();
        throw new NotImplementedException();
    }

    // https://everybody.codes/event/2025/quests/7#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        var nl = input.IndexOf("\n\n");
        var names = input[..nl];
        var rules = CreateRules(input[(nl + 2)..]);
        var i = 1;
        var sum = 0;
        foreach (var name in names.Split(',').ToSpans())
        {
            if (IsValidName(rules, name))
                sum += i;
            i++;
        }
        return sum.ToString();
    }

    // https://everybody.codes/event/2025/quests/7#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        var nl = input.IndexOf("\n\n");
        var names = input[..nl];
        var rules = CreateRules(input[(nl + 2)..]);
        var set = new HashSet<string>(capacity: 10_000);
        foreach (var name in names.Split(',').ToSpans())
            if (IsValidName(rules, name))
                GenerateNames(name.ToImmutableArray(), set, rules);
        return set.Count.ToString();

        static int GenerateNames(ImmutableArray<char> name, HashSet<string> names, IReadOnlyDictionary<char, IReadOnlySet<char>> rules)
        {
            if (name.Length < 11)
                foreach (var c in rules.TryGet(name[^1]) ?? _empty)
                {
                    if (name.Length >= 6)
                        names.Add(CreateName(name, c));
                    GenerateNames(name.Add(c), names, rules);
                }

            return names.Count;

            static string CreateName(ImmutableArray<char> name, char suffix)
            => string.Create(name.Length + 1, (name, suffix), (s, i) =>
            {
                foreach (var c in i.name)
                {
                    s[0] = c;
                    s = s[1..];
                }
                s[0] = i.suffix;
            });
        }
    }

    static readonly IReadOnlySet<char> _empty = new HashSet<char>(capacity: 0);

    static bool IsValidName(IReadOnlyDictionary<char, IReadOnlySet<char>> rules, ReadOnlySpan<char> name)
    {
        var found = true;
        for (var i = 1; i < name.Length; i++)
            if (!rules[name[i - 1]].Contains(name[i]))
            {
                found = false;
                break;
            }
        if (found)
            return true;
        return false;
    }

    static IReadOnlyDictionary<char, IReadOnlySet<char>> CreateRules(ReadOnlySpan<char> input)
    {
        var rules = new Dictionary<char, IReadOnlySet<char>>(capacity: input.Count('\n') + 1);
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

file static class Ext
{
    extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dict)
    {
        public TValue? TryGet(TKey key)
        => CollectionExtensions.GetValueOrDefault(dict, key);
    }
}
