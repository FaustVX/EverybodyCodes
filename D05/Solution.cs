using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;

namespace Y2025.D05;

// https://everybody.codes/event/2025/quests/5
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    => GetSwordQuality(input).ToString();

    public string Solve2(ReadOnlySpan<char> input)
    {
        var (min, max) = (long.MaxValue, long.MinValue);
        foreach (var line in input.Split('\n'))
        {
            var quality = GetSwordQuality(input[line]);
            if (quality < min)
                min = quality;
            if (quality > max)
                max = quality;
        }
        return (max - min).ToString();
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }

    static long GetSwordQuality(ReadOnlySpan<char> input)
    {
        input = input[(input.IndexOf(':') + 1)..];
        var spine = new Segment(int.Parse(input[..input.IndexOf(',')]));
        foreach (var r in input.Split(',').Skip())
            spine.Add(int.Parse(input[r]));
        return long.Parse(spine.ToString());
    }
}

file sealed record class Segment(int Central)
{
    public int? Left { get; set; }
    public int? Right { get; set; }
    public Segment? Next { get; set; }

    public void Add(int number)
    {
        if (number < Central && Left is null)
            Left = number;
        else if (number > Central && Right is null)
            Right = number;
        else if (Next is null)
            Next = new(number);
        else
            Next.Add(number);
    }

    public override string ToString()
    => $"{Central}{Next}";
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
