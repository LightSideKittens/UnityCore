using System;

/// <summary>
/// Generic base class for modifiers that work on per-glyph level (Color, Bold, Italic).
/// Handles instance + static buffer pattern, subscription to OnGlyph event.
///
/// Subclasses must implement:
/// - OnGlyphStatic() - static callback for mesh generator
/// - ApplyValue() - fill buffer with values
/// - GetStaticBuffer/SetStaticBuffer - static buffer accessor
/// </summary>
[Serializable]
public abstract class GlyphModifier<T> : BaseModifier where T : unmanaged
{
    protected ArrayPoolBuffer<T> instanceBuffer;

    protected sealed override void CreateBuffers()
    {
        instanceBuffer = new ArrayPoolBuffer<T>(256);
        SetStaticBuffer(instanceBuffer);
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
        instanceBuffer?.ReturnToPool();
        instanceBuffer = null;
    }

    protected sealed override void ClearBuffers() => instanceBuffer.Clear();

    private void OnRebuilding() => SetStaticBuffer(instanceBuffer);

    /// <summary>Return the static OnGlyph callback. Must be static method reference.</summary>
    protected abstract Action GetOnGlyphCallback();

    /// <summary>Set the static buffer reference for OnGlyph callback to use.</summary>
    protected abstract void SetStaticBuffer(ArrayPoolBuffer<T> buffer);
}