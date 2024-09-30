using System;
using LSCore.Attributes;

namespace LSCore.ObjectModule
{
    public partial class LifecycleManager<T>
    {
        [Serializable]
        public abstract class StartHandler : Handler
        {
            protected void Start(string objId)
            {
                var objToken = GetObject(objId);
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
                DoForObjectAfterTime(objId, createdAt, timeForStart, Start);
            }
            
            protected override void OnInit()
            {
                DoForEachAfterTime(time, Start);
            }
        }
    }
}