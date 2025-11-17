using System;
using System.Collections.Generic;
using System.IO;
using LSCore.ConditionModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.LifecycleSystem
{
    public class LifecycleObject : MonoBehaviour
    {
        [Serializable]
        public class Handlers : Ifs<Handler> { }
        
        [Serializable]
        public abstract class Handler : If
        {
            public JToken lastObjData;
            public JToken targetObjData;
            
            public abstract void BuildTargetData(JToken targetData);
            
            public abstract void SetupView();
            public abstract void OnShowed();
        }

        public const string isNew = nameof(isNew);

        //[SerializeField] private NumberMark markPrefab; //TODO: Improve Mark logic
        [SerializeField] public LSImage cullEvent;
        [SerializeReference] public Handlers handlers;
        [SerializeReference] public List<DoIt> onComplete;
        
        [SerializeField] private bool useId;

        [ShowIf("useId")]
        [GenerateGuid]
        [SerializeField] private string id;

        private string systemId;
        private string placementId;
        private string objId;
        
        private JToken lastObjData;
        private JToken targetObjData;

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
            lastObjData = LifecycleConfig.Get(systemId, LifecycleConfig.Type.View, ViewDataPath);
            targetObjData = LifecycleConfig.Get(systemId, LifecycleConfig.Type.Data, objId);
            
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                handler.lastObjData = lastObjData;
                handler.targetObjData = targetObjData;
            }

            if (handlers)
            {
                targetObjData[LifecycleManager.completedAt] = DateTime.UtcNow.Ticks;
                onComplete.Do();
            }
        }
        
        public void BuildTargetData(JToken token)
        {
            token[isNew] = true;
            
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].BuildTargetData(token);
            }
            
            //markPrefab.Increase();
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
                    targetObjData[isNew] = false;
                    //markPrefab.Decrease();
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