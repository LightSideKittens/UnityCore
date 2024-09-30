using System;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;

namespace LSCore.ObjectModule
{
    public partial class LifecycleManager<T>
    {
        [Serializable]
        public abstract class CreateHandler : Handler
        {
            public const string lastCreationDt = nameof(lastCreationDt);
            public static string NewObjectId => Guid.NewGuid().ToString("N");
            private JObject objIdsMap;

            protected sealed override void OnInit()
            {
                if (Config[objIds] == null)
                {
                    objIdsMap = new JObject();
                    Config[objIds] = objIdsMap;
                }

                StartCreating();
            }
            
            protected abstract void StartCreating();

            protected RJToken Create(string objId, T obj)
            {
                var objToken = GetObject(objId);
                objToken[createdAt] = DateTime.UtcNow.Ticks;
                objIdsMap.Add(objId, obj.Id);
                
                return objToken;
            }
            
            protected void CreateBySelector(ObjectsSelector selector)
            {
                foreach (var obj in selector.Select(objs))
                {
                    var id = NewObjectId;
                    var token = Create(id, obj);
                    obj.BuildTargetData(token);
                }
            }
        }
    }
}