//-----------------------------------------------------------------------
// <copyright file="TableMatrixTitleExample.cs" company="Sirenix ApS">
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

    [ShowOdinSerializedPropertiesInInspector]
    [AttributeExample(typeof(TableMatrixAttribute), "You can specify custom labels for both the the rows and columns of the table.")]
    internal class TableMatrixTitleExample
    {
        [TableMatrix(HorizontalTitle = "Read Only Matrix", IsReadOnly = true)]
        public int[,] ReadOnlyMatrix = new int[5, 5];

        [TableMatrix(HorizontalTitle = "X axis", VerticalTitle = "Y axis")]
        public InfoMessageType[,] LabledMatrix = new InfoMessageType[6, 6];
    }
}
#endif