using System.Collections;
//using static System.Collections.Generic.Dictionary<TKey, TValue>;

namespace BaseLibrary.Collections;

/// <summary>
/// A dictionary that use a ReaderWriterLockerSlim to provide thread safety.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class DictionarySlim<TKey, TValue> : IDictionary, IDictionary<TKey, TValue> where TKey : notnull

    //ICollection, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable, IEnumerable<KeyValuePair<TKey, TValue>>
{
    private readonly Dictionary<TKey, TValue> _dictionary = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public TValue this[TKey key]
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary[key];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            _lock.EnterWriteLock();
            try
            {
                _dictionary[key] = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
    public object? this[object key]
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return ((IDictionary)_dictionary)[key];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            _lock.EnterWriteLock();
            try
            {
                ((IDictionary)_dictionary)[key] = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
    public Dictionary<TKey, TValue>.KeyCollection Keys
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.Keys;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

    }
    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.Keys;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    ICollection IDictionary.Keys
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return ((IDictionary)_dictionary).Keys;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public Dictionary<TKey, TValue>.ValueCollection Values
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.Values;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.Values;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
    ICollection IDictionary.Values
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return ((IDictionary)_dictionary).Values;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public bool IsSynchronized => ((ICollection)_dictionary).IsSynchronized;

    public object SyncRoot => ((ICollection)_dictionary).SyncRoot;

    public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).IsReadOnly;

    public bool IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;



    public List<TKey> GetKeysList()
    {
        _lock.EnterReadLock();
        try
        {
            return _dictionary.Keys.ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    public bool ContainsKey(TKey key)
    {
        _lock.EnterReadLock();
        try
        {
            return _dictionary.ContainsKey(key);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    public bool ContainsKeyUnsafe(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }
    public bool ContainsValue(TValue value)
    {
        _lock.EnterReadLock();
        try
        {
            return _dictionary.ContainsValue(value);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    public bool ContainsValueUnsafe(TValue value)
    {
        return _dictionary.ContainsValue(value);
    }
    public void Add(TKey key, TValue value)
    {
        _lock.EnterWriteLock();
        try
        {
            _dictionary.Add(key, value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    public bool TryAdd(TKey key, TValue value)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_dictionary.ContainsKey(key))
            {
                return false;
            }
            _dictionary.Add(key, value);
            return true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    public bool TryAddOrUpdate(TKey key, TValue value)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key] = value;
                return true;
            }
            _dictionary.Add(key, value);
            return false;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    public bool TryGetValue(TKey key, out TValue value)
    {
        _lock.EnterReadLock();
        try
        {
            return _dictionary.TryGetValue(key, out value);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool Remove(TKey key)
    {
        _lock.EnterWriteLock();
        try
        {
            return _dictionary.Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    public bool RemoveUnsafe(TKey key)
    {
        return _dictionary.Remove(key);
    }
    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _dictionary.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    //public bool ContainsUnsafe(object key)
    //{
    //    return ((IDictionary)_dictionary).Contains(key);
    //}
    //public void RemoveUnsafe(object key)
    //{
    //    ((IDictionary)_dictionary).Remove(key);
    //}
    #region IDictionary
    public void Add(object key, object? value)
    {
        _lock.EnterWriteLock();
        try
        {
            ((IDictionary)_dictionary).Add(key, value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Contains(object key)
    {
        _lock.EnterReadLock();
        try
        {
            return ((IDictionary)_dictionary).Contains(key);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Remove(object key)
    {
        _lock.EnterWriteLock();
        try
        {
            ((IDictionary)_dictionary).Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void CopyTo(Array array, int index)
    {
        _lock.EnterReadLock();
        try
        {
            ((ICollection)_dictionary).CopyTo(array, index);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        _lock.EnterWriteLock();
        try
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Add(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        _lock.EnterReadLock();
        try
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        _lock.EnterReadLock();
        try
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        _lock.EnterWriteLock();
        try
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)_dictionary).GetEnumerator();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            return _dictionary.GetEnumerator();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            return ((IDictionary)_dictionary).GetEnumerator();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
    //IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, Enumerator.DictEntry);
    //public IDictionaryEnumerator GetEnumerator()
    //{
    //    _lock.EnterReadLock();
    //    try
    //    {
    //        return ((IDictionary)_dictionary).GetEnumerator();
    //    }
    //    finally
    //    {
    //        _lock.ExitReadLock();
    //    }
    //}


    //public Enumerator GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

    //IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
    //    Count == 0 ? GenericEmptyEnumerator<KeyValuePair<TKey, TValue>>.Instance :
    //    GetEnumerator();
    #endregion
}
