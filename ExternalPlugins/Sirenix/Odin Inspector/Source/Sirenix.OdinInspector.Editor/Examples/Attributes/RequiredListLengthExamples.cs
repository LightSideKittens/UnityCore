//-----------------------------------------------------------------------
// <copyright file="RequiredListLengthExamples.cs" company="Sirenix ApS">
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

    using System.Collections.Generic;
    using UnityEngine;

    [AttributeExample(typeof(RequiredListLengthAttribute), "An attribute that can be applied to lists, arrays and other types of collections to ensure they contain a specified number of elements.")]
    internal class RequiredListLengthExamples
    {
        [RequiredListLength(10)]
        public int[] fixedLength;

        [RequiredListLength(1, null)]
        public int[] minLength;

        [RequiredListLength(null, 10, PrefabKind = PrefabKind.InstanceInScene)]
        public List<int> maxLength;

        [RequiredListLength(3, 10)]
        public List<int> minAndMaxLength;

        public int SomeNumber;

        [RequiredListLength("@this.SomeNumber")] 
        public List<GameObject> matchLengthOfOther;

        [RequiredListLength("@this.SomeNumber", null)]
        public int[] minLengthExpression;

        [RequiredListLength(null, "@this.SomeNumber")]
        public List<int> maxLengthExpression;
    }
}
#endif