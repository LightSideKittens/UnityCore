using System;

/// <summary>
/// Generic base class for modifiers that work on per-glyph level (Color, Bold, Italic).
/// Uses shared buffers from CommonData with reference counting.
/// Multiple modifiers of the same type share one buffer.
///
/// Subclasses must implement:
/// - AttributeKey - unique key for shared buffer (use AttributeKeys constants)
/// - GetOnGlyphCallback() - static callback for mesh generator
/// - SetStaticBuffer() - set static buffer reference for callback
/// </summary>
[Serializable]
public abstract class GlyphModifier<T> : BaseModifier where T : unmanaged
{
    // Cached reference to shared buffer from CommonData
    protected ArrayPoolBuffer<T> buffer;

    /// <summary>
    /// Unique key for this attribute type. Use AttributeKeys constants.
    /// </summary>
    protected abstract string AttributeKey { get; }

    protected sealed override void CreateBuffers()
    {
        // Acquire shared buffer from CommonData (increases refCount)
        buffer = CommonData.Current.AcquireAttribute<T>(AttributeKey);
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
        // Release reference (decreases refCount, returns to pool when 0)
        CommonData.Current?.ReleaseAttribute(AttributeKey);
        buffer = null;
    }

    // Clear is handled by CommonData.ClearAllAttributes() in Reset()
    protected sealed override void ClearBuffers() { }

    private void OnRebuilding()
    {
        // Update cached reference (without changing refCount)
        buffer = CommonData.Current.GetAttribute<T>(AttributeKey);
        SetStaticBuffer(buffer);
    }

    /// <summary>Return the static OnGlyph callback. Must be static method reference.</summary>
    protected abstract Action GetOnGlyphCallback();

    /// <summary>Set the static buffer reference for OnGlyph callback to use.</summary>
    protected abstract void SetStaticBuffer(ArrayPoolBuffer<T> buffer);
}
