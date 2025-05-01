//-----------------------------------------------------------------------
// <copyright file="EditorIconsOverview.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using UnityEngine;
    using UnityEditor;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using System.Linq;

    /// <summary>
    /// Opens a window which displays a list of all icons available from <see cref="EditorIcons"/>.
    /// </summary>
    public class EditorIconsOverview : OdinSelector<object>
    {
        /// <summary>
        /// Opens a window which displays a list of all icons available from <see cref="EditorIcons"/>.
        /// </summary>
        public static void OpenEditorIconsOverview()
        {
            var window = OdinEditorWindow.InspectObject(new EditorIconsOverview());
            window.ShowUtility();
            window.WindowPadding = new Vector4();
        }

        /// <summary>
        /// Builds the selection tree.
        /// </summary>
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            this.DrawConfirmSelectionButton = false;
            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle.Height = 25;
            tree.Config.SelectMenuItemsOnMouseDown = true;

            foreach (var item in typeof(EditorIcons).GetProperties(Flags.StaticPublic).OrderBy(x => x.Name))
            {
                var returnType = item.GetReturnType();

                if (typeof(Texture).IsAssignableFrom(returnType))
                {
                    tree.Add(item.Name, item.Name, (Texture)item.GetGetMethod().Invoke(null, null));
                }
                else if (typeof(EditorIcon).IsAssignableFrom(returnType))
                {
                    tree.Add(item.Name, item.Name, (EditorIcon)item.GetGetMethod().Invoke(null, null));
                }
            }
        }

        [ShowInInspector, PropertyOrder(30)]
        [PropertyRange(10, 34), LabelWidth(50)]
        [InfoBox("This is an overview of all available icons in the Sirenix.Utilities.Editor.EditorIcons utility class.")]
        private float Size
        {
            get { return this.SelectionTree.DefaultMenuStyle.IconSize; }
            set
            {
                this.SelectionTree.DefaultMenuStyle.IconSize = value;
                this.SelectionTree.DefaultMenuStyle.Height = (int)value + 9;
            }
        }
    }
}
#endif