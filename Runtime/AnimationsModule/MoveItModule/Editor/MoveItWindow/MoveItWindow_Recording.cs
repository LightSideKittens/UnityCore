#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.AnimationsModule;
using LSCore.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class MoveItWindow
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
            if (mod.currentValue.target is Component comp)
            {
                TryModifyObjectCurve<CompFloatPropertyHandler, Component>(mod, comp);
            }
            else if (mod.currentValue.target is GameObject go)
            {
                TryModifyObjectCurve<GOFloatPropertyHandler, GameObject>(mod, go);
            }
            
            if (mod.currentValue.target is Transform transform)
            {
                /*TryModifyTransform<TransformPosition>("m_LocalPosition", mod, transform);
                TryModifyTransform<TransformRotation>("m_LocalEulerAnglesHint", mod, transform);
                TryModifyTransform<TransformScale>("m_LocalScale", mod, transform);*/
            }
        }
    }
    
    
    /*private void TryModifyTransform<T>(string propertyName, UndoPropertyModification mod, Transform transform) where T : ITransformHandler, new()
    {
        var cur = mod.currentValue;
        var propertyPath = cur.propertyPath;
        if (!propertyPath.StartsWith(propertyName))
        {
            return;
        }

        var axis = propertyPath.Split('.')[^1];
        var value = float.Parse(cur.value);
            
        if (!objectByHandlerType.TryGetValue(transform, out Dictionary<Type, MoveIt.Handler> handlers))
        {
            objectByHandlerType[transform] = handlers = new Dictionary<Type, MoveIt.Handler>();
        }
        
        var handler = TryAddTransformHandler<T>(handlers, transform);
        OdinMenuItem handlerItem = FindHandlerItem(handler);

        ModifyCurve(handlerItem, axis, value);
    }*/
    
    private void TryModifyObjectCurve<T, T1>(UndoPropertyModification mod, T1 target) where T : FloatPropertyHandler<T1>, new() where T1 : Object
    {
        var cur = mod.currentValue;
        var propertyPath = cur.propertyPath;
        var value = float.Parse(cur.value);

        if (!objectByHandlerType.TryGetValue(target, out Dictionary<Type, MoveIt.Handler> handlers))
        {
            objectByHandlerType[target] = handlers = new Dictionary<Type, MoveIt.Handler>();
        }

        var handler = TryAddObjectHandler<T, T1>(handlers, target);
        HandlerItem handlerItem = FindHandlerItem(handler);

        ModifyCurve(handlerItem, propertyPath, value);
    }

    private Dictionary<Object, Dictionary<Type, MoveIt.Handler>> objectByHandlerType = new();

    private HandlerItem FindHandlerItem(MoveIt.Handler handler)
    {
        HandlerItem handlerItem = MenuTree.RootMenuItem.GetChildMenuItemsRecursive(true)
            .OfType<HandlerItem>().FirstOrDefault(x => x.handler == handler);
        return handlerItem;
    }
    
    /*private MoveIt.Handler TryAddTransformHandler<T>(Dictionary<Type, MoveIt.Handler> handlers, Transform transform) where T : ITransformHandler, new()
    {
        if (!handlers.TryGetValue(typeof(T), out var handler))
        {
            var h = new T();
            handlers[typeof(T)] = handler = h.Handler;
            h.Transform = transform;
            AddHandler(handler);
        }

        return handler;
    }*/
    
    private MoveIt.Handler TryAddObjectHandler<T, T1>(Dictionary<Type, MoveIt.Handler> handlers, T1 target) where T : FloatPropertyHandler<T1>, new() where T1 : Object
    {
        if (!handlers.TryGetValue(typeof(T), out var handler))
        {
            var h = new T();
            handlers[typeof(T)] = handler = h;
            h.SetTarget(target);
            AddHandler(handler);
        }
        
        return handler;
    }
    
    private void ModifyCurve(HandlerItem handlerItem, string property, float value)
    {
        var curveItem = handlerItem.ChildMenuItems.OfType<CurveItem>()
            .FirstOrDefault(x => x.property == property);

        if (curveItem == null)
        {
            curveItem = new CurveItem(MenuTree, property, red, CurrentClip, handlerItem.handler, curvesEditor);
            MenuTree.AddMenuItemAtPath(handlerItem.GetFullPath(), curveItem);
        }
        
        if (!curveItem.TryGetCurve(out _))
        {
            CreateCurve(curveItem);
        }

        curveItem.TryCreateCurveEditor(this);
        GUIScene.StartMatrix(curvesEditor.curvesEditor.matrix);
        curveItem.editor.SetKeyY(timePointer.Time, value);
        GUIScene.EndMatrix();
    }
}
#endif