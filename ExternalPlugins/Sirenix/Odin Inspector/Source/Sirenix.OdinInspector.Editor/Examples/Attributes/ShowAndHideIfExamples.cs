//-----------------------------------------------------------------------
// <copyright file="ShowAndHideIfExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(ShowIfAttribute))]
    [AttributeExample(typeof(HideIfAttribute))]
    internal class ShowAndHideIfExamples
    {
        // Note that you can also reference methods and properties or use @expressions. You are not limited to fields.
        public UnityEngine.Object SomeObject;

        [EnumToggleButtons]
        public InfoMessageType SomeEnum;

        public bool IsToggled;

        [ShowIf("SomeEnum", InfoMessageType.Info)]
        public Vector3 Info;

        [ShowIf("SomeEnum", InfoMessageType.Warning)]
        public Vector2 Warning;

        [ShowIf("SomeEnum", InfoMessageType.Error)]
        public Vector3 Error;

        [ShowIf("IsToggled")]
        public Vector2 VisibleWhenToggled;

        [HideIf("IsToggled")]
        public Vector3 HiddenWhenToggled;

        [HideIf("SomeObject")]
        public Vector3 ShowWhenNull;

        [ShowIf("SomeObject")]
        public Vector3 HideWhenNull;

        [EnableIf("@this.IsToggled && this.SomeObject != null || this.SomeEnum == InfoMessageType.Error")]
        public int ShowWithExpression;
    }
}
#endif