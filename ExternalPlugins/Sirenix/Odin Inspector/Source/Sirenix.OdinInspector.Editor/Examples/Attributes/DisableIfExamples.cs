//-----------------------------------------------------------------------
// <copyright file="DisableIfExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(DisableIfAttribute))]
    internal class DisableIfExamples
    {
        public UnityEngine.Object SomeObject;

        [EnumToggleButtons]
        public InfoMessageType SomeEnum;

        public bool IsToggled;

        [DisableIf("SomeEnum", InfoMessageType.Info)]
        public Vector2 Info;

        [DisableIf("SomeEnum", InfoMessageType.Error)]
        public Vector2 Error;

        [DisableIf("SomeEnum", InfoMessageType.Warning)]
        public Vector2 Warning;

        [DisableIf("IsToggled")]
        public int DisableIfToggled;

        [DisableIf("SomeObject")]
        public Vector3 EnabledWhenNull;

        [DisableIf("@this.IsToggled && this.SomeObject != null || this.SomeEnum == InfoMessageType.Error")]
        public int DisableWithExpression;
    }
}
#endif