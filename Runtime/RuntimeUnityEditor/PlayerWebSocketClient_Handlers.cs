using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore
{
    public partial class PlayerWebSocketClient
    {
        protected override void InitHandlers()
        {
            AddHandler(OnGetHierarchy);
            AddHandler(OnFetchGameObject);
            AddHandler(OnSendModification);
        }

        private void OnGetHierarchy(JToken token)
        {
            SendHierarchy();
        }

        private void OnFetchGameObject(JToken token)
        {
            var hash = token["hash"].ToString();

            if (hashToObject.TryGetValueFromKey(hash, out object value))
            {
                if (value is GameObject go)
                {
                    SendGameObject(go);
                }
            }
        }

        private void OnSendModification(JToken token)
        {
            var hash = token["hash"].ToString();
            if (hashToObject.TryGetValueFromKey(hash, out object value))
            {
                UnityComponentConverter.Populate(value, token, serializer);
            }
        }
    }
}