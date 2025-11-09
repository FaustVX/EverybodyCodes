using System.Collections;
using System.Numerics;
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

    extension<T>(T n)
    where T : INumberBase<T>, IComparisonOperators<T, T, bool>
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
