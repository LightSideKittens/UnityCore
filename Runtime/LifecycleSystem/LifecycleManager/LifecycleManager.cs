using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.LifecycleSystem
{
    public partial class LifecycleManager : ScriptableObject
    {
        public const string createdAt = nameof(createdAt);
        public const string startedAt = nameof(startedAt);
        public const string finishedAt = nameof(finishedAt);
        public const string completedAt = nameof(completedAt);

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
        public List<LifecycleObject> objs;
        
        [Required]
        [SerializeReference] public CreateHandler create;
        [SerializeReference] public StartHandler start;
        [SerializeReference] public FinishHandler finish;
        [SerializeReference] public DeleteHandler delete;

        private Dictionary<string, LifecycleObject> objById = new();
        private string systemId;
        protected JToken Config => LifecycleConfig.Get(systemId, LifecycleConfig.Type.Data, id);
        
        public void Init(string systemId)
        {
            this.systemId = systemId;
            objById = objs.ToDictionary(x => x.Id);
            
            create?.Init(systemId, id, objs);
            start?.Init(systemId, id, objs);
            finish?.Init(systemId, id, objs);
            delete?.Init(systemId, id, objs);
        }

        public IEnumerable<LifecycleObject> Create(string placementId)
        {
            var objIdsMap = (JObject)Config[Handler.ids];
                
            if(objIdsMap == null) yield break;
            
            foreach (var prop in objIdsMap.Properties())
            {
                string objId = prop.Name;
                string viewId = prop.Value.ToString();
   
                yield return objById[viewId].Create(systemId, placementId, create.GetObjPath(objId));
            }
        }
    }
}