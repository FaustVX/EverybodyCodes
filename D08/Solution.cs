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
        throw new NotImplementedException();
    }

    // https://everybody.codes/event/2025/quests/8#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
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
