//-----------------------------------------------------------------------
// <copyright file="HideIfGroupExample.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;

    [AttributeExample(typeof(HideIfGroupAttribute))]
    internal class HideIfGroupExample
    {
        public bool Toggle = true;

        [HideIfGroup("Toggle")]
        [BoxGroup("Toggle/Shown Box")]
        public int A, B;

        [BoxGroup("Box")]
        public InfoMessageType EnumField = InfoMessageType.Info;

        [BoxGroup("Box")]
        [HideIfGroup("Box/Toggle")]
        public Vector3 X, Y;

        // Like the regular If-attributes, HideIfGroup also supports specifying values.
        // You can also chain multiple HideIfGroup attributes together for more complex behaviour.
        [HideIfGroup("Box/Toggle/EnumField", Value = InfoMessageType.Info)]
        [BoxGroup("Box/Toggle/EnumField/Border", ShowLabel = false)]
        public string Name;

        [BoxGroup("Box/Toggle/EnumField/Border")]
        public Vector3 Vector;

        // HideIfGroup will by default use the name of the group,
        // but you can also use the MemberName property to override this.
        [HideIfGroup("RectGroup", Condition = "Toggle")]
        public Rect Rect;
    }
}
#endif