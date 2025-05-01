//-----------------------------------------------------------------------
// <copyright file="RectPropertyResolver.cs" company="Sirenix ApS">
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
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class RectPropertyResolver : OdinPropertyResolver<Rect>
    {
        public override int ChildNameToIndex(string name)
        {
            StringSlice slice = name;
            return this.ChildNameToIndex(ref slice);
        }

        public override int ChildNameToIndex(ref StringSlice name)
        {
            if (name == "position")
            {
                return 0;
            }
            else if (name == "size")
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            if (childIndex == 0)
            {
                var member = typeof(Rect).GetProperty("position");
                List<Attribute> attributes = new List<Attribute>();

                return InspectorPropertyInfo.CreateForMember(this.Property, member, true, attributes);
            }
            else if (childIndex == 1)
            {
                var member = typeof(Rect).GetProperty("size");
                List<Attribute> attributes = new List<Attribute>();

                return InspectorPropertyInfo.CreateForMember(this.Property, member, true, attributes);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected override int GetChildCount(Rect value)
        {
            return 2;
        }
    }
}
#endif