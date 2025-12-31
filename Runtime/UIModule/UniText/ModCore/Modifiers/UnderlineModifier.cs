using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class UnderlineModifier : BaseLineModifier
{
    [ThreadStatic] private static byte[] buffer;

    protected override string AttributeKey => AttributeKeys.Underline;

    protected override float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale)
    {
        return faceInfo.underlineOffset * scale;
    }

    protected override void SetStaticBuffer(byte[] buf)
    {
        buffer = buf;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasUnderline(int cluster)
    {
        return buffer.HasFlag(cluster);
    }
}
