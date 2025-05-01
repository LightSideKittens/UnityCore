//-----------------------------------------------------------------------
// <copyright file="ShowDrawerChainAttributeDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Show drawer chain attribute drawer.
    /// </summary>
    [DrawerPriority(10000, 0, 0)]
    public class ShowDrawerChainAttributeDrawer : OdinAttributeDrawer<ShowDrawerChainAttribute>
    {
        private int drawnDepth;

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;

            var chain = property.GetActiveDrawerChain();
            var drawers = chain.BakedDrawerArray;

            SirenixEditorGUI.BeginToolbarBox("Drawers for property '" + this.Property.Path + "'");

            for (int i = 0; i < drawers.Length; i++)
            {
                bool highlight = drawers[i].GetType().Assembly != typeof(ShowDrawerChainAttributeDrawer).Assembly;

                if (highlight)
                {
                    GUIHelper.PushColor(Color.green);
                }

                if (i > this.drawnDepth)
                {
                    GUIHelper.PushColor(new Color(1, 1, 1, 0.5f));
                }

                if (drawers[i] != this)
                {
                    var tooltip =
                        @"You can toggle drawers on and off for debugging purposes. The state will not be saved anywhere, and is only for the current property.";
                    var labelText = i + ": " + drawers[i].GetType().GetNiceName() + (drawers[i].SkipWhenDrawing ? " (skipped)" : "");
                    var labelContent = GUIHelper.TempContent(labelText, tooltip);
                    drawers[i].SkipWhenDrawing = !EditorGUILayout.ToggleLeft(labelContent, !drawers[i].SkipWhenDrawing);
                }
                else
                {
                    EditorGUILayout.LabelField("     " + i + ": " + drawers[i].GetType().GetNiceName() + (drawers[i].SkipWhenDrawing ? " (skipped)" : ""));
                }

                var rect = GUILayoutUtility.GetLastRect();

                if (i > this.drawnDepth)
                {
                    GUIHelper.PopColor();
                }

                GUI.Label(rect, DrawerUtilities.GetDrawerPriority(drawers[i].GetType()).ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);

                if (highlight)
                {
                    GUIHelper.PopColor();
                }
            }
            SirenixEditorGUI.EndToolbarBox();

            this.CallNextDrawer(label);

            this.drawnDepth = chain.CurrentIndex;
        }
    }
}
#endif