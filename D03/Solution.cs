using ZLinq;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Runtime.InteropServices;

namespace Y2025.D03;

// https://everybody.codes/event/2025/quests/3
public sealed class Solution : ISolution
{
    // https://everybody.codes/event/2025/quests/3#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    {
        var set = new HashSet<int>();
        foreach (var number in input.Split(',').ParseTo<int>())
            set.Add(number);
        return set.AsValueEnumerable()
            .Sum()
            .ToString();
    }

    // https://everybody.codes/event/2025/quests/3#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        var set = new HashSet<int>();
        foreach (var number in input.Split(',').ParseTo<int>())
            set.Add(number);
        return set.AsValueEnumerable()
            .Order()
            .Take(20)
            .Sum()
            .ToString();
    }

    // https://everybody.codes/event/2025/quests/3#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        var dict = new Dictionary<int, int>();
        foreach (var number in input.Split(',').ParseTo<int>())
            CollectionsMarshal.GetValueRefOrAddDefault(dict, number, out _)++;

        return dict.Max(kvp => kvp.Value).ToString();
    }
}
