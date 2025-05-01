//-----------------------------------------------------------------------
// <copyright file="RequiredExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(RequiredAttribute), "Required displays an error when objects are missing.")]
    internal class RequiredExamples
    {
        [Required]
        public GameObject MyGameObject;

        [Required("Custom error message.")]
        public Rigidbody MyRigidbody;

        [InfoBox("Use $ to indicate a member string as message.")]
        [Required("$DynamicMessage")]
        public GameObject GameObject;

        public string DynamicMessage = "Dynamic error message";
    }
}
#endif