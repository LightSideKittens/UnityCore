//-----------------------------------------------------------------------
// <copyright file="AttributesExampleWindow.cs" company="Sirenix ApS">
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

    using System;
    using System.Linq;
    using Sirenix.OdinInspector.Editor.Examples;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    public class AttributesExampleWindow : OdinMenuEditorWindow
    {
        public override Vector4 WindowPadding { get => default; set { } }
        private OdinAttributeExampleItem example;

        public static void OpenWindow()
        {
            OpenWindow(null);
        }

        protected override void DrawEditor(int index)
        {
            this.example?.Draw();
        }

        public static void OpenWindow(Type attributeType)
        {
            bool isNew = Resources.FindObjectsOfTypeAll<AttributesExampleWindow>().Length == 0;

            var w = GetWindow<AttributesExampleWindow>();

            if (isNew)
            {
                w.MenuWidth = 250;
                w.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(850f, 700f);
            }

            if (attributeType != null)
            {
                w.ForceMenuTreeRebuild();

#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                var item = w.MenuTree.EnumerateTree().FirstOrDefault(x => x.Value == attributeType);
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                if (item != null)
                {
                    w.MenuTree.Selection.Clear();
                    w.MenuTree.Selection.Add(item);
                }
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;
            tree.Selection.SelectionChanged += this.SelectionChanged;
            tree.Config.DrawSearchToolbar = true;
            tree.Config.DefaultMenuStyle.Height = 22;

            AttributeExampleUtilities.BuildMenuTree(tree);

            return tree;
        }

        private void SelectionChanged(SelectionChangedType obj)
        {
            if (this.example != null)
            {
                this.example.OnDeselected();
                this.example = null;
            }

            var attr = this.MenuTree.Selection.Select(i => i.Value).FilterCast<Type>().FirstOrDefault();
            if (attr != null)
            {
                this.example = AttributeExampleUtilities.GetExample(attr);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (this.example != null)
            {
                this.example.OnDeselected();
                this.example = null;
            }
        }

        protected override void OnDestroy()
        {
            if (this.example != null)
            {
                this.example.OnDeselected();
                this.example = null;
            }
        }
    }
}
#endif