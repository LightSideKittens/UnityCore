using System;
using LSCore.ReferenceFrom.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class Create<T> : LSFunc<T> where T : Component
    {
        public T viewPrefab;
        public Transform root;
        public string pathToObject;
        private T view;
        public override T Invoke()
        {
            Transform parent = root;
            if (root != null)
            {
                parent = root.FindComponent<Transform>(pathToObject);
            }
            
            view ??= Object.Instantiate(viewPrefab, parent);
            return view;
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