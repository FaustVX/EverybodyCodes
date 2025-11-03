using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Attributes;
using EverybodyCodes.Core.Extensions;
using System.Diagnostics;

namespace Y2.D01;

// https://everybody.codes/story/2/quests/1
public sealed class Solution : ISolution
{
    static int Toss(ReadOnlySpan2D<char> grid, int slot, ReadOnlySpan<char> path)
    {
        var x = slot * 2;
        var i = 0;
        for (var y = 0; y < grid.Height; y++)
            if (grid[y, x] == '*')
                x = path[i++] switch
                {
                    'R' when x == grid.Width - 1 => x - 1,
                    'L' when x == 0 => x + 1,
                    'R' => x + 1,
                    'L' => x - 1,
                    _ => throw new UnreachableException(),
                };
        return x / 2 + 1;
    }

    [AddFinalLineFeed]
    public string Solve1(ReadOnlySpan<char> input)
    {
        var section = input.IndexOf("\n\n");
        var grid = input[..(section + 1)].AsSpan2D('\n');
        var tokens = input[(section + 2)..].AsSpan2D('\n');
        var score = 0;

        for (var t = 0; t < tokens.Height; t++)
            score += Math.Max(Toss(grid, t, tokens.GetRowSpan(t)) * 2 - (t + 1), 0);
        return score.ToString();
    }

    [AddFinalLineFeed]
    public string Solve2(ReadOnlySpan<char> input)
    {
        var section = input.IndexOf("\n\n");
        var grid = input[..(section + 1)].AsSpan2D('\n');
        var tokens = input[(section + 2)..].AsSpan2D('\n');
        var score = 0;

        foreach (var path in tokens.GetRows())
        {
            var max = 0;
            for (var slot = 0; slot <= grid.Width / 2; slot++)
            {
                var s = Toss(grid, slot, path);
                s = s * 2 - (slot + 1);
                if (s > max)
                    max = s;
            }
            score += max;
        }

        return score.ToString();
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}
