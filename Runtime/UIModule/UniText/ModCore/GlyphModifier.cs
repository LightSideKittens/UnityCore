using System;
using System.Runtime.CompilerServices;


[Serializable]
public abstract class GlyphModifier<T> : BaseModifier where T : unmanaged
{
    protected PooledArrayAttribute<T> attribute;

    protected abstract string AttributeKey { get; }

    protected sealed override void CreateBuffers()
    {
        attribute = buffers.GetOrCreateAttributeData<PooledArrayAttribute<T>>(AttributeKey);
        var cpCount = buffers.codepoints.count;
        attribute.EnsureCount(cpCount);
        SetStaticBuffer(attribute.buffer.data);
    }

    protected sealed override void Subscribe()
    {
        uniText.Rebuilding += OnRebuilding;
        uniText.MeshGenerator.OnGlyph += GetOnGlyphCallback();
    }

    protected sealed override void Unsubscribe()
    {
        uniText.Rebuilding -= OnRebuilding;
        uniText.MeshGenerator.OnGlyph -= GetOnGlyphCallback();
    }

    protected sealed override void ReleaseBuffers()
    {
        SetStaticBuffer(null);
        buffers.ReleaseAttributeData(AttributeKey);
        attribute = null;
    }

    protected sealed override void ClearBuffers()
    {
    }

    private void OnRebuilding()
    {
        attribute = buffers.GetAttributeData<PooledArrayAttribute<T>>(AttributeKey);
        SetStaticBuffer(attribute?.buffer.data);
    }

    protected void EnsureBufferCount(int required)
    {
        attribute ??= buffers.GetOrCreateAttributeData<PooledArrayAttribute<T>>(AttributeKey);
        attribute.EnsureCount(required);
        SetStaticBuffer(attribute.buffer.data);
    }

    protected abstract Action GetOnGlyphCallback();

    protected abstract void SetStaticBuffer(T[] buffer);
}


public static class ModifierBufferExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlag(this byte[] buffer, int index)
    {
        return buffer != null && (uint)index < (uint)buffer.Length && buffer[index] != 0;
    }

    public static bool HasAnyFlags(this byte[] buffer)
    {
        if (buffer == null) return false;
        var len = buffer.Length;
        var i = 0;
        var limit = len - 7;
        for (; i < limit; i += 8)
            if (buffer[i] != 0 || buffer[i + 1] != 0 || buffer[i + 2] != 0 || buffer[i + 3] != 0 ||
                buffer[i + 4] != 0 || buffer[i + 5] != 0 || buffer[i + 6] != 0 || buffer[i + 7] != 0)
                return true;
        for (; i < len; i++)
            if (buffer[i] != 0)
                return true;
        return false;
    }

    public static void SetFlagRange(this byte[] buffer, int start, int end)
    {
        if (buffer == null) return;
        var len = buffer.Length;
        if (start < 0) start = 0;
        if (end > len) end = len;
        for (var i = start; i < end; i++)
            buffer[i] = 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue(this uint[] buffer, int index)
    {
        return buffer != null && (uint)index < (uint)buffer.Length && buffer[index] != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetValueOrDefault(this uint[] buffer, int index)
    {
        if (buffer == null || (uint)index >= (uint)buffer.Length)
            return 0;
        return buffer[index];
    }

    public static void SetValueRange(this uint[] buffer, int start, int end, uint value)
    {
        if (buffer == null) return;
        var len = buffer.Length;
        if (start < 0) start = 0;
        if (end > len) end = len;
        for (var i = start; i < end; i++)
            buffer[i] = value;
    }
}
