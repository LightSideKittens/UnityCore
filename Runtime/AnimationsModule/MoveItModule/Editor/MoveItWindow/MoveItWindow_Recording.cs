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
        
        float.TryParse(prev.value, out var prevValue);
        if (!float.TryParse(cur.value, out var value))
        {
            if (cur.objectReference != null)
            {
                var valueInt = cur.objectReference.GetInstanceID();
                value = valueInt;
            }
            
            if (prev.objectReference != null)
            {
                var valueInt = prev.objectReference.GetInstanceID();
                prevValue = valueInt;
            }
        }
        
        ModifyCurve(handlerItem, propertyPath, value, prevValue);
    }

    private Dictionary<Object, Dictionary<Type, MoveIt.Handler>> objectByHandlerType = new();

    private HandlerItem FindHandlerItem(MoveIt.Handler handler)
    {
        HandlerItem handlerItem = MenuTree.RootMenuItem.GetChildMenuItemsRecursive(true)
            .OfType<HandlerItem>().FirstOrDefault(x => x.handler == handler);
        return handlerItem;
    }
    
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
    
    private CurveItem ModifyCurve(HandlerItem handlerItem, string property, float value, float prevValue = float.NaN)
    {
        var handler = handlerItem.handler;
        var curveItem = handlerItem.GetChildMenuItemsRecursive(false).OfType<CurveItem>()
            .FirstOrDefault(x => x.property == property);
        
        if (curveItem == null)
        {
            curveItem = new CurveItem(MenuTree, property, red, CurrentClip, handler, curvesEditor);
            
            var split = curveItem.property.Split('.');
            OdinMenuItem item = handlerItem;
            
            for (var i = 0; i < split.Length - 1; i++)
            {
                var part = split[i];
                var newItem = new MenuItem(MenuTree, part, null);
                MenuTree.AddMenuItemAtPath(item.GetFullPath(), newItem);
                item = newItem;
            }
            
            MenuTree.AddMenuItemAtPath(item.GetFullPath(), curveItem);
            curveItem.SetColor(colors[curveItem.Parent.ChildMenuItems.OfType<CurveItem>().Count() - 1]);
        }

        return ModifyCurve(curveItem, handler, value, prevValue);
    }

    private CurveItem ModifyCurve(CurveItem curveItem, MoveIt.Handler handler, float value, float prevValue = float.NaN)
    {
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
        
        GUIScene.StartMatrix(curvesEditor.curvesEditor.matrix);
        
        int lastId = (int)curveItem.editor.GetKey(timePointer.Time, out var index).y;
        UniDict<int, Object> objects = handler.Objects ??= new UniDict<int, Object>();
        if(index != -1 && curveItem.ContainsInKeys(lastId) == 1) objects.Remove(lastId);
        var id = (int)value;
        var obj = EditorUtility.InstanceIDToObject(id);
        if (obj != null) objects[id] = obj;
        objects.Editor_ApplyToData();
        
        curveItem.editor.SetKeyY(timePointer.Time, value);
        GUIScene.EndMatrix();
        
        return curveItem;
    }
}
#endif