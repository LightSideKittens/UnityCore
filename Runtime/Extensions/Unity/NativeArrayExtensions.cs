using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace LSCore.Extensions.Unity
{
    public static class NativeArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Write<T>(ref this NativeArray<T> arr, int index, in T value)
            where T : unmanaged
        {
            void* basePtr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(arr);
            UnsafeUtility.WriteArrayElement(basePtr, index, value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void* GetPtr<T>(ref this NativeArray<T> arr) where T : struct
        { 
            return NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(arr);
        }
    }
}