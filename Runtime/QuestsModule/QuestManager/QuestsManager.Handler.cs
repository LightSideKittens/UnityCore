using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LSCore.Attributes;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;

namespace LSCore.QuestModule
{
    public partial class QuestsManager
    {
        [Serializable]
        [HideReferenceObjectPicker]
        [TypeFrom]
        public abstract class Handler
        {
            public const string questIds = nameof(questIds);
            
            [NonSerialized] public string managerId;
            [NonSerialized] public List<Quest> quests;
            protected JToken Config => QuestConfig.Get(QuestConfig.Type.Data, managerId);
            protected RJToken GetQuest(string questId) => new(QuestConfig.Get(QuestConfig.Type.Data, GetQuestPath(questId)));
            
            public string GetQuestPath(string questId) => Path.Combine(managerId, questId);
            
            public void Init(string id, List<Quest> quests)
            {
                managerId = id;
                this.quests = quests;
                OnInit();
            }

            protected abstract void OnInit();

            public void DoForEachAfterTime(long time, Action<string, TimeSpan> action)
            {
                var questIdsMap = (JObject)Config[questIds];
                
                if(questIdsMap == null) return;
                
                var timeForDo = new TimeSpan(time);
            
                foreach (var prop in questIdsMap.Properties())
                {
                    string questId = prop.Name;
                    action(questId, timeForDo);
                }
            }
            
            public void DoForQuestAfterTime(string questId, string timeMarkKey, TimeSpan timeForDo, Action<string> action)
            {
                var questToken = GetQuest(questId);
                var timeMark = questToken[timeMarkKey];
                
                if (timeMark == null)
                {
                    questToken.setActions.Add(timeMarkKey, Do);
                    return;
                }

                Do(timeMark);

                return;

                async void Do(JToken token)
                {
                    var dt = new DateTime(token.ToObject<long>());
                    var timeSince = DateTime.UtcNow - dt;

                    if (timeSince < timeForDo)
                    {
                        await Task.Delay(timeForDo - timeSince);
                    }
                
                    action(questId);
                }
            }
        }
    }
}