using System;
using LSCore.Attributes;

namespace LSCore.QuestModule
{
    public partial class QuestsManager
    {
        [Serializable]
        public abstract class StartHandler : Handler
        {
            protected void Start(string questId)
            {
                var questToken = GetQuest(questId);
                questToken[startedAt] = DateTime.UtcNow.Ticks;
            }
        }
        
        [Serializable]
        public class StartAfterCreate : StartHandler
        {
            [TimeSpan(0, 5, 0)] 
            public long time;
            
            protected void Start(string questId, TimeSpan timeForStart)
            {
                DoForQuestAfterTime(questId, createdAt, timeForStart, Start);
            }
            
            protected override void OnInit()
            {
                DoForEachAfterTime(time, Start);
            }
        }
    }
}