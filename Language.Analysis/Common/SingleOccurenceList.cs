using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Language.Analysis.Common;

public sealed class SingleOccurenceList<T> : IList<T>
{
    readonly List<T> _list = new();
    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        if (!_list.Contains(item))
        {
            _list.Add(item);
            return;
        }
        throw new InvalidOperationException("Item already exists in the list");
    }

    public void Clear() => _list.Clear();
    public bool Contains(T item) => _list.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public bool Remove(T item) => _list.Remove(item);

    public int Count => _list.Count;
    public bool IsReadOnly => false;
    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        if (!_list.Contains(item))
        {
            _list.Insert(index, item);
            return;
        }
        throw new InvalidOperationException("Item already exists in the list");
    }

    public void RemoveAt(int index) => _list.RemoveAt(index);

    public T this[int index]
    {
        get => _list[index];
        set
        {
            if (!_list.Contains(value))
            {
                _list[index] = value;
                return;
            }
            throw new InvalidOperationException("Item already exists in the list");
        }
    }
}