using System;


[Serializable]
public abstract class GlyphModifier<T> : BaseModifier where T : unmanaged
{
    protected ArrayPoolBuffer<T> buffer;


    protected abstract string AttributeKey { get; }

    protected sealed override void CreateBuffers()
    {
        var cpCount = buffers.codepointCount;
        buffer = buffers.AcquireAttribute<T>(AttributeKey, cpCount);
        SetStaticBuffer(buffer);
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
        buffers.ReleaseAttribute(AttributeKey);
        buffer = null;
    }

    protected sealed override void ClearBuffers()
    {
    }

    private void OnRebuilding()
    {
        buffer = buffers.GetAttribute<T>(AttributeKey);
        SetStaticBuffer(buffer);
    }


    protected abstract Action GetOnGlyphCallback();


    protected abstract void SetStaticBuffer(ArrayPoolBuffer<T> buffer);
}