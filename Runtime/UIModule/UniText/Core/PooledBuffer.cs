using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public struct PooledBuffer<T>
{
    public T[] data;
    public int count;

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => count;
    }

    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => data?.Length ?? 0;
    }

    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref data[i];
    }

    public Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => data.AsSpan(0, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Rent(int capacity)
    {
        if (capacity > 0)
            data = UniTextArrayPool<T>.Rent(capacity);
        count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return()
    {
        if (data != null && data.Length > 0)
        {
            UniTextArrayPool<T>.Return(data);
            data = null;
        }
        count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FakeClear()
    {
        count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (count > 0)
        {
            data.AsSpan(0, count).Clear();
            count = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int required)
    {
        if (Capacity < required)
            Grow(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int required)
    {
        var oldLen = data?.Length ?? 0;
        var newSize = oldLen == 0 ? Math.Max(required, 4) : Math.Max(required, oldLen * 2);
        var newData = UniTextArrayPool<T>.Rent(newSize);
        if (oldLen > 0)
        {
            data.AsSpan(0, count).CopyTo(newData);
            UniTextArrayPool<T>.Return(data);
        }
        data = newData;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (count >= Capacity)
            EnsureCapacity(count + 1);
        data[count++] = item;
    }

    public void RemoveRange(int index, int removeCount)
    {
        if (removeCount <= 0) return;
        count -= removeCount;
        if (index < count)
            Array.Copy(data, index + removeCount, data, index, count - index);
    }

    public void RemoveAt(int index)
    {
        count--;
        if (index < count)
            Array.Copy(data, index + 1, data, index, count - index);
    }

    public void Sort(Comparison<T> comparison)
    {
        if (count > 1)
            Array.Sort(data, 0, count, Comparer<T>.Create(comparison));
    }

    public void Sort(int index, int length, IComparer<T> comparer)
    {
        if (length > 1)
            Array.Sort(data, index, length, comparer);
    }
}

public sealed class PooledList<T>
{
    public PooledBuffer<T> buffer;

    public PooledList()
    {
        buffer = default;
    }

    public PooledList(int capacity)
    {
        buffer = default;
        buffer.Rent(capacity);
    }
    
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => buffer.count;
    }

    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => buffer.Capacity;
    }

    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref buffer[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item) => buffer.Add(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => buffer.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FakeClear() => buffer.FakeClear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return() => buffer.Return();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int required) => buffer.EnsureCapacity(required);

    public void RemoveRange(int index, int removeCount) => buffer.RemoveRange(index, removeCount);

    public void RemoveAt(int index) => buffer.RemoveAt(index);

    public void Sort(Comparison<T> comparison) => buffer.Sort(comparison);

    public void Sort(int index, int length, IComparer<T> comparer) => buffer.Sort(index, length, comparer);
}
