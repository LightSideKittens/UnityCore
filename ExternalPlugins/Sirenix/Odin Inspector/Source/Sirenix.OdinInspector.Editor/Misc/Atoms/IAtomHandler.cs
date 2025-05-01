//-----------------------------------------------------------------------
// <copyright file="IAtomHandler.cs" company="Sirenix ApS">
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

    using System;

    public interface IAtomHandler
    {
        Type AtomType { get; }

        object CreateInstance();

        void Copy(ref object from, ref object to);

        bool Compare(object a, object b);
    }

    public interface IAtomHandler<T> : IAtomHandler
    {
        new T CreateInstance();

        void Copy(ref T from, ref T to);

        bool Compare(T a, T b);
    }
}
#endif