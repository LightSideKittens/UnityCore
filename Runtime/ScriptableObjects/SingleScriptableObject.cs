using UnityEngine;

namespace LSCore
{
    public class SingleScriptableObject<T> : ScriptableObject where T : SingleScriptableObject<T>
    {
        protected static T Instance => SingleAssets.Get<T>();
    }
}