using UnityEngine;

/// <summary>
/// Результат hit test по тексту.
/// </summary>
public readonly struct TextHitResult
{
    /// <summary>Попадание в текст.</summary>
    public readonly bool hit;

    /// <summary>Индекс глифа в positionedGlyphs.</summary>
    public readonly int glyphIndex;

    /// <summary>Индекс кластера (codepoint) в clean text.</summary>
    public readonly int cluster;

    /// <summary>Позиция глифа.</summary>
    public readonly Vector2 glyphPosition;

    /// <summary>Расстояние до глифа.</summary>
    public readonly float distance;

    public static readonly TextHitResult None = new();

    public TextHitResult(int glyphIndex, int cluster, Vector2 glyphPosition, float distance)
    {
        hit = true;
        this.glyphIndex = glyphIndex;
        this.cluster = cluster;
        this.glyphPosition = glyphPosition;
        this.distance = distance;
    }
}
