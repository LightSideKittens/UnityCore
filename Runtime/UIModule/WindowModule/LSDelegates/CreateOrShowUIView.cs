﻿using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
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
        [SerializeReference] public List<DoIt> transformActions;
        
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
        
        public override void Do()
        {
            var last = obj;
            base.Do();
            var isCreated = obj != last;

            if (!isCreated && showOnlyWhenCreated)
            {
                return;
            }
            
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