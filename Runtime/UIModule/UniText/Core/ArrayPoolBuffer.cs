using System;
using System.Runtime.CompilerServices;


public interface IPoolReturnable
{
    void ReturnToPool();
}


public interface IClearable
{
    void Clear();
}


public sealed class ArrayPoolBuffer<T> : IPoolReturnable, IClearable where T : struct
{
    private T[] data;
    private int capacity;
    private int usedCount;
    private readonly int initialCapacity;


    public T[] Data
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => data;
    }


    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => capacity;
    }


    public int UsedCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => usedCount;
    }


    public ArrayPoolBuffer(int initialCapacity)
    {
        this.initialCapacity = initialCapacity;
        capacity = initialCapacity;
        data = UniTextArrayPool<T>.Rent(initialCapacity);
        data.AsSpan(0, initialCapacity).Clear();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int required)
    {
        if (required > capacity)
            Grow(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int required)
    {
        var newBuffer = UniTextArrayPool<T>.Rent(required);
        data.AsSpan(0, capacity).CopyTo(newBuffer);
        newBuffer.AsSpan(capacity, required - capacity).Clear();
        UniTextArrayPool<T>.Return(data);
        data = newBuffer;
        capacity = required;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (usedCount > 0)
        {
            data.AsSpan(0, usedCount).Clear();
            usedCount = 0;
        }
    }


    public void Reset()
    {
        if (data != null)
            UniTextArrayPool<T>.Return(data);
        data = UniTextArrayPool<T>.Rent(initialCapacity);
        data.AsSpan(0, initialCapacity).Clear();
        capacity = initialCapacity;
    }


    public void ReturnToPool()
    {
        if (data != null)
        {
            UniTextArrayPool<T>.Return(data);
            data = null;
        }

        capacity = 0;
    }


    public void RentFromPool()
    {
        if (data != null)
            UniTextArrayPool<T>.Return(data);
        data = UniTextArrayPool<T>.Rent(initialCapacity);
        data.AsSpan(0, initialCapacity).Clear();
        capacity = initialCapacity;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOrDefault(int index)
    {
        if ((uint)index >= (uint)capacity)
            return default;
        return data[index];
    }


    public void SetRange(int start, int end, T value)
    {
        if (start < 0) start = 0;
        if (end > capacity) end = capacity;
        for (var i = start; i < end; i++)
            data[i] = value;
        if (end > usedCount) usedCount = end;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MarkUsed(int index)
    {
        var next = index + 1;
        if (next > usedCount) usedCount = next;
    }
}


public static class ArrayPoolBufferByteExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlag(this ArrayPoolBuffer<byte> buffer, int index)
    {
        return (uint)index < (uint)buffer.Capacity && buffer.Data[index] != 0;
    }


    public static bool HasAnyFlags(this ArrayPoolBuffer<byte> buffer)
    {
        var data = buffer.Data;
        var cap = buffer.Capacity;
        var i = 0;
        var limit = cap - 7;
        for (; i < limit; i += 8)
            if (data[i] != 0 || data[i + 1] != 0 || data[i + 2] != 0 || data[i + 3] != 0 ||
                data[i + 4] != 0 || data[i + 5] != 0 || data[i + 6] != 0 || data[i + 7] != 0)
                return true;
        for (; i < cap; i++)
            if (data[i] != 0)
                return true;
        return false;
    }


    public static void SetFlagRange(this ArrayPoolBuffer<byte> buffer, int start, int end)
    {
        var data = buffer.Data;
        var cap = buffer.Capacity;
        if (start < 0) start = 0;
        if (end > cap) end = cap;
        for (var i = start; i < end; i++)
            data[i] = 1;
        if (end > 0) buffer.MarkUsed(end - 1);
    }
}


public static class ArrayPoolBufferFloatExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue(this ArrayPoolBuffer<float> buffer, int index)
    {
        return (uint)index < (uint)buffer.Capacity && buffer.Data[index] != 0f;
    }


    public static void SetValueRange(this ArrayPoolBuffer<float> buffer, int start, int end, float value)
    {
        var data = buffer.Data;
        var cap = buffer.Capacity;
        if (start < 0) start = 0;
        if (end > cap) end = cap;
        for (var i = start; i < end; i++)
            data[i] = value;
        if (end > 0) buffer.MarkUsed(end - 1);
    }
}


public static class ArrayPoolBufferUintExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue(this ArrayPoolBuffer<uint> buffer, int index)
    {
        return (uint)index < (uint)buffer.Capacity && buffer.Data[index] != 0;
    }


    public static void SetValueRange(this ArrayPoolBuffer<uint> buffer, int start, int end, uint value)
    {
        var data = buffer.Data;
        var cap = buffer.Capacity;
        if (start < 0) start = 0;
        if (end > cap) end = cap;
        for (var i = start; i < end; i++)
            data[i] = value;
        if (end > 0) buffer.MarkUsed(end - 1);
    }
}