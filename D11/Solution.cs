using ZLinq;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Runtime.CompilerServices;

namespace Y2025.D11;

// https://everybody.codes/event/2025/quests/11
public sealed class Solution : ISolution
{
    // https://everybody.codes/event/2025/quests/11#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    {
        var birds = (stackalloc long[input.Count('\n') + 1]);
        input.Split('\n').ParseTo<long>().AsValueEnumerable().CopyTo(birds);
        var rounds = Phase1(birds);
        Phase2(birds, maxDepth: (int)(10 - rounds));
        return Checksum(birds).ToString();

        static void Phase2(Span<long> birds, int maxDepth)
        {
            if (maxDepth <= 0)
                return;
            var moved = false;
            for (var i = 1; i < birds.Length; i++)
            {
                if (birds[i - 1] < birds[i])
                {
                    moved = true;
                    birds[i - 1]++;
                    birds[i]--;
                }
            }
            if (moved)
                Phase2(birds, maxDepth - 1);
        }

        static long Checksum(ReadOnlySpan<long> birds)
        => birds.AsValueEnumerable()
            .Index()
            .Sum(b => (b.Index + 1) * b.Item);
    }

    static long Phase1(Span<long> birds)
    {
        var rounds = 0L;
        for (var moved = true; moved; rounds++)
        {
            moved = false;
            for (var i = 1; i < birds.Length; i++)
            {
                if (birds[i - 1] > birds[i])
                {
                    moved = true;
                    birds[i - 1]--;
                    birds[i]++;
                }
            }
        }
        return rounds - 1;
    }

    // https://everybody.codes/event/2025/quests/11#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        var birds = (stackalloc long[input.Count('\n') + 1]);
        input.Split('\n').ParseTo<long>().AsValueEnumerable().CopyTo(birds);
        long rounds = Phase1(birds);
        rounds += Phase2(birds);
        return rounds.ToString();

        static long Phase2(Span<long> birds)
        {
            var rounds = 0;
            for (var moved = true; moved; rounds++)
            {
                moved = false;
                for (var i = 1; i < birds.Length; i++)
                {
                    if (birds[i - 1] < birds[i])
                    {
                        moved = true;
                        birds[i - 1]++;
                        birds[i]--;
                    }
                }
            }

            return rounds - 1;
        }
    }

    // https://everybody.codes/event/2025/quests/11#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        var birds = (stackalloc long[input.Count('\n') + 1]);
        input.Split('\n').ParseTo<long>().AsValueEnumerable().CopyTo(birds);
        var avg = birds.AsValueEnumerable().Average();
        return birds.AsValueEnumerable()
            .Where(b => b < avg)
            .Sum(b => avg - b)
            .ToString();
    }
}

file static class Ext
{
    extension<T>(Extension.SpanSplitCastEnumerator<T> values)
    {
        public ValueEnumerable<FromSpanSplitCastEnumerator<T>, T> AsValueEnumerable()
        => new(new(values));
    }

    public ref struct FromSpanSplitCastEnumerator<T>(Extension.SpanSplitCastEnumerator<T> source) : IValueEnumerator<T>, IDisposable
    {
        private Extension.SpanSplitCastEnumerator<T> source = source;

        public readonly bool TryGetNonEnumeratedCount(out int count)
        {
            Unsafe.SkipInit(out count);
            return false;
        }

        public readonly bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            Unsafe.SkipInit(out span);
            return false;
        }

        public readonly bool TryCopyTo(scoped Span<T> destination, Index offset)
        => false;

        public bool TryGetNext(out T current)
        {
            if (source.MoveNext())
            {
                current = source.Current;
                return true;
            }

            Unsafe.SkipInit(out current);
            return false;
        }

        public readonly void Dispose()
        { }
    }
}
