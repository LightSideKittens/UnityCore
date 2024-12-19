using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    [Serializable]
    public abstract class TransformAction : LSAction<Transform> { }

    [Serializable]
    public class SetParentAction : TransformAction
    {
        public Transform root;
        public string pathToObject;
        public bool worldPositionStays = true;
        
        public override void Invoke(Transform value)
        {
            var parent = root;
            if (!string.IsNullOrEmpty(pathToObject))
            {
                parent = root.FindComponent<Transform>(pathToObject);
            }
            value.SetParent(parent, worldPositionStays);
        }
    }
    
    [Serializable]
    public class Create<T> : LSAction where T : Component
    {
        public T prefab;
        [NonSerialized] public T obj;
        [SerializeReference] public List<TransformAction> transformActions;
        
        public override void Invoke()
        {
            obj = Object.Instantiate(prefab);
            transformActions?.Invoke(obj.transform);
        }
    }
    
    [Serializable]
    public class CreateSinglePrefab<T> : Create<T> where T : Component
    {
        private static readonly Dictionary<T, T> objectByPrefab = new();
        
        public override void Invoke()
        {
            if (!objectByPrefab.TryGetValue(prefab, out obj))
            {
                base.Invoke();
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
        public ShowWindowOption option;
        public string id;
        public bool showOnlyWhenCreated;
        
        public override void Invoke()
        {
            var last = obj;
            base.Invoke();
            var isCreated = obj != last;

            if (!isCreated && showOnlyWhenCreated)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(id))
            {
                obj.Show(option);
            }
            else
            {
                using (new WindowsData.UseId(id))
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