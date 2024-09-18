using System;
using System.Collections.Generic;
using System.IO;
using LSCore.Attributes;
using LSCore.ConditionModule;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore.QuestModule
{
    public class Quest : MonoBehaviour
    {
        [Serializable]
        public abstract class Action : LSAction<Quest> { }

        [Serializable]
        [Unwrap]
        public class Wrapper : Action
        {
            [SerializeReference] public LSAction action;
            
            public override void Invoke(Quest value)
            {
                action?.Invoke();
            }
        }
        
        [Serializable]
        public class Handlers : Conditions<Handler> { }
        
        [Serializable]
        public abstract class Handler : Condition
        {
            public RJToken lastQuestData;
            public RJToken targetQuestData;
            
            public abstract void BuildTargetData(RJToken questToken);

            public void SetupView()
            {
                OnSetupView();
            }

            public void OnCullChanged(bool cull)
            {
                if (!cull)
                {
                    OnShowed();
                }
            }
            
            public abstract void OnSetupView();
            public abstract void OnShowed();

            protected bool CheckDiffAndSync<T>(object key, RJToken target,
                Action<(T lastValue, T currentValue)> onSync) where T : struct
            {
                return CheckDiffAndSync(key, target[key], onSync);
            }

            protected bool CheckDiffAndSync<T>(object key, JToken currentValue, Action<(T lastValue, T currentValue)> onSync = null) 
            {
                var lastValue = lastQuestData[key];
                
                if (lastValue == null || !JToken.DeepEquals(lastValue, currentValue))
                {
                    var lastVal = lastValue != null ? lastValue.ToObject<T>() ?? default(T) : default;
                    
                    onSync?.Invoke((lastVal, currentValue.ToObject<T>()));
                    lastQuestData[key] = currentValue;
                    return true;
                }
                
                return false;
            }
        }

        [SerializeField] public Image cullEvent;
        [SerializeReference] public Handlers handlers;
        [SerializeReference] public List<Action> onComplete;

        [SerializeField] private bool useId;

        [ShowIf("useId")]
        [GenerateGuid]
        [SerializeField] private string id;

        private string placementId;
        
        private string questId;
        private RJToken lastQuestData;
        private RJToken targetQuestData;

        public string Id => id;
        private string ViewDataPath => useId ? Path.Combine(questId, $"{placementId}{id}") : questId;

        public static Quest Create(string placementId, string questId, Quest prefab)
        {
            prefab.gameObject.SetActive(false);

            var quest = Instantiate(prefab);
            quest.InitData(placementId, questId);
            
            prefab.gameObject.SetActive(true);
            return quest;
        }

        private void InitData(string placementId, string questId)
        {
            this.placementId = placementId;
            this.questId = questId;
            lastQuestData = new(QuestConfig.Get(QuestConfig.Type.View, ViewDataPath));
            targetQuestData = new(QuestConfig.Get(QuestConfig.Type.Data, questId));
            
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                handler.lastQuestData = lastQuestData;
                handler.targetQuestData = targetQuestData;
            }

            if (handlers)
            {
                onComplete.Invoke(this);
            }
        }
        
        public void BuildTargetData(RJToken token)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].BuildTargetData(token);
            }
        }

        private void Awake()
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                cullEvent.onCullStateChanged.AddListener(handlers[i].OnCullChanged);
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