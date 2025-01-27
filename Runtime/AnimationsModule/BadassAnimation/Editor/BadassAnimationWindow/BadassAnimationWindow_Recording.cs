using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.AnimationsModule;
using LSCore.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class BadassAnimationWindow
{
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
                    Patchers._Transform.TransformTool.Calling += OnCallingTransformTool;
                    Patchers._Transform.TransformTool.Called += OnCalledTransformTool;
                    Patchers._EditorGUI.BeginPropertyInternal.Calling += OnBeginProperty;
                    Patchers._EditorGUI.EndProperty.Called += OnEndProperty;
                }
                else
                {
                    Patchers._Transform.TransformTool.Calling -= OnCallingTransformTool;
                    Patchers._Transform.TransformTool.Called -= OnCalledTransformTool;
                    Patchers._EditorGUI.BeginPropertyInternal.Calling -= OnBeginProperty;
                    Patchers._EditorGUI.EndProperty.Called -= OnEndProperty;
                }
            }
        }
    }

    private Queue<(Vector3 lastPosition, Vector3 lastEulerAngles, Vector3 lastScale)> transformsData = new();
    
    private void OnCallingTransformTool(Transform transform)
    {
        transformsData.Enqueue((transform.localPosition, transform.localEulerAngles, transform.localScale));
    }
    
    private void OnCalledTransformTool(Transform transform, bool isChanged)
    {
        var (lastPosition, lastEulerAngles, lastScale) = transformsData.Dequeue();
        if (!isChanged) return;
        var data = GetChangedAxis(lastPosition, transform.localPosition);
        
        if (data.Count > 0)
        {
            Modify<TransformPosition>();
            return;
        }
        
        data = GetChangedAxis(lastEulerAngles, transform.localEulerAngles);
        
        if (data.Count > 0)
        {
            Modify<TransformRotation>();
            return;
        }
        
        data = GetChangedAxis(lastScale, transform.localScale);
        
        if (data.Count > 0)
        {
            Modify<TransformScale>();
            return;
        }

        void Modify<T>() where T : BaseTransformHandler, new()
        {
            if (!objectByHandlerType.TryGetValue(transform, out Dictionary<Type, BadassAnimation.Handler> handlers))
            {
                objectByHandlerType[transform] = handlers = new Dictionary<Type, BadassAnimation.Handler>();
            }
        
            var handler = TryAddTransformHandler<T>(handlers, transform);
            OdinMenuItem handlerItem = FindHandlerItem(handler);

            foreach (var (axis, value) in data)
            {
                ModifyCurve(handlerItem, axis, value);
            }
        }

        List<(string axis, float value)> GetChangedAxis(Vector3 last, Vector3 cur)
        {
            (string axis, float value) data = default;
            var result = new List<(string axis, float value)>();
            
            if (!Mathf.Approximately(last.x, cur.x))
            {
                data.axis = "x";
                data.value = cur.x;
                result.Add(data);
            }
            
            if(!Mathf.Approximately(last.y, cur.y))
            {
                data.axis = "y";
                data.value = cur.y;
                result.Add(data);
            }
            
            if(!Mathf.Approximately(last.z, cur.z))
            {
                data.axis = "z";
                data.value = cur.z;
                result.Add(data);
            }
            
            return result;
        }
    }

    private Dictionary<Object, Dictionary<Type, BadassAnimation.Handler>> objectByHandlerType = new();
    private static readonly Stack<SerializedProperty> propertyStack = new ();
    
    private void OnBeginProperty(Rect arg1, GUIContent arg2, SerializedProperty arg3)
    {
        propertyStack.Push(arg3);
        EditorGUI.BeginChangeCheck();
    }
    
    private void OnEndProperty()
    {
        var prop = propertyStack.Pop();
        if(prop == null) return;
        if (EditorGUI.EndChangeCheck())
        {
            var target = prop.serializedObject.targetObject;
            
            if (target is Transform transform)
            {
                HandleTransformPropertyChanges(transform, prop);
            }
        }
    }

    private OdinMenuItem FindHandlerItem(BadassAnimation.Handler handler)
    {
        OdinMenuItem handlerItem = MenuTree.RootMenuItem.GetChildMenuItemsRecursive(true)
            .OfType<HandlerItem>().FirstOrDefault(x => x.handler == handler);
        return handlerItem;
    }
    
    private void HandleTransformPropertyChanges(Transform transform, SerializedProperty prop)
    {
        if (!objectByHandlerType.TryGetValue(transform, out Dictionary<Type, BadassAnimation.Handler> handlers))
        {
            objectByHandlerType[transform] = handlers = new Dictionary<Type, BadassAnimation.Handler>();
        }

        TryAddHandler<TransformPosition>("m_LocalPosition");
        TryAddHandler<TransformRotation>("m_LocalRotation");
        TryAddHandler<TransformScale>("m_LocalScale");

        void TryAddHandler<T>(string propName) where T : BaseTransformHandler, new()
        {
            var split = prop.propertyPath.Split('.');
            if (split.Length < 2) return;

            if (split[0] == propName)
            {
                var handler = TryAddTransformHandler<T>(handlers, transform);
                OdinMenuItem handlerItem = FindHandlerItem(handler);

                if (handlerItem != null)
                {
                    if (split[0] == "m_LocalRotation")
                    {
                        var axis = split[1];
                        Patchers._Transform.SetLocalEulerAngles.Called += TryModifyRotationCurve;

                        void TryModifyRotationCurve(Vector3 eulerAngles)
                        {
                            Patchers._Transform.SetLocalEulerAngles.Called -= TryModifyRotationCurve;
                            
                            TryModify("x", eulerAngles.x);
                            TryModify("y", eulerAngles.y);
                            TryModify("z", eulerAngles.z);
                            void TryModify(string curveName, float value)
                            {
                                if (axis == curveName)
                                {
                                    ModifyCurve(handlerItem, curveName, value);
                                }
                            }
                        }
                        return;
                    }
                    
                    TryModifyCurve("x");
                    TryModifyCurve("y");
                    TryModifyCurve("z");

                    void TryModifyCurve(string curveName)
                    {
                        if (split[1] == curveName)
                        {
                            ModifyCurve(handlerItem, curveName, prop.floatValue);
                        }
                    }
                }
            }
        }
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
            LSHandles.StartMatrix(editor.curvesEditor.matrix);
            curveItem.editor.SetKeyY(timePointer.Time, value);
            LSHandles.EndMatrix();
        }
    }
}