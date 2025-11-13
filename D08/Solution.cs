using ZLinq;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;

namespace Y2025.D08;

// https://everybody.codes/event/2025/quests/8
public sealed class Solution : ISolution
{
    // https://everybody.codes/event/2025/quests/8#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    {
        var halfLength = (Globals.IsTest ? 8 : 32) / 2;
        var nails = input.Split(',').CastTo<int>();
        nails.MoveNext();
        var last = nails.Current;
        var count = 0;
        foreach (var nail in nails)
        {
            if (Math.Abs(nail - last) == halfLength)
                count++;
            last = nail;
        }
        return count.ToString();
    }

    // https://everybody.codes/event/2025/quests/8#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        var nails = input.Split(',').CastTo<int>();
        nails.MoveNext();
        var last = nails.Current;
        var count = 0L;
        var lines = (stackalloc (int start, int end)[input.Count(',')]);
        var nextLines = lines;
        foreach (var nail in nails)
        {
            var line = (Math.Min(nail, last), Math.Max(nail, last));
            count += CrossedLines(line, lines);
            nextLines[0] = line;
            nextLines = nextLines[1..];
            last = nail;
        }
        return count.ToString();
    }

    // https://everybody.codes/event/2025/quests/8#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        var lines = (stackalloc (int start, int end)[input.Count(',')]);
        {
            var nails = input.Split(',').CastTo<int>();
            nails.MoveNext();
            var last = nails.Current;
            var nextLines = lines;
            foreach (var nail in nails)
            {
                nextLines[0] = (Math.Min(nail, last), Math.Max(nail, last));
                nextLines = nextLines[1..];
                last = nail;
            }
        }
        var length = Globals.IsTest ? 8 : 256;
        var max = 0L;
        for (var i = 1; i <= length; i++)
            for (var j = i + 1; j <= length; j++)
            {
                var count = CrossedLines((i, j), lines);
                if (lines.Contains((i, j)))
                    count++;
                if (count > max)
                    max = count;
            }
        return max.ToString();
    }

    static int CrossedLines((int start, int end) line, ReadOnlySpan<(int start, int end)> lines)
    => lines.AsValueEnumerable()
        .Count(l => CrossALine(line, l));

    static bool CrossALine((int start, int end) line1, (int start, int end) line2)
    => (line1.start > line2.start && line1.start < line2.end && line1.end > line2.end)
    || (line2.start > line1.start && line2.start < line1.end && line2.end > line1.end);
}

file static class Ext
{
    extension(MemoryExtensions.SpanSplitEnumerator<char> enumerator)
    {
        public Enumerator<T> CastTo<T>()
        where T : ISpanParsable<T>
        => new(enumerator.GetEnumerator());
    }

    public ref struct Enumerator<T>(MemoryExtensions.SpanSplitEnumerator<char> enumerator)
    where T : ISpanParsable<T>
    {
        private MemoryExtensions.SpanSplitEnumerator<char> _enumerator = enumerator;

        public readonly Enumerator<T> GetEnumerator()
        => this;

        public readonly T Current
        => T.Parse(_enumerator.Source[_enumerator.Current], null);

        public bool MoveNext()
        => _enumerator.MoveNext();
    }
}
