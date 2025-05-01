//-----------------------------------------------------------------------
// <copyright file="ChildGameObjectsOnlyAttributeExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(ChildGameObjectsOnlyAttribute), "The ChildGameObjectsOnly attribute can be used on Components and GameObject fields and will prepend a small button next to the object-field that will search through all child gameobjects for assignable objects and present them in a dropdown for the user to choose from.")]
    [TypeInfoBox("Note that this drawn example does not represent a GameObject, so the attribute is not shown correctly in the Attribute Overview window. If you copy it to a script, for example via the Save Component Script button, the example will display correctly. You may also click the Documentation button to view our online documentation for visual examples of how it works.")]
    internal class ChildGameObjectsOnlyAttributeExamples
    {
        [ChildGameObjectsOnly]
        public Transform ChildOrSelfTransform;

        [ChildGameObjectsOnly]
        public GameObject ChildGameObject;

        [ChildGameObjectsOnly(IncludeSelf = false)]
        public Light[] Lights;
    }
}
#endif