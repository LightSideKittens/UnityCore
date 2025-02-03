using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LSCore
{
    public partial class EditorWebSocketClient
    {
        private void CreateGameObjectRecursive(JProperty prop, Transform parent)
        {
            JToken token = prop.Value;
            string goName = token["name"]!.ToString();

            GameObject go = new GameObject(goName);
            SceneManager.MoveGameObjectToScene(go, scene);
            var tr = go.transform;
            
            if (parent != null) tr.SetParent(parent);
            
            hashToObject[prop.Name] = go;
            var comps = token["components"]!;

            foreach (var compToken in comps)
            {
                var type = Type.GetType(compToken["type"]!.ToString());
                var hash = compToken["hash"]!.ToString();
                var comp = type == typeof(Transform) ? tr : go.AddComponent(type);
                hashToObject[hash] = comp;
            }
            
            if (token["children"] is JObject children)
            {
                foreach (var childProperty in children.Properties())
                {
                    CreateGameObjectRecursive(childProperty, tr);
                }
            }
        }
    }
}