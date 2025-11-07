using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;

namespace Y2025.D04;

// https://everybody.codes/event/2025/quests/4
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        var first = int.Parse(input[..input.IndexOf('\n')]);
        var last = int.Parse(input[(input.LastIndexOf('\n') + 1)..]);
        return (first * 2025 / last).ToString();
    }

    public string Solve2(ReadOnlySpan<char> input)
    {
        var first = double.Parse(input[..input.IndexOf('\n')]);
        var last = double.Parse(input[(input.LastIndexOf('\n') + 1)..]);
        return Math.Ceiling(10000000000000.0 / (first / last)).ToString();
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}
