//-----------------------------------------------------------------------
// <copyright file="AttributeListExtensions.cs" company="Sirenix ApS">
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
    using System.Collections.Generic;

    /// <summary>
    /// Extension method for List&lt;Attribute&gt;
    /// </summary>
    public static class AttributeListExtensions
    {
        /// <summary>
        /// Determines whether the list contains a specific attribute type.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns>
        ///   <c>true</c> if the specified attribute list has attribute; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasAttribute<T>(this IList<Attribute> attributeList)
            where T : Attribute
        {
            var count = attributeList.Count;

            for (int i = 0; i < count; i++)
                if (attributeList[i] is T)
                    return true;

            return false;
        }

        /// <summary>
        /// Adds a new instance of the given type of attribute if it's not in the list.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns></returns>
        public static T GetOrAddAttribute<T>(this List<Attribute> attributeList)
            where T : Attribute, new()
        {
            var count = attributeList.Count;
            T attr;

            for (int i = 0; i < count; i++)
            {
                attr = attributeList[i] as T;

                if (attr != null)
                    return attr;
            }

            attr = new T();
            attributeList.Add(attr);
            return attr;
        }

        /// <summary>
        /// Gets the first instance of an attribute of the given type in the list.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns></returns>
        public static T GetAttribute<T>(this IList<Attribute> attributeList)
            where T : Attribute
        {
            T attr;

            for (int i = 0; i < attributeList.Count; i++)
            {
                attr = attributeList[i] as T;
                if (attr != null)
                {
                    return attr;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a new instance of the attribute to the list.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns></returns>
        public static T Add<T>(this List<Attribute> attributeList)
            where T : Attribute, new()
        {
            var attr = new T();
            attributeList.Add(attr);
            return attr;
        }

        /// <summary>
        /// Removes all instances of the given type in the list.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="attributeList">The attribute list.</param>
        /// <returns></returns>
        public static bool RemoveAttributeOfType<T>(this List<Attribute> attributeList)
            where T : Attribute
        {
            var count = attributeList.Count;
            bool removed = false;

            for (int i = 0; i < count; i++)
            {
                if (attributeList[i] is T)
                {
                    attributeList.RemoveAt(i);
                    i--;
                    count--;
                    removed = true;
                }
            }

            return removed;
        }
    }
}
#endif