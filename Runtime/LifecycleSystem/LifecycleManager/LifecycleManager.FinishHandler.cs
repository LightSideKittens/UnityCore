using System;
using LSCore.Attributes;

namespace LSCore.LifecycleSystem
{
    public partial class LifecycleManager
    {
        [Serializable]
        public abstract class FinishHandler : Handler
        {
            protected void Finish(string objId)
            {
                var objToken = GetObj(objId);
                objToken[finishedAt] = DateTime.UtcNow.Ticks;
            }
        }

        [Serializable]
        public class FinishAfterStart : FinishHandler
        {
            [TimeSpan(0, 5, 0)] 
            public long time;
            
            protected void Finish(string objId, TimeSpan timeForFinish)
            {
                DoForObjAfterTime(objId, startedAt, timeForFinish, Finish);
            }
            
            protected override void OnInit()
            {
                DoForEachAfterTime(time, Finish);
            }
        }
    }
}