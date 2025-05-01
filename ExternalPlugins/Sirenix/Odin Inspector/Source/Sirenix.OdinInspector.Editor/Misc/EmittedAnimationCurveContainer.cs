//-----------------------------------------------------------------------
// <copyright file="EmittedAnimationCurveContainer.cs" company="Sirenix ApS">
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

    using System.Reflection;
    using UnityEngine;

    public class EmittedAnimationCurveContainer : EmittedScriptableObject<AnimationCurve>
    {
        public AnimationCurve value;

        public override FieldInfo BackingFieldInfo
        {
            get
            {
                return typeof(EmittedAnimationCurveContainer).GetField("value");
            }
        }

        public override AnimationCurve GetValue()
        {
            return this.value;
        }

        public override void SetValue(AnimationCurve value)
        {
            this.value = value;
        }
    }
}
#endif