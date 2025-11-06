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
        throw new NotImplementedException();
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}
