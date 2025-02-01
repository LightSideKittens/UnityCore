using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class ObjectExtensions
    {
        public static bool TryGetGameObject(this Object obj, out GameObject gameObject)
        {
            if (obj is Component comp)
            {
                gameObject = comp.gameObject;
                return true;
            }
            
            if (obj is GameObject go)
            {
                gameObject = go;
                return true;
            }
            
            gameObject = null;
            return false;
        }
    }
}