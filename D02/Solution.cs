using System.Diagnostics;
using System.Runtime.CompilerServices;
using EverybodyCodes.Core;
using ZLinq;

namespace Y2.D02;

// https://everybody.codes/story/2/quests/2
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        for (var i = 0;; i++)
        {
            input = (i % 3) switch
            {
                0 => input.TrimStart('R'),
                1 => input.TrimStart('G'),
                2 => input.TrimStart('B'),
                _ => throw new UnreachableException(),
            };
            if (input.IsEmpty || (input = input[1..]).IsEmpty)
                return (i + 1).ToString();
        }
    }

    public string Solve2(ReadOnlySpan<char> input)
    => Solve(input, Globals.IsTest ? 5 : 100).ToString();

    public string Solve3(ReadOnlySpan<char> input)
    => Solve(input, 100_000).ToString();

    static long Solve(ReadOnlySpan<char> input, int repeat)
    {
        input = string.Create(input.Length * repeat, input, (b, s) =>
        {
            while (!b.IsEmpty)
            {
                s.CopyTo(b);
                b = b[s.Length..];
            }
        });
        var firsts = input.AsValueEnumerable()
            .Take(input.Length / 2)
            .ToQueue();
        var lasts = input.AsValueEnumerable()
            .Skip(firsts.Count)
            .ToQueue();

        for (var i = 0;; i++)
        {
            var fluff = (i % 3) switch
            {
                0 => 'R',
                1 => 'G',
                2 => 'B',
                _ => throw new UnreachableException(),
            };
            if (fluff == firsts.Peek())
            {
                firsts.Dequeue();
                lasts.Dequeue();
            }
            else
            {
                firsts.Dequeue();
                Transfert(lasts, firsts);
                firsts.Dequeue();
                i++;
            }
            if (firsts.Count == 0)
                return i + 1;
        }

        static void Transfert<T>(Queue<T> from, Queue<T> to)
        => to.Enqueue(from.Dequeue());
    }
}

file static class Ext
{
    extension<TEnumerator, TSource>(ValueEnumerable<TEnumerator, TSource> source)
    where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
    {
        public Queue<TSource> ToQueue()
        {
            using var val = source.Enumerator;
            if (val.TryGetSpan(out var span))
            {
                var queue = new Queue<TSource>(span.Length);
                Q<TSource>.GetSize(queue) = span.Length;
                span.CopyTo(Q<TSource>.GetArray(queue));
                return queue;
            }

            var queue2 = val.TryGetNonEnumeratedCount(out var count) ? new Queue<TSource>(count) : new Queue<TSource>();
            while (val.TryGetNext(out var current))
            {
                queue2.Enqueue(current);
            }

            return queue2;
        }
    }

    static class Q<T>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_array")]
        public static extern ref readonly T[] GetArray(Queue<T> queue);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_size")]
        public static extern ref int GetSize(Queue<T> queue);
    }
}
