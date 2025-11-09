using System.Collections;
using CommunityToolkit.HighPerformance;

namespace EverybodyCodes.Core.Extensions;

[System.Diagnostics.DebuggerStepThrough]
public static partial class Extension
{
    extension<T>(ReadOnlySpan<T> span)
    where T : unmanaged
    {
        public ReadOnlySpan2D<T> AsSpan2D(T delimitor, bool skipLastColumn = true)
        {
            var s = span.AsSpan2D(span.Count(delimitor), System.MemoryExtensions.IndexOf(span, delimitor) + 1);
            if (skipLastColumn)
                return s[.., ..^1];
            return s;
        }
    }

    extension<T>(ReadOnlySpan2D<T> span)
    where T : unmanaged
    {
        public RowsEnumerable<T> GetRows()
        => new(span);
    }

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
}
