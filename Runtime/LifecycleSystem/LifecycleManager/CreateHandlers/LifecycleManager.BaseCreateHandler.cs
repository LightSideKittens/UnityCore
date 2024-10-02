using System;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;

namespace LSCore.LifecycleSystem
{
    public partial class LifecycleManager
    {
        [Serializable]
        public abstract class CreateHandler : Handler
        {
            public const string lastCreationDt = nameof(lastCreationDt);
            public static string NewObjId => Guid.NewGuid().ToString("N");
            private JObject objIdsMap;

            protected sealed override void OnInit()
            {
                if (Config[ids] == null)
                {
                    objIdsMap = new JObject();
                    Config[ids] = objIdsMap;
                }

                StartCreating();
            }
            
            protected abstract void StartCreating();

            protected RJToken Create(string objId, LifecycleObject lifecycleObject)
            {
                var objToken = GetObj(objId);
                objToken[createdAt] = DateTime.UtcNow.Ticks;
                objIdsMap.Add(objId, lifecycleObject.Id);
                
                return objToken;
            }
            
            protected void CreateBySelector(MultipleSelector selector)
            {
                foreach (var obj in selector.Select(objs))
                {
                    var id = NewObjId;
                    var token = Create(id, obj);
                    obj.BuildTargetData(token);
                }
            }
        }
    }
}