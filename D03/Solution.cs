using ZLinq;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Diagnostics;

namespace Y1.D03;

// https://everybody.codes/story/1/quests/3
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        var snails = (stackalloc Snail[input.Count('\n') + 1]);
        var i = 0;
        foreach (var lineRange in input.SplitAny('\n'))
            snails[i++] = Snail.Parse(input[lineRange]);
        for (var day = 0; day < 100; day++)
            foreach (ref var snail in snails)
                snail.Move();
        return snails.AsValueEnumerable()
            .Sum(s => s.PosFormula)
            .ToString();
    }

    public string Solve2(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}

file record struct Snail(int X, int Y)
{
    public static Snail Parse(ReadOnlySpan<char> line)
    {
        var ranges = (stackalloc Range[5]);
        if (ranges[..line.SplitAny(ranges, [' ', '='], StringSplitOptions.RemoveEmptyEntries)].ToIndexer(line) is [_, var x, _, var y])
            return new(int.Parse(x) - 1, int.Parse(y) - 1);
        throw new UnreachableException();
    }

    public readonly int PosFormula
    => X + 1 + 100 * (Y + 1);

    public bool Move()
    {
        if (Y <= 0)
        {
            (Y, X) = (X, 0);
            return true;
        }
        Y--;
        X++;
        return false;
    }
}
