using System;
using LSCore.Attributes;

namespace LSCore.QuestModule
{
    public partial class QuestsManager
    {
        [Serializable]
        public abstract class FinishHandler : Handler
        {
            protected void Finish(string questId)
            {
                var questToken = GetQuest(questId);
                questToken[finishedAt] = DateTime.UtcNow.Ticks;
            }
        }

        [Serializable]
        public class FinishAfterStart : FinishHandler
        {
            [TimeSpan] public long time;
            
            protected void Finish(string questId, TimeSpan timeForFinish)
            {
                DoForQuestAfterTime(questId, startedAt, timeForFinish, Finish);
            }
            
            protected override void OnInit()
            {
                DoForEachAfterTime(time, Finish);
            }
        }
    }
}