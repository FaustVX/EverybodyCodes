namespace EverybodyCodes.Core;

public interface ISolution
{
    public string Solve(int part, ReadOnlySpan<char> input)
    => part switch
    {
        1 => Solve1(input),
        2 => Solve2(input),
        3 => Solve3(input),
        _ => throw new IndexOutOfRangeException(),
    };

    public abstract string Solve1(ReadOnlySpan<char> input);
    public abstract string Solve2(ReadOnlySpan<char> input);
    public abstract string Solve3(ReadOnlySpan<char> input);
}
