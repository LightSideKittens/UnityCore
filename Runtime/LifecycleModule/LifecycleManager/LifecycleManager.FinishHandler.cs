using System;
using LSCore.Attributes;

namespace LSCore.ObjectModule
{
    public partial class LifecycleManager<T>
    {
        [Serializable]
        public abstract class FinishHandler : Handler
        {
            protected void Finish(string objId)
            {
                var objToken = GetObject(objId);
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
                DoForObjectAfterTime(objId, startedAt, timeForFinish, Finish);
            }
            
            protected override void OnInit()
            {
                DoForEachAfterTime(time, Finish);
            }
        }
    }
}