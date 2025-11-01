using ZLinq;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;
using System.Diagnostics;

namespace Y1.D02;

// https://everybody.codes/story/1/quests/2
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
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
            }
        (left, right) = (nodes[left].Right!, nodes[right].Right!);
        return string.Create(100, nodes[left].GetLargestRow(nodes), (span, row) =>
        {
            foreach (var node in row)
            {
                span[0] = node.Name;
                span = span[1..];
            }
        }).TrimEnd('\0') + string.Create(100, nodes[right].GetLargestRow(nodes), (span, row) =>
        {
            foreach (var node in row)
            {
                span[0] = node.Name;
                span = span[1..];
            }
        }).TrimEnd('\0');
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

[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
file struct Node<T>(T Name, int Value)
{
    public Index Left { get; private set; } = ^0;
    public Index Right { get; private set; } = ^0;
    public T Name { get; } = Name;
    public int Value { get; } = Value;

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

    public readonly IEnumerable<Node<T>> GetRow(int layer, ReadOnlySpan<Node<T>> span)
    {
        if (layer == 0)
            return [this];
        return [..Left .IsValid ? span[Left].GetRow(layer - 1, span) : [], ..Right.IsValid ? span[Right].GetRow(layer - 1, span) : []];
    }

    public readonly IEnumerable<Node<T>> GetLargestRow(ReadOnlySpan<Node<T>> span)
    {
        IEnumerable<Node<T>> largest = [];
        for (var i = 1;; i++)
        {
            var current = GetRow(i, span);
            if (!current.Any())
                return largest;
            else if (largest.TryGetNonEnumeratedCount(out var c1) && current.TryGetNonEnumeratedCount(out var c2) && c2 > c1)
                largest = current;
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
