using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LSCore.Attributes;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;

namespace LSCore.LifecycleSystem
{
    public partial class LifecycleManager
    {
        [Serializable]
        [HideReferenceObjectPicker]
        [TypeFrom]
        public abstract class Handler
        {
            public const string ids = nameof(ids);
            
            [NonSerialized] public string systemId;
            [NonSerialized] public string managerId;
            [NonSerialized] public List<LifecycleObject> objs;
            protected JToken Config => LifecycleConfig.Get(systemId, LifecycleConfig.Type.Data, managerId);
            protected RJToken GetObj(string objId) => new(LifecycleConfig.Get(systemId, LifecycleConfig.Type.Data, GetObjPath(objId)));
            
            public string GetObjPath(string objId) => Path.Combine(managerId, objId);
            
            public void Init(string systemId, string id, List<LifecycleObject> objs)
            {
                this.systemId = systemId;
                managerId = id;
                this.objs = objs;
                OnInit();
            }

            protected abstract void OnInit();

            public void DoForEachAfterTime(long time, Action<string, TimeSpan> action)
            {
                var objIdsMap = (JObject)Config[ids];
                
                if(objIdsMap == null) return;
                
                var timeForDo = new TimeSpan(time);
            
                foreach (var prop in objIdsMap.Properties())
                {
                    string objId = prop.Name;
                    action(objId, timeForDo);
                }
            }
            
            public void DoForObjAfterTime(string objId, string timeMarkKey, TimeSpan timeForDo, Action<string> action)
            {
                var objToken = GetObj(objId);
                var timeMark = objToken[timeMarkKey];
                
                if (timeMark == null)
                {
                    objToken.setActions.Add(timeMarkKey, Do);
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
                
                    action(objId);
                }
            }
        }
    }
}