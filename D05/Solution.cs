using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Diagnostics;

namespace Y2025.D05;

using System;

// https://everybody.codes/event/2025/quests/5
public sealed class Solution : ISolution
{
    // https://everybody.codes/event/2025/quests/5#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    => Segment.Parse(input).Quality.ToString();

    // https://everybody.codes/event/2025/quests/5#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        var (min, max) = (long.MaxValue, long.MinValue);
        foreach (var line in input.Split('\n'))
        {
            var quality = Segment.Parse(input[line]).Quality;
            if (quality < min)
                min = quality;
            if (quality > max)
                max = quality;
        }
        return (max - min).ToString();
    }

    // https://everybody.codes/event/2025/quests/5#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        var swords = new List<Segment>(capacity: input.Count('\n') + 1);
        foreach (var line in input.Split('\n'))
            swords.Add(Segment.Parse(input[line]));
        swords.Sort((l, r) =>
        {
            if (l.Quality.CompareTo(r.Quality) is not 0 and var c)
                return -c;
            for(var (a, b) = (l, r); (a, b) is (not null, not null); (a, b) = (a.Next, b.Next))
                if (a.LocalValue.CompareTo(b.LocalValue) is not 0 and var v)
                    return -v;
            return -l.ID.CompareTo(r.ID);
        });
        return swords.Index().Sum(s => (s.Index + 1L) * s.Item.ID).ToString();
    }
}

file sealed record class Segment(int ID, int Central)
{
    public int? Left { get; set; }
    public int? Right { get; set; }
    public Segment? Next { get; set; }

    public static Segment Parse(ReadOnlySpan<char> input)
    {
        var id = int.Parse(input[..input.IndexOf(':')]);
        input = input[(input.IndexOf(':') + 1)..];
        var spine = new Segment(id, int.Parse(input[..input.IndexOf(',')]));
        foreach (var r in input.Split(',').Skip())
            spine.Add(int.Parse(input[r]));
        return spine;
    }

    public void Add(int number)
    {
        if (number < Central && Left is null)
            Left = number;
        else if (number > Central && Right is null)
            Right = number;
        else if (Next is null)
            Next = new(ID, number);
        else
            Next.Add(number);
    }

    public long Quality
    => Next is { } next ? ((long)Central).Concat(next.Quality) : Central;

    public int LocalValue
    => this switch
    {
        { Left: int left, Right: int right } => left.Concat(Central).Concat(right),
        { Left: null, Right: null } => Central,
        { Left: int left } => left.Concat(Central),
        { Right: int right } => Central.Concat(right),
        _ => throw new UnreachableException(),
    };

    public override string ToString()
    => $"{Quality}";
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

    extension<T>(T n)
    where T : System.Numerics.IBinaryInteger<T>
    {
        public int GetDecimalLength()
        {
            if (T.IsZero(n))
                return 1;
            n = T.Abs(n);
            var ten = T.CreateChecked(10);
            var length = 0;
            while (n > T.Zero)
            {
                n /= ten;
                length++;
            }
            return length;
        }

        public T Concat(T other)
        {
            if (T.IsZero(n))
                return other;
            if (T.IsZero(other))
                return n;
            var length = other.GetDecimalLength();
            var ten = T.CreateChecked(10);
            var mul = T.One;
            for (var i = 0; i < length; i++)
                mul *= ten;
            return n * mul + other;
        }
    }
}
