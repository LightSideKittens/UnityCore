//-----------------------------------------------------------------------
// <copyright file="AtomAndEnumPropertyResolver.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    [ResolverPriority(1000)]
    public class AtomAndEnumPropertyResolver<TValue> : OdinPropertyResolver<TValue>, IMaySupportPrefabModifications
    {
        public bool MaySupportPrefabModifications { get { return true; } }

        public override int ChildNameToIndex(string name)
        {
            return -1;
        }

        public override int ChildNameToIndex(ref StringSlice name)
        {
            return -1;
        }

        public override bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            var type = property.ValueEntry.TypeOfValue;
            return type.IsEnum || AtomHandlerLocator.IsMarkedAtomic(type);
        }

        protected override int GetChildCount(TValue value)
        {
            return 0;
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            throw new System.NotSupportedException();
        }
    }
}
#endif