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
            var compsArr = new JArray();
            var goData = new JObject()
            {
                {"hash", ObjHash(go)},
                {"components", compsArr},
            };
            
            go.GetComponents(compsList);
            foreach (var comp in compsList)
            {
                if(IsIgnoredType(comp.GetType())) continue;
                hashToObject[comp] = ObjHash(comp);
                var compToken = serializer.Serialize(comp);
                compsArr.Add(compToken);
            }
            
            SendMethod(nameof(SendGameObject), goData);
        }

        public void SendFrameRate()
        {
            SendMethod(nameof(SendFrameRate), World.FrameRate);
        }
    }
}