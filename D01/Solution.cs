using ZLinq;
using EverybodyCodes.Core;

namespace Y1.D01;

// https://everybody.codes/story/1/quests/1
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    => Calculate(input, 0).ToString();

    private static long Calculate(ReadOnlySpan<char> input, int maxRemainders)
    {
        var lines = (stackalloc Line[input.Count('\n') + 1]);
        var i = 0;
        foreach (var range in input.SplitAny('\n'))
            lines[i++] = Line.Parse(input[range]);
        return lines.AsValueEnumerable()
            .Select(l => l.Calculate(maxRemainders))
            .Max();
    }

    public string Solve2(ReadOnlySpan<char> input)
    => Calculate(input, 5).ToString();

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}

file readonly record struct Line(int A, int B, int C, long X, long Y, long Z, int M)
{
    public static Line Parse(ReadOnlySpan<char> line)
    {
        var ranges = (stackalloc Range[line.CountAny(' ', '=') + 1]);
        line.SplitAny(ranges, [' ', '=']);
        return new(int.Parse(line[ranges[1]]), int.Parse(line[ranges[3]]), int.Parse(line[ranges[5]]), long.Parse(line[ranges[7]]), long.Parse(line[ranges[9]]), long.Parse(line[ranges[11]]), int.Parse(line[ranges[13]]));
    }

    public long Calculate(int maxRemainders)
    => Eni(A, X, M, maxRemainders) + Eni(B, Y, M, maxRemainders) + Eni(C, Z, M, maxRemainders);

    public static long Eni(int n, long exp, int mod, int maxRemainders)
    {
        var score = 1L;
        if (maxRemainders > 0 && exp > maxRemainders)
        {
            score = SMA(n, exp - maxRemainders, mod);
            exp = maxRemainders;
        }
        else
            maxRemainders = (int)exp;
        var remainders = (stackalloc long[maxRemainders]);
        for (var i = 1; i <= exp; i++)
            remainders[^i] = score = score * n % mod;
        var result = 0L;
        foreach (var rem in remainders)
            result = result.Concat(rem);
        return result;

        static int SMA(int n, long exp, int mod)
        {
            var result = (long)n;
            if (exp > 2)
                result = SMA(n, exp >> 1, mod);
            result *= result;
            if (exp % 2 == 1)
                result *= n;
            return (int)(result % mod);
        }
    }
}

file static class Ext
{
    extension<T>(Span<T> span)
    {
        public void AddAt(ref int pos, T value)
        {
            span[pos++] = value;
            pos %= span.Length;
        }
    }

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
