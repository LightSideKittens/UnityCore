using UnityEngine;


public readonly struct TextHitResult
{
    public readonly bool hit;


    public readonly int glyphIndex;


    public readonly int cluster;


    public readonly Vector2 glyphPosition;


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