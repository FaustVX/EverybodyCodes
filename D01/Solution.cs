using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Attributes;
using EverybodyCodes.Core.Extensions;
using System.Diagnostics;
using System.Collections.Immutable;
using System.Collections.Frozen;

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

    [AddFinalLineFeed]
    public string Solve3(ReadOnlySpan<char> input)
    {
        var section = input.IndexOf("\n\n");
        var grid = input[..(section + 1)].AsSpan2D('\n');
        var tokens = input[(section + 2)..].AsSpan2D('\n');

        var tossCache = TossCacheGenerator(grid, tokens).ToFrozenDictionary();

        var length = grid.Width / 2 + 1;
        var min = int.MaxValue;
        var max = int.MinValue;

        for (var slot0 = 0; slot0 < length; slot0++)
            for (var slot1 = 0; slot1 < length; slot1++)
            {
                if (slot1 == slot0)
                    continue;
                for (var slot2 = 0; slot2 < length; slot2++)
                {
                    if (IsIn(slot2, slot0, slot1))
                        continue;
                    for (var slot3 = 0; slot3 < length; slot3++)
                    {
                        if (IsIn(slot3, slot0, slot1, slot2))
                            continue;
                        for (var slot4 = 0; slot4 < length; slot4++)
                        {
                            if (IsIn(slot4, slot0, slot1, slot2, slot3))
                                continue;
                            for (var slot5 = 0; slot5 < length; slot5++)
                            {
                                if (IsIn(slot5, slot0, slot1, slot2, slot3, slot4))
                                    continue;
                                var s = Combinator(tossCache, [slot0, slot1, slot2, slot3, slot4, slot5]);
                                if (s > max)
                                    max = s;
                                if (s < min)
                                    min = s;
                            }
                        }
                    }
                }
            }

        return $"{min} {max}";

        static bool IsIn<T>(T value, params ReadOnlySpan<T> span)
        => span.Contains(value);

        static int Combinator(IReadOnlyDictionary<(int slot, int path), int> cache, ReadOnlySpan<int> slots)
        {
            var score = 0;
            for (var token = 0; token < slots.Length; token++)
                score += cache[(slots[token], token)];
            return score;
        }

        static IReadOnlyDictionary<(int slot, int path), int> TossCacheGenerator(ReadOnlySpan2D<char> grid, ReadOnlySpan2D<char> tokens)
        {
            var length = grid.Width / 2 + 1;
            var tossCache = new Dictionary<(int slot, int path), int>(capacity: length * tokens.Height);
            for (var slot = 0; slot < length; slot++)
                for (var path = 0; path < tokens.Height; path++)
                    tossCache[(slot, path)] = Math.Max(0, Toss(grid, slot, tokens.GetRowSpan(path)) * 2 - (slot + 1));
            return tossCache;
        }
    }
}
