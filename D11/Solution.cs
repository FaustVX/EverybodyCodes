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
        var birds = (stackalloc int[input.Count('\n') + 1]);
        input.Split('\n').ParseTo<int>().AsValueEnumerable().CopyTo(birds);
        var rounds = Phase1(birds);
        Phase2(birds, maxDepth: 10 - rounds);
        return Checksum(birds).ToString();

        static int Phase1(Span<int> birds)
        {
            var moved = false;
            for (var i = 1; i < birds.Length; i++)
            {
                if (birds[i - 1] > birds[i])
                {
                    moved = true;
                    birds[i - 1]--;
                    birds[i]++;
                }
            }
            return moved ? 1 + Phase1(birds) : 0;
        }

        static void Phase2(Span<int> birds, int maxDepth)
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

        static int Checksum(ReadOnlySpan<int> birds)
        => birds.AsValueEnumerable()
            .Index()
            .Sum(b => (b.Index + 1) * b.Item);
    }

    // https://everybody.codes/event/2025/quests/11#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }

    // https://everybody.codes/event/2025/quests/11#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
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
