﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.QuestModule
{
    public partial class QuestsManager : ScriptableObject
    {
        public const string createdAt = nameof(createdAt);
        public const string startedAt = nameof(startedAt);
        public const string finishedAt = nameof(finishedAt);

        public static IEnumerable<string> TimeMarkKeys
        {
            get
            {
                yield return createdAt;
                yield return startedAt;
                yield return finishedAt;
            }
        }

        [GenerateGuid] public string id;
        public List<Quest> quests;
        
        [SerializeReference] public CreateHandler create;
        [SerializeReference] public StartHandler start;
        [SerializeReference] public FinishHandler finish;
        [SerializeReference] public DeleteHandler delete;

        private Dictionary<string, Quest> questById = new();
        protected JToken Config => QuestConfig.Get(QuestConfig.Type.Data, id);
        
        public void Init()
        {
            questById = quests.ToDictionary(x => x.Id);
            
            create?.Init(id, quests);
            start?.Init(id, quests);
            finish?.Init(id, quests);
            delete?.Init(id, quests);
        }

        public IEnumerable<Quest> Create(string placementId)
        {
            var questIdsMap= (JObject)Config[Handler.questIds];
                
            if(questIdsMap == null) yield break;
            
            foreach (var prop in questIdsMap.Properties())
            {
                string questId = prop.Name;
                string viewId = prop.Value.ToString();
   
                yield return Quest.Create(placementId, questId, questById[viewId]);
            }
        }
    }
}