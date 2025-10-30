using ZLinq;
using EverybodyCodes.Core;

namespace Y1.D01;

// https://everybody.codes/story/1/quests/1
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        var lines = (stackalloc Line[input.Count('\n') + 1]);
        var i = 0;
        foreach (var range in input.SplitAny('\n'))
            lines[i++] = new(input[range][2] - '0', input[range][6] - '0', input[range][10] - '0', input[range][14] - '0', input[range][18] - '0', input[range][22] - '0', int.Parse(input[range][26..]));
        return lines.AsValueEnumerable()
            .Select(l => l.Calculate())
            .Max().ToString();
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

file readonly record struct Line(int A, int B, int C, int X, int Y, int Z, int M)
{
    public long Calculate()
    => Eni(A, X, M) + Eni(B, Y, M) + Eni(C, Z, M);

    public static long Eni(int n, int exp, int mod)
    {
        var remainders = (stackalloc int[exp]);
        var score = 1;
        for (var i = 1; i <= exp; i++)
            remainders[^i] = score = score * n % mod;
        var result = 0L;
        foreach (var rem in remainders)
            result = result.Concat(rem);
        return result;
    }
}

file static class Ext
{
    extension<T>(T n)
    where T : System.Numerics.IBinaryInteger<T>
    {
        public int GetDecimalLength()
        {
            if (T.IsZero(n))
                return 1;
            n = T.Abs(n);
            var ten = T.CreateChecked(10);
            var length = 0;
            while (n > T.Zero)
            {
                n /= ten;
                length++;
            }
            return length;
        }

        public T Concat(T other)
        {
            if (T.IsZero(n))
                return other;
            if (T.IsZero(other))
                return n;
            var length = other.GetDecimalLength();
            var ten = T.CreateChecked(10);
            var mul = T.One;
            for (var i = 0; i < length; i++)
                mul *= ten;
            return n * mul + other;
        }
    }
}
