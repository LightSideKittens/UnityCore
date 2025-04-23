using System;
using System.Reflection;
using UnityEngine.Animations;

namespace InternalBindings
{
    public static unsafe class GenericBindingUtilityProxy
    {
        public delegate void SetFloatValuesDelegate(
            void* boundProperties,
            int   boundPropertiesCount,
            void* values,
            int   valuesCount);
        
        public static readonly SetFloatValuesDelegate SetFloatValues;
        
        static GenericBindingUtilityProxy()
        {
            var gbType = typeof(GenericBindingUtility);
            var mi = gbType.GetMethod(
                "SetFloatValues",
                BindingFlags.NonPublic | BindingFlags.Static);

            if (mi == null)
                throw new MissingMethodException(gbType.FullName, "SetFloatValues");
            SetFloatValues = (SetFloatValuesDelegate)mi.CreateDelegate(typeof(SetFloatValuesDelegate));
        }
    }
}