//-----------------------------------------------------------------------
// <copyright file="CollectionResolverUtilities.cs" company="Sirenix ApS">
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

    public static class CollectionResolverUtilities
    {
        public static string DefaultIndexToChildName(int index)
        {
            return "$" + index;
        }

        public static int DefaultChildNameToIndex(ref StringSlice name)
        {
            if (name.Length <= 1) return -1;

            int index;

            StringSlice numberSlice;
            if (name[0] == '$')
            {
                numberSlice = name.Slice(1);
            }
            else if (name.Length > 2 && name[0] == '[' && name[name.Length - 1] == ']')
            {
                numberSlice = name.Slice(1, name.Length - 2);
            }
            else return -1;

            if (numberSlice.TryParseToInt(out index) && index >= 0)
            {
                return index;
            }

            return -1;
        }

        public static int DefaultChildNameToIndex(string name)
        {
            StringSlice slice = name;
            return DefaultChildNameToIndex(ref slice);
        }
    }
}
#endif