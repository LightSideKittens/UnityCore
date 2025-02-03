using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LSCore
{
    public partial class PlayerWebSocketClient
    {
        [Button]
        public void SendHierarchy()
        {
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            var hierarchy = new JObject();
            
            foreach (GameObject root in rootGameObjects)
            {
                hierarchy.Add(ObjHash(root), SerializeGameObject(root));
            }
            
            SendMethod(nameof(SendHierarchy), hierarchy);
        }
        
        public void SendGameObject(GameObject go)
        {
            UnityComponentConverter.rootPath = string.Empty;
            var compsArr = new JArray();
            var goData = new JObject()
            {
                {"hash", ObjHash(go)},
                {"components", compsArr},
            };
            
            go.GetComponents(compsList);
            foreach (var comp in compsList)
            {
                hashToObject[comp] = ObjHash(comp);
                var compToken = UnityComponentConverter.Serialize(comp, serializer);
                compsArr.Add(compToken);
            }
            
            SendMethod(nameof(SendGameObject), goData);
        }
    }
}