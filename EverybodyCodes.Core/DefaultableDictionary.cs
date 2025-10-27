using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EverybodyCodes.Core;

[DebuggerStepThrough]
public class DefaultableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Func<TKey, TValue> _defaultValue;
    public DefaultableDictionary(int capacity, TValue defaultValue = default)
    : base(capacity)
    => _defaultValue = _ => defaultValue;

    public DefaultableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, TValue defaultValue = default)
    : base(collection)
    => _defaultValue = _ => defaultValue;

    public DefaultableDictionary(Func<TKey, TValue> create)
    : base()
    => _defaultValue = create;

    public DefaultableDictionary(TValue defaultValue = default)
    : base()
    => _defaultValue = _ => defaultValue;

    public new TValue this[TKey key]
    {
        get
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(this, key, out var exists);
            if (exists)
                return value!;
            return value = _defaultValue(key);
        }
        set => base[key] = value;
    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => this[key];
        set => this[key] = value;
    }

    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key]
    => this[key];
}
