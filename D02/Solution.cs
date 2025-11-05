using ZLinq;
using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;
using EverybodyCodes.Core.Extensions;

namespace Y2025.D02;

// https://everybody.codes/event/2025/quests/2
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        var ranges = (stackalloc Range[4]);
        input.SplitAny(ranges, ['=', '[', ',', ']'], StringSplitOptions.RemoveEmptyEntries);

        var a = new Complex(int.Parse(input[ranges[1]]), int.Parse(input[ranges[2]]));
        var value = Complex.Zero;

        for (var i = 0; i < 3; i++)
        {
            value *= value;
            value /= new Complex(10, 10);
            value += a;
        }

        return value.ToString();
    }

    public string Solve2(ReadOnlySpan<char> input)
    {
        var ranges = (stackalloc Range[4]);
        input.SplitAny(ranges, ['=', '[', ',', ']'], StringSplitOptions.RemoveEmptyEntries);

        var a = new Complex(int.Parse(input[ranges[1]]), int.Parse(input[ranges[2]]));
        var count = 0;

        for (var y = 0; y < 101; y++)
            for (var x = 0; x < 101; x++)
            {
                var isValid = true;
                var p = a + new Complex(x * 10, y * 10);
                var result = p;
                for (var i = 1; i < 100; i++)
                {
                    result *= result;
                    result /= new Complex(100000, 100000);
                    result += p;
                    if (Math.Abs(result.Real) > 1000000 || Math.Abs(result.Imaginary) > 1000000)
                    {
                        isValid = false;
                        break;
                    }
                }
                if (isValid)
                    count++;
            }

        return count.ToString();
    }

    public string Solve3(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
    }
}

file record struct Complex(long Real, long Imaginary)
{
    public static Complex Zero => new();

    public void operator +=(Complex other)
    {
        Real += other.Real;
        Imaginary += other.Imaginary;
    }

    public static Complex operator +(Complex left, Complex right)
    => left += right;

    public void operator *=(Complex other)
    => (Real, Imaginary) = (Real * other.Real - Imaginary * other.Imaginary, Real * other.Imaginary + Imaginary * other.Real);

    public static Complex operator *(Complex left, Complex right)
    => left *= right;

    public void operator /=(Complex other)
    {
        Real /= other.Real;
        Imaginary /= other.Imaginary;
    }

    public static Complex operator /(Complex left, Complex right)
    => left /= right;

    public override readonly string ToString()
    => $"[{Real},{Imaginary}]";
}
