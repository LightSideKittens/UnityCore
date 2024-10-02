using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.LifecycleSystem
{
    public abstract class LifecycleSystem<T> : SingleService<T> where T : LifecycleSystem<T>
    {
        [Serializable]
        public class CreateLifecycleObjects : LSAction
        {
            public string placementId;
            [SerializeReference] public List<TransformAction> transformActions;
            private bool isCreated;

            public override void Invoke()
            {
                if(isCreated) return;
                isCreated = true;
                
                foreach (var obj in Create(placementId))
                {
                    transformActions.Invoke(obj.transform);
                }
            }
        }

        [SerializeField] private string systemId = "DefaultLifecycleSystem";
        [SerializeField] private LifecycleManager[] managers;

        protected override bool CreateImmediately => true;

        protected override void Init()
        {
            base.Init();
            
            for (int i = 0; i < managers.Length; i++)
            {
                managers[i].Init(systemId);
            }
        }

        public static IEnumerable<LifecycleObject> Create(string placementId)
        {
            foreach (var manager in Instance.managers)
            {
                foreach (var obj in manager.Create(placementId))
                {
                    yield return obj;
                }
            }
        }
    }
}