using UnityEngine;

namespace LSCore
{
    public class Scriptable : ScriptableObject, ISerializationCallbackReceiver
    {
        void ISerializationCallbackReceiver.OnBeforeSerialize() => OnSave();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            OnLoad();
#if UNITY_EDITOR
            World.Created -= OnLoad;
            World.Created += OnLoad;
            World.Destroyed -= OnLoad;
            World.Destroyed += OnLoad;
#endif
        }

        protected virtual void OnSave() { }
        protected virtual void OnLoad() { }
    }
}