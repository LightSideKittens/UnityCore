using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class UnderlineModifier : BaseLineModifier
{
    private static ArrayPoolBuffer<byte> buffer;

    protected override string AttributeKey => AttributeKeys.Underline;

    protected override float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale)
    {
        return faceInfo.underlineOffset * scale;
    }

    protected override void SetStaticBuffer(ArrayPoolBuffer<byte> buf)
    {
        buffer = buf;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasUnderline(int cluster)
    {
        return buffer != null && buffer.HasFlag(cluster);
    }


    public static void SetFlagRange(int start, int end)
    {
        if (buffer == null) return;
        buffer.EnsureCapacity(end);
        buffer.SetFlagRange(start, end);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        buffer = null;
    }
}