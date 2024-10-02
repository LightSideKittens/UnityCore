using System;
using LSCore.Attributes;

namespace LSCore.LifecycleSystem
{
    public partial class LifecycleManager
    {
        [Serializable]
        public abstract class StartHandler : Handler
        {
            protected void Start(string objId)
            {
                var objToken = GetObj(objId);
                objToken[startedAt] = DateTime.UtcNow.Ticks;
            }
        }
        
        [Serializable]
        public class StartAfterCreate : StartHandler
        {
            [TimeSpan(0, 5, 0)] 
            public long time;
            
            protected void Start(string objId, TimeSpan timeForStart)
            {
                DoForObjAfterTime(objId, createdAt, timeForStart, Start);
            }
            
            protected override void OnInit()
            {
                DoForEachAfterTime(time, Start);
            }
        }
    }
}