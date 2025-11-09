using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Diagnostics;

namespace Y2025.D04;

// https://everybody.codes/event/2025/quests/4
public sealed class Solution : ISolution
{
    // https://everybody.codes/event/2025/quests/4#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    {
        var first = int.Parse(input[..input.IndexOf('\n')]);
        var last = int.Parse(input[(input.LastIndexOf('\n') + 1)..]);
        return (first * 2025 / last).ToString();
    }

    // https://everybody.codes/event/2025/quests/4#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        var first = decimal.Parse(input[..input.IndexOf('\n')]);
        var last = decimal.Parse(input[(input.LastIndexOf('\n') + 1)..]);
        return Math.Ceiling(1e13m / (first / last)).ToString();
    }

    // https://everybody.codes/event/2025/quests/4#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        var previous = decimal.Parse(input[..input.IndexOf('\n')]);
        var ratio = 1m;
        foreach (var range in input.Split('\n').Skip())
        {
            var gear = input[range];
            if (gear.IndexOf('|') is var pipe and not < 0)
            {
                var left = decimal.Parse(gear[..pipe]);
                ratio /= left / previous;
                previous = decimal.Parse(gear[(pipe + 1)..]);
            }
            else
                ratio /= decimal.Parse(gear) / previous;
        }
        return ((long)(ratio * 100)).ToString();
    }
}

file static class Ext
{
    extension<T>(System.MemoryExtensions.SpanSplitEnumerator<T> enumerator)
    where T : IEquatable<T>
    {
        public System.MemoryExtensions.SpanSplitEnumerator<T> Skip(int count = 1)
        {
            enumerator = enumerator.GetEnumerator();
            for (var i = 0; i < count; i++)
                enumerator.MoveNext();
            return enumerator;
        }
    }
}
