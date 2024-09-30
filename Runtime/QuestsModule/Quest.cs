using System;
using System.Collections.Generic;
using System.IO;
using LSCore.Attributes;
using LSCore.ConditionModule;
using LSCore.ConfigModule;
using LSCore.Extensions;
using LSCore.LifecycleSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.QuestModule
{
    [ConfigPath("Quests")]
    public class Quest : MonoBehaviour, ILifecycleObject<Quest>
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

        private string placementId;
        
        private string questId;
        private RJToken lastQuestData;
        private RJToken targetQuestData;

        public string Id => id;
        private string ViewDataPath => useId ? Path.Combine(questId, $"{placementId}{id}") : questId;

        public Quest Create(string placementId, string questId)
        {
            gameObject.SetActive(false);

            var quest = Instantiate(this);
            quest.InitData(placementId, questId);
            
            quest.gameObject.SetActive(true);
            gameObject.SetActive(true);
            return quest;
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

        private void InitData(string placementId, string questId)
        {
            this.placementId = placementId;
            this.questId = questId;
            lastQuestData = new(QuestsManager.Config.View(ViewDataPath));
            targetQuestData = new(QuestsManager.Config.Data(questId));
            
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                handler.lastQuestData = lastQuestData;
                handler.targetQuestData = targetQuestData;
            }

            if (handlers)
            {
                targetQuestData["completedAt"] = DateTime.UtcNow.Ticks;
                onComplete.Invoke(this);
            }
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