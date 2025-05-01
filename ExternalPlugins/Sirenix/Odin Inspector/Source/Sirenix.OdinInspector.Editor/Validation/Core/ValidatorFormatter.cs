//-----------------------------------------------------------------------
// <copyright file="ValidatorFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.Serialization.RegisterFormatter(typeof(Sirenix.OdinValidator.Editor.ValidatorFormatter<>))]

namespace Sirenix.OdinValidator.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.Serialization;

    public class ValidatorFormatter<T> : ReflectionOrEmittedBaseFormatter<T> where T : Validator, new()
    {
        protected override T GetUninitializedObject()
        {
            return new T();
        }
    }
}
#endif