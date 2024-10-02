using System;
using System.Collections.Generic;
using System.IO;
using LSCore.Attributes;
using LSCore.ConditionModule;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.LifecycleSystem
{
    public class LifecycleObject : MonoBehaviour
    {
        [Serializable]
        public abstract class Action : LSAction<LifecycleObject> { }

        [Serializable]
        [Unwrap]
        public class ActionWrapper : Action
        {
            [SerializeReference] public LSAction action;
            
            public override void Invoke(LifecycleObject value)
            {
                action?.Invoke();
            }
        }
        
        [Serializable]
        public class Handlers : Conditions<Handler> { }
        
        [Serializable]
        public abstract class Handler : Condition
        {
            public RJToken lastObjData;
            public RJToken targetObjData;
            
            public abstract void BuildTargetData(RJToken objToken);

            public void SetupView()
            {
                OnSetupView();
            }
            
            protected abstract void OnSetupView();
            public abstract void OnShowed();
        }

        public const string isNew = nameof(isNew);

        [SerializeField] private NumberMark markPrefab;
        [SerializeField] public LSImage cullEvent;
        [SerializeReference] public Handlers handlers;
        [SerializeReference] public List<Action> onComplete;

        [SerializeField] private bool useId;

        [ShowIf("useId")]
        [GenerateGuid]
        [SerializeField] private string id;

        private string systemId;
        private string placementId;
        private string objId;
        
        private RJToken lastObjData;
        private RJToken targetObjData;

        public string Id => id;
        private string ViewDataPath => useId ? Path.Combine(objId, $"{placementId}{id}") : objId;

        public LifecycleObject Create(string systemId, string placementId, string objId)
        {
            gameObject.SetActive(false);

            var obj = Instantiate(this);
            obj.InitData(systemId, placementId, objId);
            
            obj.gameObject.SetActive(true);
            gameObject.SetActive(true);
            return obj;
        }

        private void InitData(string systemId, string placementId, string objId)
        {
            this.systemId = systemId;
            this.placementId = placementId;
            this.objId = objId;
            lastObjData = new(LifecycleConfig.Get(systemId, LifecycleConfig.Type.View, ViewDataPath));
            targetObjData = new(LifecycleConfig.Get(systemId, LifecycleConfig.Type.Data, objId));
            
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                handler.lastObjData = lastObjData;
                handler.targetObjData = targetObjData;
            }

            if (handlers)
            {
                targetObjData[LifecycleManager.completedAt] = DateTime.UtcNow.Ticks;
                onComplete.Invoke(this);
            }
        }
        
        public void BuildTargetData(RJToken token)
        {
            token[isNew] = true;
            
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].BuildTargetData(token);
            }
            
            markPrefab.Increase();
        }

        private void Awake()
        {
            cullEvent.Showed += OnShowed;
            
            for (int i = 0; i < handlers.Count; i++)
            {
                cullEvent.Showed += handlers[i].OnShowed;
            }

            void OnShowed()
            {
                cullEvent.Showed -= OnShowed;
                
                if (targetObjData[isNew]!.ToObject<bool>())
                {
                    Debug.Log("OnCullChanged");
                    targetObjData[isNew] = false;
                    markPrefab.Decrease();
                }
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].SetupView();
            }
        }
    }
}