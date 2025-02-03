using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    public partial class EditorWebSocketClient
    {
        [Button]
        public void GetHierarchy()
        {
            SendMethod(nameof(GetHierarchy));
        }
        
        public void FetchGameObject(GameObject go)
        {
            if (!hashToObject.ContainsValue(go)) return;
            
            var data = new JObject()
            {
                {"hash", hashToObject[go]}
            };
            
            SendMethod(nameof(FetchGameObject), data);
        }
        
        public void SendModification(InspectorProperty property)
        {
            var target = property.SerializationRoot.ValueEntry.WeakSmartValue;
            SendModification(target, property.UnityPropertyPath);
        }
        
        public void SendModification(UndoPropertyModification modification)
        {
            var cur = modification.currentValue;
            SendModification(cur.target, cur.propertyPath);
        }

        private void SendModification(object target, string propertyPath)
        {
            if(!hashToObject.TryGetKeyFromValue(target, out var hash)) return;
            
            UnityComponentConverter.rootPath = string.Empty;
            var compToken = UnityComponentConverter.Serialize(target, serializer, propertyPath);
            SendMethod(nameof(SendModification), compToken);
        }
    }
}