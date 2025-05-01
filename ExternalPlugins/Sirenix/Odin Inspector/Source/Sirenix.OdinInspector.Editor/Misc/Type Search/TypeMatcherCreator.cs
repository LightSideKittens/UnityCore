//-----------------------------------------------------------------------
// <copyright file="TypeMatcherCreator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    public abstract class TypeMatcherCreator
    {
        public abstract bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher);
    }
}
#endif