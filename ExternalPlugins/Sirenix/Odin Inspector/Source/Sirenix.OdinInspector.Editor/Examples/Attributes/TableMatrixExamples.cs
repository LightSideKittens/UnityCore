//-----------------------------------------------------------------------
// <copyright file="TableMatrixExamples.cs" company="Sirenix ApS">
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
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [ShowOdinSerializedPropertiesInInspector]
    [AttributeExample(typeof(TableMatrixAttribute), "Right-click columns to use context menus, or drag the column and row labels in order to modify the tables.")]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class TableMatrixExamples
    {
        [TableMatrix(HorizontalTitle = "Square Celled Matrix", SquareCells = true)]
        public Texture2D[,] SquareCelledMatrix;

        [TableMatrix(SquareCells = true)]
        public Mesh[,] PrefabMatrix;

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            SquareCelledMatrix = new Texture2D[8, 4]
            {
                { ExampleHelper.GetTexture(), null, null, null },
                { null, ExampleHelper.GetTexture(), null, null },
                { null, null, ExampleHelper.GetTexture(), null },
                { null, null, null, ExampleHelper.GetTexture() },
                { ExampleHelper.GetTexture(), null, null, null },
                { null, ExampleHelper.GetTexture(), null, null },
                { null, null, ExampleHelper.GetTexture(), null },
                { null, null, null, ExampleHelper.GetTexture() },
            };

            PrefabMatrix = new Mesh[8, 4]
            {
                { ExampleHelper.GetMesh(), null, null, null },
                { null, ExampleHelper.GetMesh(), null, null },
                { null, null, ExampleHelper.GetMesh(), null },
                { null, null, null, ExampleHelper.GetMesh() },
                { null, null, null, ExampleHelper.GetMesh() },
                { null, null, ExampleHelper.GetMesh(), null },
                { null, ExampleHelper.GetMesh(), null, null },
                { ExampleHelper.GetMesh(), null, null, null },
            };
        }
#endif
    }
}
#endif