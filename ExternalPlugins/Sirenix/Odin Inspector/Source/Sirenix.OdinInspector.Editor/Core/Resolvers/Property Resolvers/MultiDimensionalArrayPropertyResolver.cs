//-----------------------------------------------------------------------
// <copyright file="MultiDimensionalArrayPropertyResolver.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections;

    [ResolverPriority(-1)]
    public sealed class MultiDimensionalArrayPropertyResolver<T> : OdinPropertyResolver<T> where T : IList
    {
        public override bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            var type = property.ValueEntry.TypeOfValue;
            return type.IsArray && type.GetArrayRank() > 1;
        }

        public override int ChildNameToIndex(string name)
        {
            return -1;
        }

        public override int ChildNameToIndex(ref StringSlice name)
        {
            return -1;
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            throw new NotSupportedException();
        }

        protected override int GetChildCount(T value)
        {
            return 0;
        }
    }
}
#endif