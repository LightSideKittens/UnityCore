using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.Utilities.Editor;
using System;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
public sealed class LSOnValueChangedAttributeDrawer<T> :
    OdinAttributeDrawer<LSOnValueChangedAttribute, T>,
    IDisposable
{
    private ActionResolver onChangeAction;
    private bool subscribedToOnUndoRedo;

    private object Get(ref ActionResolverContext xxx, int d)
    {
        return null;
    }
    
    protected override void Initialize()
    {
        if (Attribute.InvokeOnUndoRedo)
        {
            Property.Tree.OnUndoRedoPerformed += OnUndoRedo;
            subscribedToOnUndoRedo = true;
        }

        if (Attribute.Parameters != null)
        {
            foreach (var parameter in Attribute.Parameters)
            {
                Type type = Property.Children[parameter].Info.TypeOfValue;
                onChangeAction = ActionResolver.Get(Property, Attribute.Action, new NamedValue(parameter, type,
                    (ref ActionResolverContext _, int _) => Property.Children[parameter].ValueEntry.WeakSmartValue));
            }
        }
        else
        {
            onChangeAction = ActionResolver.Get(Property, Attribute.Action);
        }
        
        Action<int> action = TriggerAction;
        ValueEntry.OnValueChanged += action;
        if ((Attribute.IncludeChildren || typeof(T).IsValueType))
        {
            if (Attribute.Parameters == null)
            {
                ValueEntry.OnChildValueChanged += action;
            }
            else
            {
                foreach (var parameter in Attribute.Parameters)
                {
                    Property.Children[parameter].ValueEntry.OnValueChanged += action;
                }
            }
        }

        if (!Attribute.InvokeOnInitialize || onChangeAction.HasError)
        {
            return;
        }

        onChangeAction.DoActionForAllSelectionIndices();
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
        for (int selectionIndex = 0; selectionIndex < ValueEntry.ValueCount; ++selectionIndex)
            TriggerAction(selectionIndex);
    }

    private void TriggerAction(int selectionIndex)
    {
        Property.Tree.DelayActionUntilRepaint(() => onChangeAction.DoAction(selectionIndex));
    }

    public void Dispose()
    {
        if (!subscribedToOnUndoRedo)
        {
            return;
        }

        Property.Tree.OnUndoRedoPerformed -= OnUndoRedo;
    }
}

