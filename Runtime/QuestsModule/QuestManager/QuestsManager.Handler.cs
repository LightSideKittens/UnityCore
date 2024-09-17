using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LSCore.QuestModule
{
    public class RJToken
    {
        public Dictionary<object, Action<JToken>> setActions = new();
        
        private JToken token;

        public RJToken(JToken token) => this.token = token;
        
        public virtual JToken? this[object key]
        {
            get => token[key];
            set
            {
                token[key] = value;

                if (setActions.Remove(key, out var action))
                {
                    action(value);
                }
            }
        }

        public void Replace(RJToken value)
        {
            token.Replace(value.token);
        }
    }

    public partial class QuestsManager
    {
        [Serializable]
        public abstract class Handler
        {
            public const string questIds = nameof(questIds);
            
            [NonSerialized] public string managerId;
            [NonSerialized] public List<Quest> quests;
            protected JToken Config => QuestConfig.Get(QuestConfig.Type.Data, managerId);
            protected RJToken GetQuest(string questId) => new(QuestConfig.Get(QuestConfig.Type.Data, GetQuestPath(questId)));
            protected string GetQuestPath(string questId) => Path.Combine(managerId, questId);
            
            public void Init(string id, List<Quest> quests)
            {
                managerId = id;
                this.quests = quests;
                OnInit();
            }

            protected abstract void OnInit();

            public void DoForEachAfterTime(long time, Action<string, TimeSpan> action)
            {
                var questIdsArr= Config[questIds];
                
                if(questIdsArr == null) return;
                
                var timeForDo = new TimeSpan(time);
                
                foreach (var questId in questIdsArr)
                {
                    action(questId.ToString(), timeForDo);
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