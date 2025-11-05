using CommunityToolkit.HighPerformance;
using EverybodyCodes.Core;

namespace Y2025.D02;

// https://everybody.codes/event/2025/quests/2
public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        var a = Complex.Parse(input[input.IndexOf('[')..]);
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
    => Solve(input, 101).ToString();

    public string Solve3(ReadOnlySpan<char> input)
    => Solve(input, 1001).ToString();

    static int Solve(ReadOnlySpan<char> input, int size)
    {
        var a = Complex.Parse(input[input.IndexOf('[')..]);
        var count = 0;
        var offset = 1000 / (size - 1);

        for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var isValid = true;
                var p = a + new Complex(x * offset, y * offset);
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

        return count;
    }
}

file record struct Complex(long Real, long Imaginary)
{
    public static Complex Zero => new();

    public static Complex Parse(ReadOnlySpan<char> input)
    {
        input = input[1..^1];
        var comma = input.IndexOf(',');
        return new(int.Parse(input[..comma]), int.Parse(input[(comma+1)..]));
    }

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
