//-----------------------------------------------------------------------
// <copyright file="EmittedGradientContainer.cs" company="Sirenix ApS">
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

    public class EmittedGradientContainer : EmittedScriptableObject<Gradient>
    {
        public Gradient value;

        public override FieldInfo BackingFieldInfo
        {
            get
            {
                return typeof(EmittedGradientContainer).GetField("value");
            }
        }

        public override Gradient GetValue()
        {
            return this.value;
        }

        public override void SetValue(Gradient value)
        {
            this.value = value;
        }
    }
}
#endif