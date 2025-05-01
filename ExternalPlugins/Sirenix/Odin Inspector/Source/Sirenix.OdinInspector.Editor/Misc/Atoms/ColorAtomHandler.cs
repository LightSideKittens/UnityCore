//-----------------------------------------------------------------------
// <copyright file="ColorAtomHandler.cs" company="Sirenix ApS">
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

    using UnityEngine;

    [AtomHandler]
    public sealed class ColorAtomHandler : EquatableStructAtomHandler<Color>
    {
    }

    [AtomHandler]
    public sealed class Color32AtomHandler : EquatableStructAtomHandler<Color32>
    {
    }
}
#endif