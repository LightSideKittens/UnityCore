using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LSCore.Attributes;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.ObjectModule
{
    public partial class LifecycleManager<T>
    {
        [Serializable]
        [HideReferenceObjectPicker]
        [TypeFrom]
        public abstract class Handler
        {
            public const string objIds = nameof(objIds);
            
            [NonSerialized] public string managerId;
            [NonSerialized] public List<T> objs;
            protected JToken Config => LifecycleConfig<T>.Get(LifecycleConfig<T>.Type.Data, managerId);
            protected RJToken GetObject(string objId) => new(LifecycleConfig<T>.Get(LifecycleConfig<T>.Type.Data, GetObjectPath(objId)));
            
            public string GetObjectPath(string objId) => Path.Combine(managerId, objId);
            
            public void Init(string id, List<T> objs)
            {
                managerId = id;
                this.objs = objs;
                OnInit();
            }

            protected abstract void OnInit();

            public void DoForEachAfterTime(long time, Action<string, TimeSpan> action)
            {
                var objIdsMap = (JObject)Config[objIds];
                
                if(objIdsMap == null) return;
                
                var timeForDo = new TimeSpan(time);
            
                foreach (var prop in objIdsMap.Properties())
                {
                    string objId = prop.Name;
                    action(objId, timeForDo);
                }
            }
            
            public void DoForObjectAfterTime(string objId, string timeMarkKey, TimeSpan timeForDo, Action<string> action)
            {
                var objToken = GetObject(objId);
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
                        try
                        {
                            await Task.Delay(timeForDo - timeSince);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            throw;
                        }
                    }
                
                    action(objId);
                }
            }
        }
    }
}