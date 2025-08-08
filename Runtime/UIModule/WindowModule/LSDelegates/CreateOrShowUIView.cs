using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LSCore
{
    [Serializable]
    public class Create<T> : DoIt where T : Component
    {
        public T prefab;
        [NonSerialized] public T obj;
        
        public override void Do()
        {
            obj = Object.Instantiate(prefab);
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
                var destroyEvent = obj.gameObject.AddComponent<DestroyEvent>();
                destroyEvent.Destroyed += () =>
                {
                    objectByPrefab.Remove(prefab);
                };
                objectByPrefab.Add(prefab, obj);
            }
        }
    }
    
    [Serializable]
    public class CreateOrShowUIView<T> : CreateSinglePrefab<T> where T : BaseUIView<T>
    {
        [SerializeReference] public List<DoIt> transformActions;
        public ShowWindowOption option;
        public string id;
        
        public override void Do()
        {
            base.Do();
            transformActions?.Do(obj);
            
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
}