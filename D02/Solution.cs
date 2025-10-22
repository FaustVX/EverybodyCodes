using ZLinq;
using EverybodyCodes.Core;

namespace Y2024.D02;

public sealed class Solution : ISolution
{
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

    public string Solve2(ReadOnlySpan<char> input)
    {
        input = input[6..];
        var ranges = (stackalloc Range[2]);
        input.Split(ranges, "\n\n");
        var words = input[ranges[0]];
        var text = input[ranges[1]];
        ranges = (stackalloc Range[words.Count(',') + 1]);
        words.Split(ranges, ',');
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
                if (text.StartsWith(words[range]) || text.StartWithReversed(words[range]))
                    return words[range].Length;
            return default;
        }
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}

file static class Ext
{
    extension<T>(ReadOnlySpan<T> span)
    where T : unmanaged, IEquatable<T>
    {
        /// <inheritdoc cref="MemoryExtensions.StartsWith{T}(ReadOnlySpan{T}, ReadOnlySpan{T})"/>
        public bool StartWithReversed(ReadOnlySpan<T> value)
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
