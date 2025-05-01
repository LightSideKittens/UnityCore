//-----------------------------------------------------------------------
// <copyright file="EquatableStructAtomHandler.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;

    public abstract class EquatableStructAtomHandler<T> : BaseAtomHandler<T> where T : struct
    {
        private static readonly Func<T, T, bool> Comparer;

        static EquatableStructAtomHandler()
        {
            Comparer = TypeExtensions.GetEqualityComparerDelegate<T>();
        }

        protected override bool CompareImplementation(T a, T b)
        {
            return Comparer(a, b);
        }

        protected override void CopyImplementation(ref T from, ref T to)
        {
            to = from;
        }

        public override T CreateInstance()
        {
            return default(T);
        }
    }
}
#endif