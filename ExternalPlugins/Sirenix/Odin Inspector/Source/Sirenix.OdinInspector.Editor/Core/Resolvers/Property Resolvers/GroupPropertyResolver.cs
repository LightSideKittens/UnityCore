//-----------------------------------------------------------------------
// <copyright file="GroupPropertyResolver.cs" company="Sirenix ApS">
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
    using System.Collections.Generic;

    [ResolverPriority(-5)]
    public class GroupPropertyResolver : OdinPropertyResolver
    {
        private InspectorPropertyInfo[] groupInfos;
        private Dictionary<StringSlice, int> nameToIndexMap = new Dictionary<StringSlice, int>(StringSliceEqualityComparer.Instance);

        public override bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            return property.Info.PropertyType == PropertyType.Group;
        }

        protected override void Initialize()
        {
            base.Initialize();

            this.groupInfos = this.Property.Info.GetGroupInfos();

            for (int i = 0; i < this.groupInfos.Length; i++)
            {
                this.nameToIndexMap[this.groupInfos[i].PropertyName] = i;
            }
        }

        public override int ChildNameToIndex(string name)
        {
            int index;
            if (this.nameToIndexMap.TryGetValue(name, out index)) return index;
            return -1;
        }

        public override int ChildNameToIndex(ref StringSlice name)
        {
            int index;
            if (this.nameToIndexMap.TryGetValue(name, out index)) return index;
            return -1;
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            return this.groupInfos[childIndex];
        }

        protected override int CalculateChildCount()
        {
            return this.groupInfos.Length;
        }
    }
}
#endif