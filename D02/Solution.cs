using ZLinq;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Diagnostics;

namespace Y1.D02;

// https://everybody.codes/story/1/quests/2
public sealed class Solution : ISolution
{
    static string Execute(ReadOnlySpan<char> input, bool newSwap)
    {
        var globalParts = (stackalloc Range[10]);
        var nodes = (stackalloc Node<char>[(input.Count('\n') + 1) * 2 + 2]);
        var i = 0;
        Index left = i++, right = i++;
        nodes[left] = nodes[right] = new(' ', 0);
        foreach (var lineRange in input.SplitAny('\n'))
            switch (globalParts[..input[lineRange].SplitAny(globalParts, [' ', '=', ',', '[', ']'], StringSplitOptions.RemoveEmptyEntries)].ToIndexer(input[lineRange]))
            {
                case ["ADD", _, _, _, var ln, var ls, _, var rn, var rs]:
                {
                    nodes[left].Add(ls[0], int.Parse(ln), i++, nodes);
                    nodes[right].Add(rs[0], int.Parse(rn), i++, nodes);
                    break;
                }
                case ["SWAP", var a] when newSwap:
                {
                    var id = int.Parse(a) * 2;
                    ref var node1 = ref nodes[id];
                    ref var node2 = ref nodes[id + 1];
                    (node1, node2) = (node2, node1);
                    break;
                }
                case ["SWAP", var a]:
                {
                    var id = int.Parse(a) * 2;
                    ref var node1 = ref nodes[id];
                    ref var node2 = ref nodes[id + 1];
                    (node1, node2) = (node1 with { Name = node2.Name, Value = node2.Value }, node2 with { Name = node1.Name, Value = node1.Value });
                    break;
                }
            }
        (left, right) = (nodes[left].Right!, nodes[right].Right!);
        var largest = (stackalloc Node<char>[nodes.Length / 2]); // just an approximation
        var size = nodes[left].GetLargestRow(largest, nodes);
        size += nodes[right].GetLargestRow(largest[size..], nodes);
        return string.Create(size, largest[..size], (span, row) =>
        {
            foreach (var node in row)
            {
                span[0] = node.Name;
                span = span[1..];
            }
        });
    }

    public string Solve1(ReadOnlySpan<char> input)
    => Execute(input, newSwap: false);

    public string Solve2(ReadOnlySpan<char> input)
    => Execute(input, newSwap: false);

    public string Solve3(ReadOnlySpan<char> input)
    => Execute(input, newSwap: true);
}

[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
file struct Node<T>(T Name, int Value)
where T : unmanaged
{
    public Index Left { get; private set; } = ^0;
    public Index Right { get; private set; } = ^0;
    public T Name { get; init; } = Name;
    public int Value { get; init; } = Value;

    public void Add(T name, int value, Index index, Span<Node<T>> span)
    {
        if (value < Value)
            if (Left.IsValid)
                span[Left].Add(name, value, index, span);
            else
                span[Left = index] = new(name, value);
        else if (value > Value)
            if (Right.IsValid)
                span[Right].Add(name, value, index, span);
            else
                span[Right = index] = new(name, value);
        else
            throw new UnreachableException();
    }

    public readonly int GetRow(int layer, Span<Node<T>> destination, ReadOnlySpan<Node<T>> span)
    {
        if (layer == 0)
        {
            destination[0] = this;
            return 1;
        }
        var offset = 0;
        if (Left.IsValid)
            offset = span[Left].GetRow(layer - 1, destination, span);
        if (Right.IsValid)
            offset += span[Right].GetRow(layer - 1, destination[offset..], span);
        return offset;
    }

    public readonly int GetLargestRow(Span<Node<T>> destination, ReadOnlySpan<Node<T>> span)
    {
        var current = (stackalloc Node<T>[destination.Length]);
        var largestOffset = 0;
        for (var i = 1;; i++)
        {
            var offset = GetRow(i, current, span);
            if (offset == 0)
                return largestOffset;
            else if (offset > largestOffset)
            {
                current.CopyTo(destination);
                largestOffset = offset;
            }
        }
    }

    public override readonly string ToString()
    => $"{Name}: {Value} [< {Left} > {Right}]";
}

file static class Ext
{
    extension(Index index)
    {
        public bool IsValid
        => index is not { IsFromEnd: true, Value: 0 };
    }
}
