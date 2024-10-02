using System;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;

namespace LSCore.QuestModule
{
    public partial class QuestsManager
    {
        [Serializable]
        public abstract class CreateHandler : Handler
        {
            public const string lastCreationDt = nameof(lastCreationDt);
            public static string NewQuestId => Guid.NewGuid().ToString("N");
            private JObject questIdsMap;

            protected sealed override void OnInit()
            {
                if (Config[questIds] == null)
                {
                    questIdsMap = new JObject();
                    Config[questIds] = questIdsMap;
                }

                StartCreating();
            }
            
            protected abstract void StartCreating();

            protected RJToken Create(string questId, Quest quest)
            {
                var questToken = GetQuest(questId);
                questToken[createdAt] = DateTime.UtcNow.Ticks;
                questIdsMap.Add(questId, quest.Id);
                
                return questToken;
            }
            
            protected void CreateBySelector(QuestsSelector selector)
            {
                foreach (var quest in selector.Select(quests))
                {
                    var id = NewQuestId;
                    var token = Create(id, quest);
                    quest.BuildTargetData(token);
                }
            }
        }
    }
}