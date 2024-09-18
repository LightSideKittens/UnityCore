using System;
using System.Threading.Tasks;
using Cronos;
using LSCore.Attributes;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.QuestModule
{
    public partial class QuestsManager
    {
        [Serializable]
        public abstract class CreateHandler : Handler
        {
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
        
        [Serializable]
        public class CreateImmediate : CreateHandler
        {
            [SerializeReference] public QuestsSelector selector;
            
            protected override void StartCreating()
            {
                if(Config["wasCreated"] != null) return;
                CreateBySelector(selector);
                Config["wasCreated"] = true;
            }
        }

        [Serializable]
        public abstract class CreateByTime : CreateHandler
        {
            public const string lastCreationDt = nameof(lastCreationDt);
            
            [HideIf("HideOneTimeField")] public bool oneTime;
            [SerializeReference] public QuestsSelector selector;

            protected virtual bool HideOneTimeField => false;
            protected abstract bool CreateImmediately { get; set; }
            protected abstract Task Wait();

            protected override async void StartCreating()
            {
                if (Config[lastCreationDt] == null)
                {
                    if (CreateImmediately && !oneTime)
                    {
                        Create();
                        return;
                    }
                }
                
                await Wait();
                Create();
            }

            private void Create()
            {
                CreateBySelector(selector);
                Config[lastCreationDt] = DateTime.UtcNow;

                if (!oneTime)
                {
                    StartCreating();
                }
            }
        }

        [Serializable]
        public class CreateByTimeSpan : CreateByTime
        {
            [TimeSpan(0, 5, 0)] 
            public long time;
            
            [field: HideIf("oneTime")]
            [field: SerializeField] protected override bool CreateImmediately { get; set; }
            
            protected override async Task Wait()
            {
                var dt = Config[lastCreationDt]?.ToObject<long>().ToDateTime() ?? DateTime.UtcNow;
                var span = time.ToTimeSpan();
                var timeSince = DateTime.UtcNow - dt;

                if (span > timeSince)
                {
                    await Task.Delay(span - timeSince);
                }
            }
        }
        

        [Serializable]
        public class CreateByDateTime : CreateByTime
        {
            [DateTime] public long time;
            
            protected override void StartCreating()
            {
                oneTime = true;
                base.StartCreating();
            }

            protected override bool CreateImmediately { get; set; } = false;
            protected override bool HideOneTimeField => true;

            protected override async Task Wait()
            {
                var targetDt = time.ToDateTime();
                var now = DateTime.UtcNow;
                    
                if (targetDt > now)
                {
                    await Task.Delay(targetDt - now);
                }
            }
        }
        
        [Serializable]
        public class CreateByCron : CreateByTime
        {
            [CronEx] public string cron;
            protected override bool CreateImmediately { get; set; } = false;
            
            protected override async Task Wait()
            {
                var dt = Config[lastCreationDt]?.ToObject<long>().ToDateTime() ?? DateTime.UtcNow;
                var target = CronExpression.Parse(cron).GetNextOccurrence(dt) ?? DateTime.MaxValue;
                await Task.Delay(target - dt);
            }
        }
    }
}