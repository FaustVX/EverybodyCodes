using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EverybodyCodes.Core;
using ZLinq;

namespace Y2024.D03;

public sealed class Solution : ISolution
{
    public string Solve1(ReadOnlySpan<char> input)
    {
        throw new NotImplementedException();
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

[System.Diagnostics.DebuggerStepThrough]
file static class Ext
{
    extension(Unsafe)
    {
        public static Span<T> AsSpan<T>(ReadOnlySpan<T> span)
        => MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in span[0]), span.Length);
    }
}
