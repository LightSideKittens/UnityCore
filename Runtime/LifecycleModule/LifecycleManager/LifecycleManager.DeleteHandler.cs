using System;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.ObjectModule
{
    public partial class LifecycleManager<T>
    {
        [Serializable]
        public abstract class DeleteHandler : Handler
        {
            protected void Delete(string objId)
            {
                Config[objIds]?[objId]?.Parent?.Remove();
                LifecycleConfig<T>.Delete(LifecycleConfig<T>.Type.Data, GetObjectPath(objId));
                LifecycleConfig<T>.DeletePath(LifecycleConfig<T>.Type.View, objId);
            }
        }

        [Serializable]
        public class DeleteAfterTimeMark : DeleteHandler
        {
   
            [ValueDropdown("Keys")] 
            [SerializeField]
            [Required]
            private string timeMarkKey;
            
            [TimeSpan(0, 5, 0)] 
            public long time;

            private IEnumerable<string> Keys => TimeMarkKeys;
            
            protected void Delete(string objId, TimeSpan timeForDelete)
            {
                DoForObjectAfterTime(objId, timeMarkKey, timeForDelete, Delete);
            }
            
            protected override void OnInit()
            {
                DoForEachAfterTime(time, Delete);
            }
        }
    }
}