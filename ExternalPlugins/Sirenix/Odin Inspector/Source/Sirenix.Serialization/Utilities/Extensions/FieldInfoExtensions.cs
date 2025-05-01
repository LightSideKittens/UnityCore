//-----------------------------------------------------------------------
// <copyright file="FieldInfoExtensions.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization.Utilities
{
#pragma warning disable

    using System;
    using System.Reflection;

    /// <summary>
    /// FieldInfo method extensions.
    /// </summary>
    internal static class FieldInfoExtensions
    {
        /// <summary>
        /// Determines whether the specified field is an alias.
        /// </summary>
        /// <param name="fieldInfo">The field to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified field is an alias; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAliasField(this FieldInfo fieldInfo)
        {
            return fieldInfo is MemberAliasFieldInfo;
        }

        /// <summary>
        /// Returns the original, backing field of an alias field if the field is an alias.
        /// </summary>
        /// <param name="fieldInfo">The field to check.</param>
        /// /// <param name="throwOnNotAliased">if set to <c>true</c> an exception will be thrown if the field is not aliased.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">The field was not aliased; this only occurs if throwOnNotAliased is true.</exception>
        public static FieldInfo DeAliasField(this FieldInfo fieldInfo, bool throwOnNotAliased = false)
        {
            MemberAliasFieldInfo aliasFieldInfo = fieldInfo as MemberAliasFieldInfo;

            if (aliasFieldInfo != null)
            {
                while (aliasFieldInfo.AliasedField is MemberAliasFieldInfo)
                {
                    aliasFieldInfo = aliasFieldInfo.AliasedField as MemberAliasFieldInfo;
                }

                return aliasFieldInfo.AliasedField;
            }

            if (throwOnNotAliased)
            {
                throw new ArgumentException("The field " + fieldInfo.GetNiceName() + " was not aliased.");
            }

            return fieldInfo;
        }
    }
}