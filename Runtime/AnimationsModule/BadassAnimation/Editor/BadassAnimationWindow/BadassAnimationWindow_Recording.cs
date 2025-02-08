#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LSCore.AnimationsModule;
using LSCore.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class BadassAnimationWindow
{
    private bool isPreview;
    private bool IsPreview
    {
        get => isPreview;
        set
        {
            if (!value && IsRecording)
            {
                IsRecording = false;
            }

            IsAnimationMode = value;
            
            isPreview = value;
            UpdateAnimationComponent();
            if (value)
            {
                EvaluateAnimation();
            }
        }
    }

    private void TryUpdateAnimationMode()
    {
        if (window.IsAnimationMode)
        {
            window.IsAnimationMode = false;
            window.IsAnimationMode = true;
        }
    }

    private bool IsAnimationMode
    {
        get => AnimationMode.InAnimationMode();
        set
        {
            if (value)
            {
                AnimationMode.StartAnimationMode();
                animation.StartAnimationMode();
            }
            else
            {
                AnimationMode.StopAnimationMode();
            }
        }
    }
    
    
    private bool isRecording;
    private bool IsRecording
    {
        get => isRecording;
        set
        {
            if (value && !IsPreview)
            {
                IsPreview = true;
            }
            
            if (isRecording != value)
            {
                isRecording = value;
                if (value)
                {
                    Undo.postprocessModifications += OnPostProcessModifications;
                }
                else
                {
                    Undo.postprocessModifications -= OnPostProcessModifications;
                }
            }
        }
    }
    
    private UndoPropertyModification[] OnPostProcessModifications(UndoPropertyModification[] modifications)
    {
        var modsList = new List<UndoPropertyModification>(modifications);
        ModifyCurves(modifications);
        animation.TrimModifications(modsList);
        modifications = modsList.ToArray();
        return modifications;
    }
    
    private void ModifyCurves(UndoPropertyModification[] modifications)
    {
        for (int i = 0; i < modifications.Length; i++)
        {
            var mod = modifications[i];
            if (mod.currentValue.target is Transform transform)
            {
                TryModifyTransform<TransformPosition>("m_LocalPosition", mod, transform);
                TryModifyTransform<TransformRotation>("m_LocalEulerAnglesHint", mod, transform);
                TryModifyTransform<TransformScale>("m_LocalScale", mod, transform);
            }
        }
    }
    
    
    private void TryModifyTransform<T>(string propertyName, UndoPropertyModification mod, Transform transform) where T : BaseTransformHandler, new()
    {
        var cur = mod.currentValue;
        var propertyPath = cur.propertyPath;
        if (!propertyPath.StartsWith(propertyName))
        {
            return;
        }

        var axis = propertyPath.Split('.')[^1];
        var value = float.Parse(cur.value);
            
        if (!objectByHandlerType.TryGetValue(transform, out Dictionary<Type, BadassAnimation.Handler> handlers))
        {
            objectByHandlerType[transform] = handlers = new Dictionary<Type, BadassAnimation.Handler>();
        }
        
        var handler = TryAddTransformHandler<T>(handlers, transform);
        OdinMenuItem handlerItem = FindHandlerItem(handler);

        ModifyCurve(handlerItem, axis, value);
    }

    private Dictionary<Object, Dictionary<Type, BadassAnimation.Handler>> objectByHandlerType = new();

    private OdinMenuItem FindHandlerItem(BadassAnimation.Handler handler)
    {
        OdinMenuItem handlerItem = MenuTree.RootMenuItem.GetChildMenuItemsRecursive(true)
            .OfType<HandlerItem>().FirstOrDefault(x => x.handler == handler);
        return handlerItem;
    }
    
    private BadassAnimation.Handler TryAddTransformHandler<T>(Dictionary<Type, BadassAnimation.Handler> handlers, Transform transform) where T : BaseTransformHandler, new()
    {
        if (!handlers.TryGetValue(typeof(T), out var handler))
        {
            handlers[typeof(T)] = handler = new T();
            var transformHandler = (T)handler;
            transformHandler.transform = transform;
            AddHandler(handler);
        }

        return handler;
    }
    
    private void ModifyCurve(OdinMenuItem handlerItem, string curveName, float value)
    {
        var curveItem = handlerItem.ChildMenuItems.OfType<CurveItem>()
            .FirstOrDefault(x => x.Name == curveName);
        if (curveItem != null)
        {
            if (!curveItem.TryGetCurve(out _))
            {
                CreateCurve(curveItem);
            }

            curveItem.TryCreateCurveEditor(this);
            GUIScene.StartMatrix(editor.curvesEditor.matrix);
            curveItem.editor.SetKeyY(timePointer.Time, value);
            GUIScene.EndMatrix();
        }
    }
}
#endif