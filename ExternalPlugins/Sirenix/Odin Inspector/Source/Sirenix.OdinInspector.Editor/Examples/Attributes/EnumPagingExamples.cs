//-----------------------------------------------------------------------
// <copyright file="EnumPagingExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(EnumPagingAttribute))]
    internal class EnumPagingExamples
    {
        [EnumPaging]
        public SomeEnum SomeEnumField;
        
        public enum SomeEnum
        {
            A, B, C
        }
    }
}
#endif