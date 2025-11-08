using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Y2.D03;
using System;
using System.Linq;

// https://everybody.codes/story/2/quests/3
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        var dices = (stackalloc Die[input.Count('\n') + 1]);
        var i = 0;
        foreach (var die in input.Split('\n'))
            dices[i++] = Die.Parse(input[die]);
        var points = 10_000;
        for (i = 0; points > 0; i++)
        {
            var score = 0;
            foreach (ref var die in dices)
                score += die.Roll();
            points -= score;
        }
        return i.ToString();
    }

    public string Solve2(ReadOnlySpan<char> input)
    {
        var lineBreak = input.IndexOf("\n\n");
        var results = input[(lineBreak + 2)..].ToString();
        input = input[..lineBreak];
        var dices = (stackalloc Die[input.Count('\n') + 1]);
        var i = 0;
        foreach (var die in input.Split('\n'))
            dices[i++] = Die.Parse(input[die]);
        var sorted = (stackalloc int[dices.Length]);
        dices.AsValueEnumerable()
            .OrderBy(d => d.Roll(results))
            .Select(d => d.Index)
            .CopyTo(sorted);
        return string.Create(sorted.Length * 2 - 1, sorted, (span, indices) =>
        {
            foreach (var index in indices)
            {
                span[0] = (char)(index + '0');
                span = span[1..];
                if (!span.IsEmpty)
                {
                    span[0] = ',';
                    span = span[1..];
                }
            }
        });
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}

file struct Die(int index, Array faces, int facesCount, int seed)
{
    private readonly Array faces = faces;
    public readonly int Index { get; } = index;
    public readonly int Seed { get; } = seed;
    public readonly int _facesCount = facesCount;
    private long _pulse = seed;
    private long _rollCount = 1;
    private int _lastRolled = 0;

    public int Roll(ReadOnlySpan<char> results)
    {
        var i = 0;
        while (!results.IsEmpty)
        {
            var roll = Roll();
            if (roll == results[0] - '0')
                results = results[1..];
            i++;
        }
        return i;
    }

    public int Roll()
    {
        var spin = _rollCount * _pulse;
        _lastRolled = (int)((spin + _lastRolled) % _facesCount);
        var roll = faces[_lastRolled];
        _pulse += spin;
        _pulse %= Seed;
        _pulse += 1 + _rollCount + Seed;
        _rollCount++;
        return roll;
    }

    public static Die Parse(ReadOnlySpan<char> input)
    {
        var ranges = (stackalloc Range[5]);
        input.SplitAny(ranges, [' ', '=', ':'], StringSplitOptions.RemoveEmptyEntries);
        if (ranges.ToIndexer(input) is [var index, _, ['[', .. var faces, ']'], _, var seed])
            return new(int.Parse(index), new(faces.Split(',')), faces.Count(',') + 1, int.Parse(seed));
        throw new UnreachableException();
    }
}

[InlineArray(10)]
file struct Array
{
    public Array(ReadOnlySpan<int> ints)
    => ints.CopyTo(this);
    public Array(MemoryExtensions.SpanSplitEnumerator<char> enumerator)
    {
        Span<int> array = this;
        foreach (var item in enumerator)
        {
            array[0] = int.Parse(enumerator.Source[item]);
            array = array[1..];
        }
    }

    private int _field;
}
