using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LSCore
{
    [Serializable]
    public class Create<T> : DoIt where T : Component
    {
        public T prefab;
        [SerializeReference] public List<DoIt> transformActions;
        [NonSerialized] public T obj;
        
        public override void Do()
        {
            obj = Object.Instantiate(prefab);
            transformActions?.Do(obj);
        }
    }
    
    [Serializable]
    public class CreateSinglePrefab<T> : Create<T> where T : Component
    {
        private static readonly Dictionary<T, T> objectByPrefab = new();
        
        public override void Do()
        {
            if (!objectByPrefab.TryGetValue(prefab, out obj))
            {
                base.Do();
                DestroyEvent.AddOnDestroy(obj.gameObject, () =>
                {
                    objectByPrefab.Remove(prefab);
                });
                objectByPrefab.Add(prefab, obj);
            }
        }
    }
    
    [Serializable]
    public class CreateOrShowUIView<T> : CreateSinglePrefab<T> where T : BaseUIView<T>
    {
        public ShowWindowOption option;
        public string id;
        
        public override void Do()
        {
            base.Do();
            CanvasUpdateRegistry.Updated += Show;

            void Show()
            {
                CanvasUpdateRegistry.Updated -= Show;
                using (new UIViewBoss.UseId(id))
                {
                    obj.Show(option);
                }
            }
        }
    }

    [Serializable]
    public class CreateOrShowCanvasView : CreateOrShowUIView<CanvasView> { }
    [Serializable]
    public class CreateOrShowUIView : CreateOrShowUIView<UIView> { }

    [Serializable]
    public class CreateOrShowUIViewDynamic : CreateOrShowUIView
    {
        [SerializeReference] public Get<UIView> uiView;
        
        public override void Do()
        {
            prefab = uiView.Data; 
            base.Do();
        }
    }
    
    [Serializable]
    public class CreateSinglePrefabDynamic<T> : CreateSinglePrefab<T>  where T : Component
    {
        [SerializeReference] public Get<T> getPrefab;
        
        public override void Do()
        {
            prefab = getPrefab.Data;
            base.Do();
        }
    }
}