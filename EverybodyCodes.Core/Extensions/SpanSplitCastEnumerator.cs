using System.Diagnostics;
using System.Runtime.CompilerServices;
using ZLinq;

namespace EverybodyCodes.Core.Extensions;

[DebuggerStepThrough]
public ref struct SpanSplitCastEnumerator<T>(MemoryExtensions.SpanSplitEnumerator<char> enumerator, Func<ReadOnlySpan<char>, IFormatProvider?, T> parser)
where T : allows ref struct
{
    private MemoryExtensions.SpanSplitEnumerator<char> _enumerator = enumerator;

    public readonly SpanSplitCastEnumerator<T> GetEnumerator()
    => this;

    public readonly T Current
    => parser(_enumerator.Source[_enumerator.Current], null);

    public bool MoveNext()
    => _enumerator.MoveNext();
}

[DebuggerStepThrough]
public ref struct FromSpanSplitCastEnumerator<T>(SpanSplitCastEnumerator<T> source) : IValueEnumerator<T>, IDisposable
{
    private SpanSplitCastEnumerator<T> source = source;

    public readonly bool TryGetNonEnumeratedCount(out int count)
    {
        Unsafe.SkipInit(out count);
        return false;
    }

    public readonly bool TryGetSpan(out ReadOnlySpan<T> span)
    {
        Unsafe.SkipInit(out span);
        return false;
    }

    public readonly bool TryCopyTo(scoped Span<T> destination, Index offset)
    => false;

    public bool TryGetNext(out T current)
    {
        if (source.MoveNext())
        {
            current = source.Current;
            return true;
        }

        Unsafe.SkipInit(out current);
        return false;
    }

    public readonly void Dispose()
    { }
}

public static partial class Extension
{
    extension(MemoryExtensions.SpanSplitEnumerator<char> enumerator)
    {
        public SpanSplitCastEnumerator<T> ParseTo<T>()
        where T : ISpanParsable<T>
        => new(enumerator.GetEnumerator(), T.Parse);
        public SpanSplitCastEnumerator<T> ParseTo<T>(Func<ReadOnlySpan<char>, IFormatProvider?, T> parser)
        where T : allows ref struct
        => new(enumerator.GetEnumerator(), parser);
        public SpanSplitCastEnumerator<ReadOnlySpan<char>> ToSpans()
        => new(enumerator.GetEnumerator(), static (s, _) => s);
    }

    extension<T>(SpanSplitCastEnumerator<T> values)
    {
        public ValueEnumerable<FromSpanSplitCastEnumerator<T>, T> AsValueEnumerable()
        => new(new(values));
    }
}
