using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Animations;

public static class GenericBindingUtils
{
    private static readonly int s_FlagsOffset = Marshal.OffsetOf(typeof(GenericBinding), "m_Flags").ToInt32();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetFlagsRaw(ref this GenericBinding binding, int rawFlags)
    {
        Unsafe.WriteUnaligned(
            destination: (byte*)Unsafe.AsPointer(ref binding) + s_FlagsOffset,
            value: rawFlags);
    }
}
