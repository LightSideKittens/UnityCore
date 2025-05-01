//-----------------------------------------------------------------------
// <copyright file="TableColumnWidthExample.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [AttributeExample(typeof(TableColumnWidthAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
    internal class TableColumnWidthExample
    {
        [TableList]
        public List<MyItem> List = new List<MyItem>()
        {
            new MyItem(),
            new MyItem(),
            new MyItem(),
        };

        [Serializable]
        public class MyItem
        {
            [PreviewField(Height = 20)]
            [TableColumnWidth(30, Resizable = false)]
            public Texture2D Icon;

            [TableColumnWidth(60)]
            public int ID;

            public string Name;

#if UNITY_EDITOR // Editor-related code must be excluded from builds
            [OnInspectorInit]
            private void CreateData()
            {
                Icon = ExampleHelper.GetTexture();
            }
#endif
        }
    }
}
#endif