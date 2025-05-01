//-----------------------------------------------------------------------
// <copyright file="DisableContextMenuExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(DisableContextMenuAttribute))]
    internal class DisableContextMenuExamples
    {
        [InfoBox("DisableContextMenu disables all right-click context menus provided by Odin. It does not disable Unity's context menu.", InfoMessageType.Warning)]
        [DisableContextMenu]
        public int[] NoRightClickList = new int[] { 2, 3, 5 };

        [DisableContextMenu(disableForMember: false, disableCollectionElements: true)]
        public int[] NoRightClickListOnListElements = new int[] { 7, 11 };

        [DisableContextMenu(disableForMember: true, disableCollectionElements: true)]
        public int[] DisableRightClickCompletely = new int[] { 13, 17 };

        [DisableContextMenu]
        public int NoRightClickField = 19;
    }
}
#endif