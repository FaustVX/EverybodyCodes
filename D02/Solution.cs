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
        Node<string> left = new("", 0), right = new("", 0);
        foreach (var lineRange in input.SplitAny('\n'))
            switch (globalParts[..input[lineRange].SplitAny(globalParts, [' ', '=', ',', '[', ']'], StringSplitOptions.RemoveEmptyEntries)].ToIndexer(input[lineRange]))
            {
                case ["ADD", _, _, _, var ln, var ls, _, var rn, var rs]:
                {
                    var l = left.Add(ls.ToString(), int.Parse(ln));
                    var r = right.Add(rs.ToString(), int.Parse(rn));
                    break;
                }
            }
        (left, right) = (left.Right!, right.Right!);
        return string.Create(100, left.GetLargestRow(), (span, row) =>
        {
            foreach (var node in row)
            {
                node.Name.CopyTo(span);
                span = span[node.Name.Length..];
            }
        }).TrimEnd('\0') + string.Create(100, right.GetLargestRow(), (span, row) =>
        {
            foreach (var node in row)
            {
                node.Name.CopyTo(span);
                span = span[node.Name.Length..];
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
file sealed class Node<T>(T Name, int Value)
{
    public Node<T>? Left { get; private set; }
    public Node<T>? Right { get; private set; }
    public T Name { get; } = Name;
    public int Value { get; } = Value;

    public IEnumerable<Node<T>> this[int layer]
    {
        get
        {
            if (layer == 0)
                return [this];
            return [..Left?[layer - 1] ?? [], ..Right?[layer - 1] ?? []];
        }
    }

    public Node<T> Add(T name, int value)
    {
        if (value < Value)
            if (Left is {} l)
                return l.Add(name, value);
            else
                return Left = new(name, value);
        else if (value > Value)
            if (Right is {} r)
                return r.Add(name, value);
            else
                return Right = new(name, value);
        else
            throw new UnreachableException();
    }

    public IEnumerable<Node<T>> GetLargestRow()
    {
        IEnumerable<Node<T>> largest = [];
        for (var i = 1;; i++)
        {
            var current = this[i];
            if (!current.Any())
                return largest;
            else if (largest.TryGetNonEnumeratedCount(out var c1) && current.TryGetNonEnumeratedCount(out var c2) && c2 > c1)
                largest = current;
        }
    }

    public override string ToString()
    => $"{Name}: {Value} [< {Left} > {Right}]";
}
