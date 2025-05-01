//-----------------------------------------------------------------------
// <copyright file="NullableDrawer.cs" company="Sirenix ApS">
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

    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Property drawer for nullables.
    /// </summary>
    public sealed class NullableDrawer<T> : OdinValueDrawer<T?>, IDisposable, IDefinesGenericMenuItems where T : struct
    {
        private PropertyTree<Wrapper> tree;

        protected override void Initialize()
        {
            Wrapper[] wrappers = new Wrapper[this.ValueEntry.ValueCount];

            for (int i = 0; i < wrappers.Length; i++)
            {
                var wrapper = new Wrapper();
                wrappers[i] = wrapper;
            }

            this.tree = new PropertyTree<Wrapper>(wrappers);
            tree.UpdateTree();
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            for (int i = 0; i < this.tree.Targets.Count; i++)
            {
                this.tree.Targets[i].SetValue(entry.Values[i]);
            }

            this.tree.GetRootProperty(0).Label = label;
            this.tree.Draw(false);

            for (int i = 0; i < this.tree.Targets.Count; i++)
            {
                var value = this.tree.Targets[i];

                if (value.Value == null)
                {
                    entry.Values[i] = null;
                }
                else
                {
                    entry.Values[i] = value.Value.Value;
                }
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var content = new GUIContent("Set to null");
            IPropertyValueEntry<T?> entry = (IPropertyValueEntry<T?>)property.ValueEntry;

            if (entry.IsEditable && entry.SmartValue.HasValue)
            {
                genericMenu.AddItem(content, false, () =>
                {
                    property.Tree.DelayActionUntilRepaint(() =>
                    {
                        entry.SmartValue = null;
                    });
                });
            }
            else
            {
                genericMenu.AddDisabledItem(content);
            }
        }

        public void Dispose()
        {
            this.tree?.Dispose();
        }

        [ShowOdinSerializedPropertiesInInspector]
        private class Wrapper
        {
            public NullableValue<T> Value;

            public void SetValue(T? value)
            {
                if (value.HasValue)
                {
                    this.Value = new NullableValue<T>();
                    this.Value.Value = value.Value;
                }
            }
        }
    }

    internal class NullableValue<T>
    {
        [HideLabel]
        public T Value;
    }
}
#endif