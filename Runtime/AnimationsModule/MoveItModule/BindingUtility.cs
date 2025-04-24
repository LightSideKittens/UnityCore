using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Animations;

namespace LSCore.AnimationsModule
{
    public static unsafe class BindingUtility
    {
        public delegate void SetValuesDelegate(
            void* boundProperties,
            int   boundPropertiesCount,
            void* values,
            int   valuesCount);
        
        public static readonly SetValuesDelegate SetFloatValues;
        public static readonly SetValuesDelegate SetDiscreteValues;
        
        static BindingUtility()
        {
            var gbType = typeof(GenericBindingUtility);
            var mi = gbType.GetMethod(
                "SetFloatValues",
                BindingFlags.NonPublic | BindingFlags.Static);
            
            var mi2 = gbType.GetMethod(
                "SetDiscreteValues",
                BindingFlags.NonPublic | BindingFlags.Static);
            
            SetFloatValues = (SetValuesDelegate)mi.CreateDelegate(typeof(SetValuesDelegate));
            SetDiscreteValues = (SetValuesDelegate)mi2.CreateDelegate(typeof(SetValuesDelegate));
        }
        
        public static unsafe void SetValues(
            NativeArray<BoundProperty> boundProperties,
            NativeArray<float> values)
        {
            var unsafePtr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(boundProperties);
            var pointerWithoutChecks = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(values);
            SetFloatValues(unsafePtr, boundProperties.Length, pointerWithoutChecks, values.Length);
        }
    
        public static unsafe void SetValues(
            NativeArray<BoundProperty> boundProperties,
            NativeArray<int> values)
        {
            var unsafePtr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(boundProperties);
            var pointerWithoutChecks = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(values);
            SetDiscreteValues(unsafePtr, boundProperties.Length, pointerWithoutChecks, values.Length);
        }
    }
}