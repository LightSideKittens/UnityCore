#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.AnimationsModule;
using LSCore.DataStructs;
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

            isPreview = value;
            IsAnimationMode = value;
            UpdateAnimationComponent();

            if (value)
            {
                EvaluateAnimation();
            }
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

    private Dictionary<MoveIt.Handler, List<(float prevValue, string property)>> needUpdateAnimationComponent = new();
    
    private void ModifyCurves(UndoPropertyModification[] modifications)
    {
        needUpdateAnimationComponent.Clear();
        
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

        if (needUpdateAnimationComponent.Count > 0)
        {
            UpdateAnimationComponent();

            foreach (var (handler, data) in needUpdateAnimationComponent)
            {
                foreach (var (prevValue, property) in data)
                {
                    if (handler.TryGetEvaluator(property, out var evaluator))
                    {
                        evaluator.startY = prevValue;
                    }
                }
            }
            
            needUpdateAnimationComponent.Clear();
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
        if (!objectByHandlerType.TryGetValue(target, out Dictionary<Type, MoveIt.Handler> handlers))
        {
            objectByHandlerType[target] = handlers = new Dictionary<Type, MoveIt.Handler>();
        }

        var handler = TryAddObjectHandler<T, T1>(handlers, target);
        HandlerItem handlerItem = FindHandlerItem(handler);
        
        var cur = mod.currentValue;
        var prev = mod.previousValue;
        var propertyPath = cur.propertyPath;
        CurveItem curveItem = null;
        Action action = null;
        int lastId = 0;
        UniDict<int, Object> objects = null;
        
        float.TryParse(prev.value, out var prevValue);
        if (!float.TryParse(cur.value, out var value))
        {
            objects = handler.Objects ??= new UniDict<int, Object>();
            
            if (cur.objectReference != null)
            {
                var valueInt = cur.objectReference.GetInstanceID();
                value = valueInt;

                action = () =>
                {
                    objects[valueInt] = cur.objectReference;
                };
            }
            
            if (prev.objectReference != null)
            {
                var valueInt = prev.objectReference.GetInstanceID();
                prevValue = valueInt;
            }
        }
        
        curveItem = GetCurve(handlerItem, propertyPath, prevValue);
        
        lastId = (int)curveItem.editor.GetKey(timePointer.Time, out var index).y;
        objects = handler.Objects ??= new UniDict<int, Object>();
        if(index != -1 && curveItem.ContainsInKeys(lastId) == 1) objects.Remove(lastId);
        
        ModifyCurve(curveItem, value);
        
        action?.Invoke();
        objects.Editor_ApplyToData();
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
    
    private CurveItem GetCurve(HandlerItem handlerItem, string property, float prevValue = float.NaN)
    {
        var handler = handlerItem.handler;
        var curveItem = handlerItem.ChildMenuItems.OfType<CurveItem>()
            .FirstOrDefault(x => x.property == property);

        if (curveItem == null)
        {
            curveItem = new CurveItem(MenuTree, property, red, CurrentClip, handler, curvesEditor);
            MenuTree.AddMenuItemAtPath(handlerItem.GetFullPath(), curveItem);
        }
        
        if (!curveItem.TryGetCurve(out _))
        {
            curveItem.CreateCurve(out var evaluator);
            if (!needUpdateAnimationComponent.TryGetValue(handler, out var data))
            {
                data = new List<(float prevValue, string property)>();
                needUpdateAnimationComponent.Add(handler, data);
            }
            data.Add((prevValue, evaluator.property));
        }

        curveItem.TryCreateCurveEditor(this);
        
        return curveItem;
    }
    
    private CurveItem ModifyCurve(HandlerItem handlerItem, string property, float value, float prevValue = float.NaN)
    {
        var curveItem = GetCurve(handlerItem, property, prevValue);
        ModifyCurve(curveItem, value);
        return curveItem;
    }

    private void ModifyCurve(CurveItem curveItem, float value)
    {
        GUIScene.StartMatrix(curvesEditor.curvesEditor.matrix);
        curveItem.editor.SetKeyY(timePointer.Time, value);
        GUIScene.EndMatrix();
    }
}
#endif