using ZLinq;
using EverybodyCodes.Core;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core.Attributes;
using CommunityToolkit.HighPerformance.Enumerables;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Y2024.D02;

using System;

public sealed class Solution : ISolution
{
    // https://everybody.codes/event/2024/quests/2#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    {
        input = input[6..];
        var ranges = (stackalloc Range[2]);
        input.Split(ranges, "\n\n");
        var words = input[ranges[0]];
        var text = input[ranges[1]];
        ranges = (stackalloc Range[words.Count(',') + 1]);
        words.Split(ranges, ',');
        var count = 0;
        for (int i = 0; i < text.Length; i++)
            if (StartWith(text[i..], words, ranges))
                count++;
        return count.ToString();

        static bool StartWith(ReadOnlySpan<char> text, ReadOnlySpan<char> words, ReadOnlySpan<Range> ranges)
        {
            foreach (var range in ranges)
                if (text.StartsWith(words[range]))
                    return true;
            return false;
        }
    }

    // https://everybody.codes/event/2024/quests/2#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        input = input[6..];
        var ranges = (stackalloc Range[2]);
        input.Split(ranges, "\n\n");
        var words = input[ranges[0]];
        var text = input[ranges[1]];
        ranges = (stackalloc Range[words.Count(',') + 1]);
        words.Split(ranges, ',');
        ranges.AsValueEnumerable().OrderByDescending(r => r.Length).CopyTo(ranges);
        var count = 0;
        Range last = default;
        for (int i = 0; i < text.Length; i++)
            if (StartWith(text[i..], words, ranges) is not 0 and var l)
                if (last.IsOverlapping(Range.FromStartWithLength(i, l)))
                    last = last.CreateOverlap(Range.FromStartWithLength(i, l));
                else
                {
                    count += last.Length;
                    last = Range.FromStartWithLength(i, l);
                }
        count += last.Length;
        return count.ToString();

        static int StartWith(ReadOnlySpan<char> text, ReadOnlySpan<char> words, ReadOnlySpan<Range> ranges)
        {
            foreach (var range in ranges)
                if (text.StartsWith(words[range]) || text.StartsWithReversed(words[range]))
                    return words[range].Length;
            return default;
        }
    }

    // https://everybody.codes/event/2024/quests/2#:~:text=Part%20III
    [AddFinalLineFeed]
    public string Solve3(ReadOnlySpan<char> input)
    {
        input = input[6..];
        var ranges = (stackalloc Range[2]);
        input.Split(ranges, "\n\n");
        var words = input[ranges[0]];
        var text = input[ranges[1]];
        ranges = (stackalloc Range[words.Count(',') + 1]);
        words.Split(ranges, ',');
        ranges.AsValueEnumerable().OrderByDescending(r => r.Length).CopyTo(ranges);
        var span2D = text.AsSpan2D('\n');
        var set = new HashSet<(int x, int y)>();

        for (var y = 0; y < span2D.Height; y++)
            for (var x = 0; x < span2D.Width; x++)
                foreach (var range in ranges)
                {
                    var row = span2D.GetRow(y);
                    if (row.StartsWithWraped(x, words[range])
                    || row.StartsWithWraped(x, words[range].Reversed()))
                        for (var i = 0; i < range.Length; i++)
                            set.Add(((x + i) % row.Length, y));
                    var column = span2D[y.., ..].GetColumn(x);
                    if (column.StartsWith(words[range])
                    || column.StartsWith(words[range].Reversed()))
                        for (var i = 0; i < range.Length; i++)
                            set.Add((x, y + i));
                }
        return set.Count.ToString();
    }
}

[System.Diagnostics.DebuggerStepThrough]
file static class Ext
{
    extension<T>(ReadOnlyRefEnumerable<T> span)
    where T : unmanaged, IEquatable<T>
    {
        /// <inheritdoc cref="MemoryExtensions.StartsWith{T}(ReadOnlySpan{T}, ReadOnlySpan{T})"/>
        public bool StartsWith(ReadOnlySpan<T> value)
        {
            if (span.Length < value.Length)
                return false;
            for (var i = 0; i < value.Length; i++)
                if (!span[i].Equals(value[i]))
                    return false;
            return true;
        }

        public bool StartsWithWraped(int startIndex, ReadOnlySpan<T> value)
        {
            for (var i = 0; i < value.Length; i++)
                if (!span[(i + startIndex) % span.Length].Equals(value[i]))
                    return false;
            return true;
        }

        public ReadOnlyRefEnumerable<T> GetRange(Range range)
        => ReadOnlyRefEnumerable<T>.DangerousCreate(in span[range.Start], range.GetOffsetAndLength(span.Length).Length * span.GetStep(), span.GetStep());
    }

    extension<T>(ref readonly ReadOnlyRefEnumerable<T> span)
    where T : unmanaged, IEquatable<T>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "step")]
        public extern ref int GetStep();
    }

    extension<T>(ReadOnlySpan<T> span)
    where T : unmanaged
    {
        /// <inheritdoc cref="MemoryExtensions.StartsWith{T}(ReadOnlySpan{T}, ReadOnlySpan{T})"/>
        public bool StartsWithReversed(ReadOnlySpan<T> value)
        {
            var reversed = (stackalloc T[value.Length]);
            value.Reverse(reversed);
            return span.StartsWith(reversed);
        }

        public void Reverse(Span<T> destination)
        {
            span.CopyTo(destination);
            destination.Reverse();
        }

        public ReadOnlySpan<T> Reversed()
        {
            Unsafe.AsSpan(span).Reverse();
            return span;
        }

        public ReadOnlySpan2D<T> AsSpan2D(T delimitor)
        => span.AsSpan2D(span.Count(delimitor), span.IndexOf(delimitor) + 1)[.., ..^1];

        public ReadOnlyRefEnumerable<T> AsReadOnlyRefEnumerable()
        => ReadOnlyRefEnumerable<T>.DangerousCreate(in span[0], span.Length, 1);
    }

    extension(Unsafe)
    {
        public static Span<T> AsSpan<T>(ReadOnlySpan<T> span)
        => MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in span[0]), span.Length);
    }

    extension(Range range)
    {
        public bool IsOverlapping(Range other)
        {
            int start1 = range.Start.IsFromEnd ? int.MaxValue - range.Start.Value : range.Start.Value;
            int end1 = range.End.IsFromEnd ? int.MaxValue - range.End.Value : range.End.Value;
            int start2 = other.Start.IsFromEnd ? int.MaxValue - other.Start.Value : other.Start.Value;
            int end2 = other.End.IsFromEnd ? int.MaxValue - other.End.Value : other.End.Value;
            return start1 < end2 && start2 < end1;
        }

        public Range CreateOverlap(Range other)
        {
            int start1 = range.Start.IsFromEnd ? int.MaxValue - range.Start.Value : range.Start.Value;
            int end1 = range.End.IsFromEnd ? int.MaxValue - range.End.Value : range.End.Value;
            int start2 = other.Start.IsFromEnd ? int.MaxValue - other.Start.Value : other.Start.Value;
            int end2 = other.End.IsFromEnd ? int.MaxValue - other.End.Value : other.End.Value;

            int overlapStart = Math.Min(start1, start2);
            int overlapEnd = Math.Max(end1, end2);

            if (overlapStart < overlapEnd)
                return new Range(overlapStart, overlapEnd);
            return new Range(0, 0);
        }

        public int Length
        {
            get
            {
                int start = range.Start.IsFromEnd ? int.MaxValue - range.Start.Value : range.Start.Value;
                int end = range.End.IsFromEnd ? int.MaxValue - range.End.Value : range.End.Value;
                return Math.Max(0, end - start);
            }
        }

        public static Range FromStartWithLength(Index start, int length)
        {
            int startValue = start.IsFromEnd ? int.MaxValue - start.Value : start.Value;
            int endValue = startValue + length;
            return new Range(start, new Index(endValue, false));
        }
    }
}
