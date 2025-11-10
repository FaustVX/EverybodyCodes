using ZLinq;
using EverybodyCodes.Core;

namespace Y2025.D06;

// https://everybody.codes/event/2025/quests/6
public sealed class Solution : ISolution
{
    // https://everybody.codes/event/2025/quests/6#:~:text=Part%20I
    public string Solve1(ReadOnlySpan<char> input)
    {
        var total = 0;
        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] != 'a')
                continue;
            total += input[..i].Count('A');
        }
        return total.ToString();
    }

    // https://everybody.codes/event/2025/quests/6#:~:text=Part%20II
    public string Solve2(ReadOnlySpan<char> input)
    {
        var total = 0;
        for (var i = 0; i < input.Length; i++)
        {
            if (!char.IsLower(input[i]))
                continue;
            total += input[..i].Count((char)(input[i] - 32));
        }
        return total.ToString();
    }

    // https://everybody.codes/event/2025/quests/6#:~:text=Part%20III
    public string Solve3(ReadOnlySpan<char> input)
    {
        input = string.Create(input.Length * (Globals.IsTest ? 1 : 1000), input, (s, i) =>
        {
            while (!s.IsEmpty)
            {
                i.CopyTo(s);
                s = s[i.Length..];
            }
        });
        var distance = Globals.IsTest ? 10 : 1000;
        var total = 0;
        for (var i = 0; i < input.Length; i++)
        {
            if (!char.IsLower(input[i]))
                continue;
            var area = input.TrySlice(i - distance, distance * 2 + 1);
            total += area.Count((char)(input[i] - 32));
        }
        return total.ToString();
    }
}

file static class Ext
{
    extension<T>(ReadOnlySpan<T> span)
    {
        public ReadOnlySpan<T> TrySlice(int start, int length)
        {
            if (start < 0)
                (start, length) = (0, length + start);
            if (length > span[start..].Length)
                length = span[start..].Length;
            return span.Slice(start, length);
        }
    }
}
