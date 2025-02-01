using System;
using System.Collections.Generic;

public class BoundedLinkedList<T> : LinkedList<T>
{
    private readonly int _maxSize;
    public event Action<T> FirstRemoved;
    public event Action<T> LastRemoved;
    public event Action<T> Removed;
    public event Action<T> Added;
    public event Action<T> FirstAdded;
    public event Action<T> LastAdded;
    
    public int MaxSize => _maxSize;

    public BoundedLinkedList(int maxSize)
    {
        _maxSize = maxSize;
        FirstRemoved = OnRemoved;
        LastRemoved = OnRemoved;
        FirstAdded = OnAdded;
        LastAdded = OnAdded;
    }

    private void OnRemoved(T value)
    {
        Removed?.Invoke(value);
    }
    
    private void OnAdded(T value)
    {
        Added?.Invoke(value);
    }
    
    public new LinkedListNode<T> AddFirst(T value)
    {
        if (Count == _maxSize)
        {
            RemoveLast();
        }
        var first = base.AddFirst(value);
        FirstAdded(value);
        return first;
    }

    public new LinkedListNode<T> AddLast(T value)
    {
        if (Count == _maxSize)
        {
            RemoveFirst();
        }
        var last = base.AddLast(value);
        LastAdded(value);
        return last;
    }

    public new void RemoveFirst()
    {
        if(Count == 0) return;
        var data = First.Value;
        base.RemoveFirst();
        FirstRemoved(data);
    }
    
    public new void RemoveLast()
    {
        if(Count == 0) return;
        var data = Last.Value;
        base.RemoveLast();
        LastRemoved(data);
    }

    public new void Clear()
    {
        if (Removed != null)
        {
            foreach (var item in this)
            {
                Removed(item);
            }
        }
        base.Clear();
    }

    public new void Remove(T value)
    {
        base.Remove(value);
        Removed?.Invoke(value);
    }
}