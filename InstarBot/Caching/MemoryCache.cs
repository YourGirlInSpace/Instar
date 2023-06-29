using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;

namespace PaxAndromeda.Instar.Caching;

public sealed class MemoryCache<T> : MemoryCache, IEnumerable<KeyValuePair<string, T>>
{
    public MemoryCache(string name, NameValueCollection config = null!) : base(name, config)
    {
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MemoryCache(string name, NameValueCollection config, bool ignoreConfigSection) : base(name, config, ignoreConfigSection)
    {
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public bool Add(string key, T value, DateTimeOffset absoluteExpiration, string regionName = null!)
    {
        return value != null && base.Add(key, value, absoluteExpiration, regionName);
    }

    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public bool Add(string key, T value, CacheItemPolicy policy, string regionName = null!)
    {
        return value != null && base.Add(key, value, policy, regionName);
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public new T Get(string key, string regionName = null!)
    {
        return (T) base.Get(key, regionName);
    }

    public new IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        using var enumerator = base.GetEnumerator();

        while (enumerator.MoveNext())
        {
            yield return new KeyValuePair<string, T>(enumerator.Current.Key, (T)enumerator.Current.Value);
        }
    }
}