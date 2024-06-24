using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    public class Create<T> : LSFunc<T> where T : Object
    {
        public T viewPrefab;
        public Transform parent;
        private T view;
        public override T Invoke()
        {
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