using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EverybodyCodes.Core;
using CommunityToolkit.HighPerformance;
using System.Diagnostics;
using ZLinq;

namespace Y2024.D03;

using System;

public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        var span = MemoryMarshal.Cast(Unsafe.AsSpan(input), static c => (ushort)(c switch
        {
            '.' => 0,
            '\n' => '\n',
            '#' => 1,
            _ => throw new UnreachableException(),
        }));
        var ground = span.AsSpan2D('\n').Minimize((ushort)0);
        span.Replace('\n', (ushort)0);
        for (var isModified = true; isModified;)
        {
            isModified = false;
            for (var y = 1; y < ground.Height - 1; y++)
                for (var x = 1; x < ground.Width - 1; x++)
                {
                    ref var c = ref ground[y, x];
                    if (c == 0)
                        continue;
                    if (ground[y - 1, x] >= c && ground[y + 1, x] == c && ground[y, x - 1] >= c && ground[y, x + 1] == c)
                    {
                        isModified = true;
                        c++;
                    }
                }
        }

        return span.AsValueEnumerable().Sum().ToString();
    }

    public string Solve2(ReadOnlySpan<char> input)
    => Solve1(input);

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}

[System.Diagnostics.DebuggerStepThrough]
file static class Ext
{
    extension(Unsafe)
    {
        public static Span<T> AsSpan<T>(ReadOnlySpan<T> span)
        => MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in span[0]), span.Length);
    }

    extension(MemoryMarshal)
    {
        public static Span<TTo> Cast<TFrom, TTo>(Span<TFrom> from, Func<TFrom, TTo> converter)
        where TFrom : struct
        where TTo : struct
        {
            var to = MemoryMarshal.Cast<TFrom, TTo>(from);
            for (var i = 0; i < from.Length; i++)
                to[i] = converter(from[i]);
            return to;
        }
    }

    extension<T>(Span<T> span)
    where T : unmanaged
    {
        public Span2D<T> AsSpan2D(T delimitor)
        => span.AsSpan2D(span.Count(delimitor), span.IndexOf(delimitor) + 1)[.., ..^1];
    }

    extension<T>(Span2D<T> span)
    where T : unmanaged
    {
        public Span2D<T> Minimize(T border)
        {
            var temp = (stackalloc T[span.Width]);
            var prev = span;

            while (true)
            {
                span.GetRow(0).CopyTo(temp);
                if (!temp.ContainsAnyExcept(border))
                    break;
                prev = span;
                span = span[1.., ..];
            }
            span = prev;
            while (true)
            {
                span.GetRow((^1).GetOffset(span.Height)).CopyTo(temp);
                if (!temp.ContainsAnyExcept(border))
                    break;
                prev = span;
                span = span[..^1, ..];
            }
            span = prev;

            temp = temp[..span.Height];
            while (true)
            {
                span.GetColumn(0).CopyTo(temp);
                if (!temp.ContainsAnyExcept(border))
                    break;
                prev = span;
                span = span[.., 1..];
            }
            span = prev;
            while (true)
            {
                span.GetColumn((^1).GetOffset(span.Width)).CopyTo(temp);
                if (!temp.ContainsAnyExcept(border))
                    break;
                prev = span;
                span = span[.., ..^1];
            }
            span = prev;

            return span;
        }
    }
}
