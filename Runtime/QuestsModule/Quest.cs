using System;
using System.IO;
using LSCore.ConditionModule;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore.QuestModule
{
    public class Quest : MonoBehaviour
    {
        [Serializable]
        public class Handlers : Conditions<Handler> { }
        
        [Serializable]
        public abstract class Handler : Condition
        {
            public RJToken lastQuestData;
            public RJToken targetQuestData;
            
            public abstract void BuildTargetData(RJToken questToken);
            public abstract void SetupView();

            public void OnCullChanged(bool cull)
            {
                if (!cull)
                {
                    OnShowed();
                    lastQuestData.Replace(targetQuestData);
                }
            }
            
            public abstract void OnShowed();
        }

        [SerializeReference] public Image cullEvent;
        [SerializeReference] public Handlers handlers;

        [SerializeField] private bool useViewId;

        [ShowIf("useViewId")]
        [GenerateGuid]
        [SerializeField] private string id;

        private string placementId;
        
        private string questId;
        private RJToken lastQuestData;
        private RJToken targetQuestData;

        private string ViewDataPath => useViewId ? Path.Combine(questId, $"{placementId}{id}") : questId;

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