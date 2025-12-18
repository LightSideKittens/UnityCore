using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Интерфейс для возврата буфера в пул.
/// </summary>
public interface IPoolReturnable
{
    void ReturnToPool();
}

/// <summary>
/// Интерфейс для очистки буфера.
/// </summary>
public interface IClearable
{
    void Clear();
}

/// <summary>
/// Хелпер для управления буферами из ArrayPool.
/// Инкапсулирует логику Rent/Return/Grow/Clear.
///
/// Использование:
/// private ArrayPoolBuffer&lt;float&gt; buffer = new(256);
///
/// // В Apply():
/// buffer.EnsureCapacity(cpCount);
/// buffer.Data[i] = value;
///
/// // В Reset():
/// buffer.Clear();
///
/// // В Deinitialize():
/// buffer.ReturnToPool();
/// </summary>
public sealed class ArrayPoolBuffer<T> : IPoolReturnable, IClearable where T : struct
{
    private T[] data;
    private int capacity;
    private int usedCount;  // отслеживаем максимальный использованный индекс
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

    /// <summary>Максимальный использованный индекс с последнего Clear.</summary>
    public int UsedCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => usedCount;
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
    /// Очищает только использованную часть буфера.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (usedCount > 0)
        {
            data.AsSpan(0, usedCount).Clear();
            usedCount = 0;
        }
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
    /// Возвращает массив в пул и обнуляет буфер.
    /// Вызывать в Deinitialize модификатора.
    /// </summary>
    public void ReturnToPool()
    {
        if (data != null)
        {
            ArrayPool<T>.Shared.Return(data);
            data = null;
        }
        capacity = 0;
    }

    /// <summary>
    /// Инициализирует буфер заново после ReturnToPool.
    /// </summary>
    public void RentFromPool()
    {
        if (data != null)
            ArrayPool<T>.Shared.Return(data);
        data = ArrayPool<T>.Shared.Rent(initialCapacity);
        data.AsSpan(0, initialCapacity).Clear();
        capacity = initialCapacity;
    }

    // NOTE: HasValue removed - use specialized extension methods instead:
    // - ArrayPoolBufferByteExtensions.HasFlag() for byte buffers
    // - ArrayPoolBufferUintExtensions.HasValue() for uint buffers
    // - ArrayPoolBufferFloatExtensions.HasValue() for float buffers
    // Generic EqualityComparer<T>.Default.Equals() is slow for primitives!

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
        if (end > usedCount) usedCount = end;
    }

    /// <summary>
    /// Обновляет usedCount если index >= usedCount.
    /// Вызывается при записи отдельного элемента.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MarkUsed(int index)
    {
        int next = index + 1;
        if (next > usedCount) usedCount = next;
    }
}

/// <summary>
/// Специализированные методы для byte буферов (флаги).
/// </summary>
public static class ArrayPoolBufferByteExtensions
{
    /// <summary>Проверяет установлен ли флаг.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlag(this ArrayPoolBuffer<byte> buffer, int index)
    {
        return (uint)index < (uint)buffer.Capacity && buffer.Data[index] != 0;
    }

    /// <summary>Проверяет есть ли хотя бы один установленный флаг в буфере.</summary>
    public static bool HasAnyFlags(this ArrayPoolBuffer<byte> buffer)
    {
        var data = buffer.Data;
        int cap = buffer.Capacity;
        int i = 0;
        int limit = cap - 7;
        for (; i < limit; i += 8)
        {
            if (data[i] != 0 || data[i + 1] != 0 || data[i + 2] != 0 || data[i + 3] != 0 ||
                data[i + 4] != 0 || data[i + 5] != 0 || data[i + 6] != 0 || data[i + 7] != 0)
                return true;
        }
        for (; i < cap; i++)
        {
            if (data[i] != 0)
                return true;
        }
        return false;
    }

    /// <summary>Устанавливает флаг для диапазона.</summary>
    public static void SetFlagRange(this ArrayPoolBuffer<byte> buffer, int start, int end)
    {
        var data = buffer.Data;
        int cap = buffer.Capacity;
        if (start < 0) start = 0;
        if (end > cap) end = cap;
        for (int i = start; i < end; i++)
            data[i] = 1;
        if (end > 0) buffer.MarkUsed(end - 1);
    }
}

/// <summary>
/// Специализированные методы для float буферов.
/// </summary>
public static class ArrayPoolBufferFloatExtensions
{
    /// <summary>Проверяет есть ли ненулевое значение.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue(this ArrayPoolBuffer<float> buffer, int index)
    {
        return (uint)index < (uint)buffer.Capacity && buffer.Data[index] != 0f;
    }

    /// <summary>Устанавливает значение для диапазона.</summary>
    public static void SetValueRange(this ArrayPoolBuffer<float> buffer, int start, int end, float value)
    {
        var data = buffer.Data;
        int cap = buffer.Capacity;
        if (start < 0) start = 0;
        if (end > cap) end = cap;
        for (int i = start; i < end; i++)
            data[i] = value;
        if (end > 0) buffer.MarkUsed(end - 1);
    }
}

/// <summary>
/// Специализированные методы для uint буферов (packed colors).
/// </summary>
public static class ArrayPoolBufferUintExtensions
{
    /// <summary>Проверяет есть ли ненулевое значение.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue(this ArrayPoolBuffer<uint> buffer, int index)
    {
        return (uint)index < (uint)buffer.Capacity && buffer.Data[index] != 0;
    }

    /// <summary>Устанавливает значение для диапазона.</summary>
    public static void SetValueRange(this ArrayPoolBuffer<uint> buffer, int start, int end, uint value)
    {
        var data = buffer.Data;
        int cap = buffer.Capacity;
        if (start < 0) start = 0;
        if (end > cap) end = cap;
        for (int i = start; i < end; i++)
            data[i] = value;
        if (end > 0) buffer.MarkUsed(end - 1);
    }
}
