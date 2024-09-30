using System.Collections.Generic;
using System.Linq;
using LSCore.LifecycleSystem;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.ObjectModule
{
    public partial class LifecycleManager<T> : ScriptableObject where T : ILifecycleObject<T>
    {
        public class Config : LifecycleConfig<T>
        {
            public static JToken View(string path) => Get(Type.View, path);
            public static JToken Data(string path) => Get(Type.Data, path);
        }
        
        public const string createdAt = nameof(createdAt);
        public const string startedAt = nameof(startedAt);
        public const string finishedAt = nameof(finishedAt);

        public static IEnumerable<string> TimeMarkKeys
        {
            get
            {
                yield return createdAt;
                yield return startedAt;
                yield return finishedAt;
            }
        }

        [GenerateGuid] public string id;
        public List<T> objs;
        
        [Required]
        [SerializeReference] public CreateHandler create;
        [SerializeReference] public StartHandler start;
        [SerializeReference] public FinishHandler finish;
        [SerializeReference] public DeleteHandler delete;

        private Dictionary<string, T> objById = new();
        protected JToken Cfg => LifecycleConfig<T>.Get(LifecycleConfig<T>.Type.Data, id);
        
        public void Init()
        {
            objById = objs.ToDictionary(x => x.Id);
            
            create?.Init(id, objs);
            start?.Init(id, objs);
            finish?.Init(id, objs);
            delete?.Init(id, objs);
        }

        public IEnumerable<T> Create(string placementId)
        {
            var objIdsMap = (JObject)Cfg[Handler.objIds];
                
            if(objIdsMap == null) yield break;
            
            foreach (var prop in objIdsMap.Properties())
            {
                string objId = prop.Name;
                string viewId = prop.Value.ToString();
   
                yield return objById[viewId].Create(placementId, create.GetObjectPath(objId));
            }
        }
    }
}