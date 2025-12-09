using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Хелпер для управления статическими буферами из ArrayPool.
/// Инкапсулирует логику Rent/Return/Grow/Clear.
///
/// Использование:
/// private static ArrayPoolBuffer&lt;float&gt; buffer = new(256);
///
/// // В Apply():
/// buffer.EnsureCapacity(cpCount);
/// buffer.Data[i] = value;
///
/// // В Reset():
/// buffer.Clear();
///
/// // В OnDomainReload():
/// buffer.Reset();
/// </summary>
public struct ArrayPoolBuffer<T> where T : struct
{
    private T[] data;
    private int capacity;
    private readonly int initialCapacity;

    /// <summary>Текущий массив данных.</summary>
    public T[] Data
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => data;
    }

    /// <summary>Текущая ёмкость буфера.</summary>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => capacity;
    }

    /// <summary>
    /// Создаёт буфер с указанной начальной ёмкостью.
    /// Массив сразу очищается (ArrayPool не гарантирует нули).
    /// </summary>
    public ArrayPoolBuffer(int initialCapacity)
    {
        this.initialCapacity = initialCapacity;
        this.capacity = initialCapacity;
        this.data = ArrayPool<T>.Shared.Rent(initialCapacity);
        this.data.AsSpan(0, initialCapacity).Clear();
    }

    /// <summary>
    /// Увеличивает буфер если нужно. Новая часть очищается.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int required)
    {
        if (required > capacity)
            Grow(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int required)
    {
        var newBuffer = ArrayPool<T>.Shared.Rent(required);
        data.AsSpan(0, capacity).CopyTo(newBuffer);
        // Очищаем новую часть (ArrayPool не гарантирует нули)
        newBuffer.AsSpan(capacity, required - capacity).Clear();
        ArrayPool<T>.Shared.Return(data);
        data = newBuffer;
        capacity = required;
    }

    /// <summary>
    /// Очищает буфер (заполняет нулями).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        data.AsSpan(0, capacity).Clear();
    }

    /// <summary>
    /// Сбрасывает буфер к начальному состоянию.
    /// Вызывать в OnDomainReload.
    /// </summary>
    public void Reset()
    {
        if (data != null)
            ArrayPool<T>.Shared.Return(data);
        data = ArrayPool<T>.Shared.Rent(initialCapacity);
        data.AsSpan(0, initialCapacity).Clear();
        capacity = initialCapacity;
    }

    /// <summary>
    /// Проверяет есть ли ненулевое значение по индексу.
    /// Безопасно для индексов вне диапазона.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasValue(int index)
    {
        if ((uint)index >= (uint)capacity)
            return false;
        // Для примитивных типов != default работает
        return !EqualityComparer<T>.Default.Equals(data[index], default);
    }

    /// <summary>
    /// Получает значение по индексу или default если вне диапазона.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOrDefault(int index)
    {
        if ((uint)index >= (uint)capacity)
            return default;
        return data[index];
    }

    /// <summary>
    /// Устанавливает значение для диапазона [start, end).
    /// </summary>
    public void SetRange(int start, int end, T value)
    {
        if (start < 0) start = 0;
        if (end > capacity) end = capacity;
        for (int i = start; i < end; i++)
            data[i] = value;
    }
}

/// <summary>
/// Специализированные методы для byte буферов (флаги).
/// </summary>
public static class ArrayPoolBufferByteExtensions
{
    /// <summary>Проверяет установлен ли флаг.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlag(this ref ArrayPoolBuffer<byte> buffer, int index)
    {
        return (uint)index < (uint)buffer.Capacity && buffer.Data[index] != 0;
    }

    /// <summary>Устанавливает флаг для диапазона.</summary>
    public static void SetFlagRange(this ref ArrayPoolBuffer<byte> buffer, int start, int end)
    {
        var data = buffer.Data;
        int cap = buffer.Capacity;
        if (start < 0) start = 0;
        if (end > cap) end = cap;
        for (int i = start; i < end; i++)
            data[i] = 1;
    }
}

/// <summary>
/// Специализированные методы для float буферов.
/// </summary>
public static class ArrayPoolBufferFloatExtensions
{
    /// <summary>Проверяет есть ли ненулевое значение.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue(this ref ArrayPoolBuffer<float> buffer, int index)
    {
        return (uint)index < (uint)buffer.Capacity && buffer.Data[index] != 0f;
    }

    /// <summary>Устанавливает значение для диапазона.</summary>
    public static void SetValueRange(this ref ArrayPoolBuffer<float> buffer, int start, int end, float value)
    {
        var data = buffer.Data;
        int cap = buffer.Capacity;
        if (start < 0) start = 0;
        if (end > cap) end = cap;
        for (int i = start; i < end; i++)
            data[i] = value;
    }
}

/// <summary>
/// Специализированные методы для uint буферов (packed colors).
/// </summary>
public static class ArrayPoolBufferUintExtensions
{
    /// <summary>Проверяет есть ли ненулевое значение.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue(this ref ArrayPoolBuffer<uint> buffer, int index)
    {
        return (uint)index < (uint)buffer.Capacity && buffer.Data[index] != 0;
    }

    /// <summary>Устанавливает значение для диапазона.</summary>
    public static void SetValueRange(this ref ArrayPoolBuffer<uint> buffer, int start, int end, uint value)
    {
        var data = buffer.Data;
        int cap = buffer.Capacity;
        if (start < 0) start = 0;
        if (end > cap) end = cap;
        for (int i = start; i < end; i++)
            data[i] = value;
    }
}
