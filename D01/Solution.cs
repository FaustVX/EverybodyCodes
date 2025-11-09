using System.Diagnostics;
using System.Numerics;
using EverybodyCodes.Core;

namespace Y2025.D01;

using System;

// https://everybody.codes/event/2025/quests/1
public sealed class Solution : ISolution
{
    static string Solve(ReadOnlySpan<char> input, Func<int, int, Span<Range>, int> function)
    {
        var names = input[..input.IndexOf('\n')];
        var ranges = (stackalloc Range[names.Count(',') + 1]);
        names.Split(ranges, ',');
        var index = 0;
        var instructions = input[(input.IndexOf('\n') + 2)..];
        foreach (var instruction in instructions.Split(','))
            index = instructions[instruction] switch 
            {
                ['L', .. var dir] when int.TryParse(dir, out var a)
                    => function(-a, index, ranges),
                ['R', .. var dir] when int.TryParse(dir, out var a)
                    => function(a, index, ranges),
                _ => throw new UnreachableException(),
            };
        return names[ranges[index]].ToString();
    }

    // https://everybody.codes/event/2025/quests/1#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    => Solve(input, (a, index, ranges) => Math.Clamp(index + a, 0, ranges.Length - 1));

    // https://everybody.codes/event/2025/quests/1#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    => Solve(input, (a, index, ranges) => (index + a).EuclideanModulo(ranges.Length));

    // https://everybody.codes/event/2025/quests/1#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    => Solve(input, (a, _, ranges) =>
    {
        ranges[0].SwapWith(ref ranges[a.EuclideanModulo(ranges.Length)]);
        return 0;
    });
}

file static class Ext
{
    extension<T>(ref T value)
    where T : unmanaged
    {
        public void SwapWith(ref T other)
        => (value, other) = (other, value);
    }

    extension<T>(T left)
    where T : IModulusOperators<T, T, T>, IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>, IComparisonOperators<T, T, bool>
    {
        public T EuclideanModulo(T right)
        {
            if (left >= T.AdditiveIdentity)
                return left % right;
            return (left % right + right) % right;
        }
    }
}
