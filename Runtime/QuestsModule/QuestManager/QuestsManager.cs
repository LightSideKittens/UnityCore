using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.QuestModule
{
    public partial class QuestsManager : ScriptableObject
    {
        public const string createdAt = nameof(createdAt);
        public const string startedAt = nameof(startedAt);
        public const string finishedAt = nameof(finishedAt);
        public const string completedAt = nameof(completedAt);

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
        
        [Required]
        [SerializeReference] public CreateHandler create;
        [SerializeReference] public StartHandler start;
        [SerializeReference] public FinishHandler finish;
        [SerializeReference] public DeleteHandler delete;

        private Dictionary<string, Quest> questById = new();
        private string systemId;
        protected JToken Config => QuestConfig.Get(systemId, QuestConfig.Type.Data, id);
        
        public void Init(string systemId)
        {
            this.systemId = systemId;
            questById = quests.ToDictionary(x => x.Id);
            
            create?.Init(systemId, id, quests);
            start?.Init(systemId, id, quests);
            finish?.Init(systemId, id, quests);
            delete?.Init(systemId, id, quests);
        }

        public IEnumerable<Quest> Create(string placementId)
        {
            var questIdsMap = (JObject)Config[Handler.questIds];
                
            if(questIdsMap == null) yield break;
            
            foreach (var prop in questIdsMap.Properties())
            {
                string questId = prop.Name;
                string viewId = prop.Value.ToString();
   
                yield return questById[viewId].Create(systemId, placementId, create.GetQuestPath(questId));
            }
        }
    }
}