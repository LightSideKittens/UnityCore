//-----------------------------------------------------------------------
// <copyright file="OnValueChangedAttributeDrawer.cs" company="Sirenix ApS">
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

    using Utilities.Editor;
    using UnityEngine;
    using System;
    using ActionResolvers;
    using UnityEditor;

    /// <summary>
    /// Draws properties marked with <see cref="OnValueChangedAttribute"/>.
    /// </summary>
    /// <seealso cref="OnCollectionChangedAttribute"/>
    /// <seealso cref="OnValueChangedAttribute"/>
    /// <seealso cref="OnInspectorGUIAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    /// <seealso cref="InfoBoxAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class OnValueChangedAttributeDrawer<T> : OdinAttributeDrawer<OnValueChangedAttribute, T>, IDisposable
    {
        private ActionResolver onChangeAction;
        private bool subscribedToOnUndoRedo;

        protected override void Initialize()
        {
            if (Attribute.InvokeOnUndoRedo)
            {
                Property.Tree.OnUndoRedoPerformed += OnUndoRedo;
                subscribedToOnUndoRedo = true;
            }

            onChangeAction = ActionResolver.Get(Property, Attribute.Action);

            // TODO: OnValueChanged notifications whenever prefabs are modified, if this is the modified prefab
            //Undo.postprocessModifications += TODO_THIS;

            Action<int> triggerAction = TriggerAction;
            ValueEntry.OnValueChanged += triggerAction;

            if (Attribute.IncludeChildren || typeof(T).IsValueType)
            {
                ValueEntry.OnChildValueChanged += triggerAction;
            }

            if (Attribute.InvokeOnInitialize && !onChangeAction.HasError)
            {
                onChangeAction.DoActionForAllSelectionIndices();
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (onChangeAction.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(onChangeAction.ErrorMessage);
            }

            CallNextDrawer(label);
        }

        private void OnUndoRedo()
        {
            for (int i = 0; i < ValueEntry.ValueCount; i++)
            {
                TriggerAction(i);
            }
        }

        private void TriggerAction(int selectionIndex)
        {
            Property.Tree.DelayActionUntilRepaint(() =>
            {
                onChangeAction.DoAction(selectionIndex);
            });
        }

        public void Dispose()
        {
            if (subscribedToOnUndoRedo)
            {
                if (Undo.isProcessing)
                { 
                    for (int i = 0; i < ValueEntry.ValueCount; i++)
                    {
                        onChangeAction.DoAction(i);
                    }
                }
                
                Property.Tree.OnUndoRedoPerformed -= OnUndoRedo;
            }
        }
    }
}
#endif