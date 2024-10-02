using System;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.LifecycleSystem
{
    public partial class LifecycleManager
    {
        [Serializable]
        public abstract class DeleteHandler : Handler
        {
            protected void Delete(string objId)
            {
                Config[ids]?[objId]?.Parent?.Remove();
                LifecycleConfig.Delete(systemId, LifecycleConfig.Type.Data, GetObjPath(objId));
                LifecycleConfig.DeletePath(systemId, LifecycleConfig.Type.View, objId);
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
                DoForObjAfterTime(objId, timeMarkKey, timeForDelete, Delete);
            }
            
            protected override void OnInit()
            {
                DoForEachAfterTime(time, Delete);
            }
        }
    }
}