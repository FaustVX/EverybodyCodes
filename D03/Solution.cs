using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;

namespace Y2025.D03;

// https://everybody.codes/event/2025/quests/3
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        var set = new HashSet<int>();
        foreach (var number in input.Split(','))
            set.Add(int.Parse(input[number]));
        return set.AsValueEnumerable()
            .Sum()
            .ToString();
    }

    public string Solve2(ReadOnlySpan<char> input)
    {
        var set = new HashSet<int>();
        foreach (var number in input.Split(','))
            set.Add(int.Parse(input[number]));
        return set.AsValueEnumerable()
            .Order()
            .Take(20)
            .Sum()
            .ToString();
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}
