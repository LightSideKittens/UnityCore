using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class StrikethroughModifier : BaseLineModifier
{
    [ThreadStatic] private static byte[] buffer;

    protected override string AttributeKey => AttributeKeys.Strikethrough;

    protected override float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale)
    {
        if (faceInfo.strikethroughOffset != 0)
            return faceInfo.strikethroughOffset * scale;

        var xHeightMid = faceInfo.meanLine > 0
            ? faceInfo.meanLine * 0.5f
            : faceInfo.ascentLine * 0.35f;
        return xHeightMid * scale;
    }

    protected override void SetStaticBuffer(byte[] buf)
    {
        buffer = buf;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasStrikethrough(int cluster)
    {
        return buffer.HasFlag(cluster);
    }
}
