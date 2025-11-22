using System.Collections;
using System.Diagnostics;
using CommunityToolkit.HighPerformance;

namespace EverybodyCodes.Core.Extensions;

[DebuggerStepThrough]
public readonly ref struct RowsEnumerable<T>(ReadOnlySpan2D<T> span2d)
{
    private readonly ReadOnlySpan2D<T> _span2D = span2d;

    public readonly Enumerator GetEnumerator()
    => new(_span2D);

    public ref struct Enumerator(ReadOnlySpan2D<T> span2d) : IEnumerator<ReadOnlySpan<T>>
    {
        private readonly ReadOnlySpan2D<T> _span2D = span2d;
        private int _i = -1;
        public readonly ReadOnlySpan<T> Current
        => _span2D.GetRowSpan(_i);

        readonly object IEnumerator.Current
        => Current.ToString();

        public readonly void Dispose() { }

        public bool MoveNext()
        => ++_i < _span2D.Height;

        public readonly void Reset() { }
    }
}
