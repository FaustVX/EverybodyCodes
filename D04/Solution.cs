using ZLinq;
using EverybodyCodes.Core;

namespace Y2024.D04;

// https://everybody.codes/event/2024/quests/4
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        var ranges = (stackalloc Range[input.Count('\n') + 1]);
        var nails = (stackalloc int[ranges.Length]);
        input.Split(ranges, '\n');
        for (int i = 0; i < ranges.Length; i++)
            nails[i] = int.Parse(input[ranges[i]]);
        var min = nails.AsValueEnumerable().Min();
        foreach (ref var nail in nails)
            nail -= min;
        return nails.AsValueEnumerable().Sum().ToString();
    }

    public string Solve2(ReadOnlySpan<char> input)
    => Solve1(input);

    public string Solve3(ReadOnlySpan<char> input)
    {
        var ranges = (stackalloc Range[input.Count('\n') + 1]);
        var nails = (stackalloc int[ranges.Length]);
        input.Split(ranges, '\n');
        for (int i = 0; i < ranges.Length; i++)
            nails[i] = int.Parse(input[ranges[i]]);
        var min = nails.AsValueEnumerable().Min();
        foreach (ref var nail in nails)
            nail -= min;
        nails.Sort();
        var median = nails[nails.Length / 2];
        foreach (ref var nail in nails)
            nail = Math.Abs(nail - median);

        return nails.AsValueEnumerable().Sum().ToString();
    }
}
