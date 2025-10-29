using System.Diagnostics;
using ZLinq;
using EverybodyCodes.Core;

namespace Y2024.D01;

public sealed class Solution : ISolution
{
    // https://everybody.codes/event/2024/quests/1#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    => input.AsValueEnumerable().Sum(Potions).ToString();

    // https://everybody.codes/event/2024/quests/1#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    => input.AsValueEnumerable().Chunk(2).Sum(static p => p switch
    {
        ['x', 'x'] => 0,
        ['x', var b] => Potions(b),
        [var a, 'x'] => Potions(a),
        [var a, var b] => Potions(a) + Potions(b) + 2,
        _ => throw new UnreachableException(),
    }).ToString();
    // https://everybody.codes/event/2024/quests/1#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    => input.AsValueEnumerable().Chunk(3).Sum(static p => p switch
    {
        ['x', 'x', 'x'] => 0,
        [var a, 'x', 'x'] => Potions(a),
        ['x', var b, 'x'] => Potions(b),
        ['x', 'x', var c] => Potions(c),
        ['x', var b, var c] => Potions(b) + Potions(c) + 2,
        [var a, 'x', var c] => Potions(a) + Potions(c) + 2,
        [var a, var b, 'x'] => Potions(a) + Potions(b) + 2,
        [var a, var b, var c] => Potions(a) + Potions(b) + Potions(c) + 6,
        _ => throw new UnreachableException(),
    }).ToString();

    static int Potions(char c)
    => c switch
    {
        'A' or 'x' => 0,
        'B' => 1,
        'C' => 3,
        'D' => 5,
        _ => throw new UnreachableException(),
    };
}
