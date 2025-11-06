using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Diagnostics;

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
    {
        var repeat = Globals.IsTest ? 5 : 100;
        var circle = string.Create(input.Length * repeat, input, (b, s) =>
        {
            while (!b.IsEmpty)
            {
                s.CopyTo(b);
                b = b[s.Length..];
            }
        });
        var firsts = circle.AsValueEnumerable()
            .Take(circle.Length / 2)
            .ToQueue();
        var lasts = circle.AsValueEnumerable()
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
                return (i + 1).ToString();
        }

        static void Transfert<T>(Queue<T> from, Queue<T> to)
        => to.Enqueue(from.Dequeue());
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}

file static class Ext
{
    public static Queue<TSource> ToQueue<TEnumerator, TSource>(this ValueEnumerable<TEnumerator, TSource> source)
    where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
    {
        using var val = source.Enumerator;
        if (val.TryGetSpan(out var span))
        {
            var hashSet = new Queue<TSource>(span.Length);
            var readOnlySpan = span;
            for (var i = 0; i < readOnlySpan.Length; i++)
            {
                TSource item = readOnlySpan[i];
                hashSet.Enqueue(item);
            }

            return hashSet;
        }

        var hashSet2 = val.TryGetNonEnumeratedCount(out var count) ? new Queue<TSource>(count) : new Queue<TSource>();
        while (val.TryGetNext(out var current))
        {
            hashSet2.Enqueue(current);
        }

        return hashSet2;
    }
}
