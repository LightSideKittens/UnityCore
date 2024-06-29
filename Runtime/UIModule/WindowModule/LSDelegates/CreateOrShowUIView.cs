using System;
using System.Collections.Generic;
using LSCore.CommonComponents;
using LSCore.ReferenceFrom.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class Create<T> : LSFunc<T> where T : Component
    {
        private static readonly Dictionary<T, T> objectByPrefab = new();
        
        public T viewPrefab;
        public Transform root;
        public string pathToObject;
        
        public override T Invoke()
        {
            Transform parent = root;
            if (root != null)
            {
                parent = root.FindComponent<Transform>(pathToObject);
            }

            if (!objectByPrefab.TryGetValue(viewPrefab, out var obj))
            {
                obj = Object.Instantiate(viewPrefab, parent);
                var destroyEvent = obj.gameObject.AddComponent<DestroyEvent>();
                destroyEvent.Destroyed += () =>
                {
                    objectByPrefab.Remove(viewPrefab);
                };
                objectByPrefab.Add(viewPrefab, obj);
            }
            
            return obj;
        }
    }
    
    public class CreateOrShowUIView<T> : Create<T> where T : BaseUIView<T>
    {
        public ShowWindowOption option;
        public override T Invoke()
        {
            var view = base.Invoke();
            view.Show(option);
            return view;
        }
    }

    [Serializable]
    public class CreateOrShowCanvasView : CreateOrShowUIView<CanvasView> { }
    [Serializable]
    public class CreateOrShowUIView : CreateOrShowUIView<UIView> { }
    
    [Serializable]
    public abstract class BaseCreateOrShowUIViewAction<TAction, TView> : LSAction where TAction : CreateOrShowUIView<TView> where TView : BaseUIView<TView>
    {
        [InlineProperty]
        [HideReferenceObjectPicker]
        public TAction action;
        
        public override void Invoke()
        {
            action.Invoke();
        }
    }
    
    [Serializable] public class CreateOrShowCanvasViewAction : BaseCreateOrShowUIViewAction<CreateOrShowCanvasView, CanvasView> { }
    [Serializable] public class CreateOrShowUIViewAction : BaseCreateOrShowUIViewAction<CreateOrShowUIView, UIView> { }
}