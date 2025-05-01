//-----------------------------------------------------------------------
// <copyright file="HideLabelAttributeDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="HideLabelAttribute"/>.
    /// </summary>
	/// <seealso cref="HideLabelAttribute"/>
	/// <seealso cref="LabelTextAttribute"/>
	/// <seealso cref="TitleAttribute"/>
	/// <seealso cref="HeaderAttribute"/>
	/// <seealso cref="GUIColorAttribute"/>
    [DrawerPriority(91, 0, 0)]
    public sealed class HideLabelAttributeDrawer : OdinAttributeDrawer<HideLabelAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.CallNextDrawer(null);
        }
    }
}
#endif