using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Linq.Expressions;

namespace Y2025.D01;

using System;
using System.Diagnostics;
using System.Numerics;

// https://everybody.codes/event/2025/quests/1
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
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
                    => Math.Max(index - a, 0),
                ['R', .. var dir] when int.TryParse(dir, out var a)
                    => Math.Min(index + a, ranges.Length - 1),
                _ => throw new UnreachableException(),
            };
        return names[ranges[index]].ToString();
    }

    public string Solve2(ReadOnlySpan<char> input)
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
                    => (index - a).EuclideanModulo(ranges.Length),
                ['R', .. var dir] when int.TryParse(dir, out var a)
                    => (index + a).EuclideanModulo(ranges.Length),
                _ => throw new UnreachableException(),
            };
        return names[ranges[index]].ToString();
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        var names = input[..input.IndexOf('\n')];
        var ranges = (stackalloc Range[names.Count(',') + 1]);
        names.Split(ranges, ',');
        var instructions = input[(input.IndexOf('\n') + 2)..];
        foreach (var instruction in instructions.Split(','))
            switch (instructions[instruction])
            {
                case ['L', .. var dir] when int.TryParse(dir, out var a):
                    ranges[0].SwapWith(ref ranges[(-a).EuclideanModulo(ranges.Length)]);
                    break;
                case ['R', .. var dir] when int.TryParse(dir, out var a):
                    ranges[0].SwapWith(ref ranges[a.EuclideanModulo(ranges.Length)]);
                    break;
                default: throw new UnreachableException();
            };
        return names[ranges[0]].ToString();
    }
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
