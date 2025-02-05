using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LSCore
{
    public partial class EditorWebSocketClient
    {
        private Scene scene;

        protected override void InitHandlers()
        {
            AddHandler(OnSendHierarchy);
            AddHandler(OnSendGameObject);
            scene = SceneManager.CreateScene("EditorClientScene");
            SceneManager.LoadScene(scene.name, LoadSceneMode.Additive);
        }

        private void OnSendHierarchy(JToken token)
        {
            var root = (JObject)token;
            foreach (var property in root.Properties())
            {
                CreateGameObjectRecursive(property, null);
            }
        }
        
        private void OnSendGameObject(JToken token)
        {
            var hash = token["hash"].ToString();
            var compsArr = (JArray)token["components"];

            if (hashToObject.TryGetValueFromKey(hash, out object value))
            {
                var go = (GameObject)value;
                go.GetComponents(compsList);
                foreach (var comp in compsList)
                {
                    if(comp is Transform) continue;
                    hashToObject.RemoveFromValue(comp);
                    DestroyImmediate(comp);
                }
                
                types.Clear();
                
                foreach (var comp in compsArr)
                {
                    Type targetType = Type.GetType(comp["type"]!.ToString());
                    types.Add(targetType);
                    hashToObject[comp["hash"]!.ToString()] = targetType == typeof(Transform) ? go.transform : go.AddComponent(targetType);
                }
                
                go.GetComponents(compsList);
                for (int i = 0; i < compsArr.Count; i++)
                {
                    serializer.Populate(compsList[i], compsArr[i]);
                }
            }
        }
    }
}