using System.Numerics;
using CommunityToolkit.HighPerformance;
using ZLinq;

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

    extension<T>(Span<T> span)
    where T : unmanaged
    {
        public Span2D<T> AsSpan2D(T delimitor, bool skipLastColumn = true)
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

    extension<T>(T left)
    where T : IModulusOperators<T, T, T>, IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>, IComparisonOperators<T, T, bool>
    {
        public T EuclideanModulo(T right)
        {
            if (left >= T.AdditiveIdentity)
                return left % right;
            return (left % right + right) % right;
        }
    }
}
