using System.Collections;
using System.Diagnostics;

namespace EverybodyCodes.Core.Extensions;

[DebuggerStepThrough]
public readonly ref struct SpanIndexer<T>(ReadOnlySpan<Range> ranges, ReadOnlySpan<T> line)
{
    private readonly ReadOnlySpan<Range> ranges = ranges;
    private readonly ReadOnlySpan<T> line = line;

    public int Length => ranges.Length;

    public ReadOnlySpan<T> this[Index a] => line[ranges[a]];

    public Enumerator GetEnumerator()
    => new(this);

    [DebuggerStepThrough]
    public ref struct Enumerator(SpanIndexer<T> indexer) : IEnumerator<ReadOnlySpan<T>>
    {
        private int _index = -1;
        private readonly SpanIndexer<T> indexer = indexer;

        public readonly ReadOnlySpan<T> Current
        => indexer[_index];

        readonly object IEnumerator.Current
        => Current.ToString();

        public bool MoveNext()
        => ++_index < indexer.Length;

        readonly void IEnumerator.Reset() { }

        readonly void IDisposable.Dispose() { }
    }
}

public static partial class Extension
{
    extension(ReadOnlySpan<Range> span)
    {
        public SpanIndexer<T> ToIndexer<T>(ReadOnlySpan<T> line)
        => new(span, line);
    }
}
