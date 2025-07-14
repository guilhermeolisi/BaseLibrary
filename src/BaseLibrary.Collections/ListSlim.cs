using System.Collections;

namespace BaseLibrary.Collections;

/// <summary>
/// A list that use a ReaderWriterLockerSlim to provide thread safety.
/// </summary>
public class ListSlim<T> : IList<T>, IList
{
    private readonly List<T> _list = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public T this[int index]
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _list[index];
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
                _list[index] = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    object? IList.this[int index]
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return ((IList)_list)[index];
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
                ((IList)_list)[index] = value;
            }
            finally
            {
                _lock.ExitWriteLock();
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
                return _list.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
    public bool IsReadOnly
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return ((IList)_list).IsReadOnly;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public bool IsSynchronized => ((ICollection)_list).IsSynchronized;

    public object SyncRoot => ((ICollection)_list).SyncRoot;

    public bool IsFixedSize => ((IList)_list).IsFixedSize;

    public void Add(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            _list.Add(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public int Add(object? value)
    {
        _lock.EnterWriteLock();
        try
        {
            return ((IList)_list).Add(value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _list.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    public bool Contains(T item)
    {
        _lock.EnterReadLock();
        try
        {
            return _list.Contains(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool Contains(object? value)
    {
        _lock.EnterReadLock();
        try
        {
            return ((IList)_list).Contains(value);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _lock.EnterReadLock();
        try
        {
            _list.CopyTo(array, arrayIndex);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void CopyTo(Array array, int index)
    {
        _lock.EnterReadLock();
        try
        {
            ((ICollection)_list).CopyTo(array, index);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IEnumerator GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            return ((IEnumerable)_list).GetEnumerator();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int IndexOf(object? value)
    {
        _lock.EnterReadLock();
        try
        {
            return ((IList)_list).IndexOf(value);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int IndexOf(T item)
    {
        return ((IList<T>)_list).IndexOf(item);
    }

    public void Insert(int index, object? value)
    {
        _lock.EnterWriteLock();
        try
        {
            ((IList)_list).Insert(index, value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Insert(int index, T item)
    {
        _lock.EnterWriteLock();
        try
        {
            _list.Insert(index, item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Remove(object? value)
    {
        _lock.EnterWriteLock();
        try
        {
            ((IList)_list).Remove(value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Remove(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _list.Remove(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveAt(int index)
    {
        _lock.EnterWriteLock();
        try
        {
            _list.RemoveAt(index);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

    }
    public void RemoveRange(int index, int count)
    {
        _lock.EnterWriteLock();
        try
        {
            _list.RemoveRange(index, count);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            return ((IEnumerable<T>)_list).GetEnumerator();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
