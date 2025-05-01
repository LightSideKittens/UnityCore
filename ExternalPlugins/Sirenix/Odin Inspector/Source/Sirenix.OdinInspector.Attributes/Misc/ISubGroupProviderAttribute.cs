//-----------------------------------------------------------------------
// <copyright file="ISubGroupProviderAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Internal
{
#pragma warning disable

    using System.Collections.Generic;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public interface ISubGroupProviderAttribute
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <returns>Not yet documented.</returns>
        IList<PropertyGroupAttribute> GetSubGroupAttributes();

        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <param name="attr">Not yet documented.</param>
        /// <returns>Not yet documented.</returns>
        string RepathMemberAttribute(PropertyGroupAttribute attr);
    }
}