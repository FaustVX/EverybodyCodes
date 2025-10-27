using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace EverybodyCodes.Core;

#if !LIBRARY
[DebuggerStepThrough]
#endif
[MemoryDiagnoser, ShortRunJob]
public class ShortBench<T>
where T : ISolution, new()
{
    private static readonly T _instance = new();

    [Benchmark]
    public object Part1()
    => _instance.Solve1(Globals.Parts[0].Input);

    [Benchmark]
    public object Part2()
    => _instance.Solve2(Globals.Parts[1].Input);

    [Benchmark]
    public object Part3()
    => _instance.Solve3(Globals.Parts[2].Input);
}

#if !LIBRARY
[DebuggerStepThrough]
#endif
[MemoryDiagnoser]
public class LongBench<T>
where T : ISolution, new()
{
    private static readonly T _instance = new();

    [Benchmark]
    public object Part1()
    => _instance.Solve1(Globals.Parts[0].Input);

    [Benchmark]
    public object Part2()
    => _instance.Solve2(Globals.Parts[1].Input);

    [Benchmark]
    public object Part3()
    => _instance.Solve3(Globals.Parts[2].Input);
}
