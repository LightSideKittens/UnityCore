using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Модификатор Underline текста.
/// Рисует линию под текстом на уровне underlineOffset из шрифта.
/// </summary>
[Serializable]
public class UnderlineModifier : BaseLineModifier
{
    // Instance буферы (реальные данные)
    private ArrayPoolBuffer<byte> instanceFlagsBuffer;
    private LineSegment[] instanceLineSegments;
    private int instanceLineSegmentsCapacity;
    private int instanceLineSegmentCount;
    private bool instanceLinesDrawnThisFrame;

    // Статический указатель только для HasUnderline (внешний API)
    private static ArrayPoolBuffer<byte> currentFlagsBuffer;

    // Свойства работают с INSTANCE полями
    protected override ArrayPoolBuffer<byte> FlagsBuffer => instanceFlagsBuffer;
    protected override LineSegment[] LineSegments { get => instanceLineSegments; set => instanceLineSegments = value; }
    protected override int LineSegmentsCapacity { get => instanceLineSegmentsCapacity; set => instanceLineSegmentsCapacity = value; }
    protected override int LineSegmentCount { get => instanceLineSegmentCount; set => instanceLineSegmentCount = value; }
    protected override bool LinesDrawnThisFrame { get => instanceLinesDrawnThisFrame; set => instanceLinesDrawnThisFrame = value; }

    protected override float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale)
    {
        return faceInfo.underlineOffset * scale;
    }

    protected override void CreateInstanceBuffers()
    {
        instanceFlagsBuffer = new ArrayPoolBuffer<byte>(256);
        instanceLineSegments = ArrayPool<LineSegment>.Shared.Rent(64);
        instanceLineSegmentsCapacity = 64;
        instanceLineSegmentCount = 0;
        instanceLinesDrawnThisFrame = false;
    }

    protected override void SetStaticBuffers()
    {
        // Только для внешнего API (HasUnderline)
        currentFlagsBuffer = instanceFlagsBuffer;
    }

    protected override void ReturnBuffersToPool()
    {
        instanceFlagsBuffer?.ReturnToPool();
        instanceFlagsBuffer = null;
        if (instanceLineSegments != null)
        {
            ArrayPool<LineSegment>.Shared.Return(instanceLineSegments);
            instanceLineSegments = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasUnderline(int cluster) => currentFlagsBuffer != null && currentFlagsBuffer.HasFlag(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        currentFlagsBuffer = null;
    }
}
