using System;
using System.Collections.Generic;
using System.IO;
using LSCore.Attributes;
using LSCore.ConditionModule;
using LSCore.Extensions;
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
        public class ActionWrapper : Action
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
        private string questId;
        
        private RJToken lastQuestData;
        private RJToken targetQuestData;

        public string Id => id;
        private string ViewDataPath => useId ? Path.Combine(questId, $"{placementId}{id}") : questId;

        public Quest Create(string systemId, string placementId, string questId)
        {
            gameObject.SetActive(false);

            var quest = Instantiate(this);
            quest.InitData(systemId, placementId, questId);
            
            quest.gameObject.SetActive(true);
            gameObject.SetActive(true);
            return quest;
        }

        private void InitData(string systemId, string placementId, string questId)
        {
            this.systemId = systemId;
            this.placementId = placementId;
            this.questId = questId;
            lastQuestData = new(QuestConfig.Get(systemId, QuestConfig.Type.View, ViewDataPath));
            targetQuestData = new(QuestConfig.Get(systemId, QuestConfig.Type.Data, questId));
            
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                handler.lastQuestData = lastQuestData;
                handler.targetQuestData = targetQuestData;
            }

            if (handlers)
            {
                targetQuestData[QuestsManager.completedAt] = DateTime.UtcNow.Ticks;
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
                
                if (targetQuestData[isNew]!.ToObject<bool>())
                {
                    Debug.Log("OnCullChanged");
                    targetQuestData[isNew] = false;
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