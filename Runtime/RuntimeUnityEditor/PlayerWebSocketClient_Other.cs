using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore
{
    public partial class PlayerWebSocketClient
    {
        private JObject SerializeGameObject(GameObject go)
        {
            hashToObject[ObjHash(go)] = go;
            var tr = go.transform;
            var comps = new JArray();
            var obj = new JObject
            {
                { "name", go.name },
                { "components", comps },
            };
            
            go.GetComponents(compsList);
            foreach (var comp in compsList)
            {
                var type = comp.GetType();
                if(IsIgnoredType(type)) continue;
                var hash = ObjHash(comp);
                hashToObject[hash] = comp;
                var compObj = new JObject()
                {
                    {"type", type.AssemblyQualifiedName},
                    {"hash", hash},
                };
                comps.Add(compObj);
            }
            
            if (tr.childCount > 0)
            {
                var children = new JObject();
                foreach (Transform child in tr)
                {
                    var childGo = child.gameObject;
                    children.Add(ObjHash(childGo), SerializeGameObject(childGo));
                }

                obj.Add("children", children);
            }

            return obj;
        }
    }
}