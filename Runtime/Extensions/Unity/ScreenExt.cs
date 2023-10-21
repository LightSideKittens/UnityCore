using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class ScreenExt
    {
        public static float Aspect { get; private set; }
        public static bool IsPortrait
        {
            get
            {
#if UNITY_EDITOR
                return Screen.height > Screen.width;
#else
                return Screen.orientation == ScreenOrientation.Portrait;
#endif
            }
        }

        public static Rect Rect { get; private set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Aspect = IsPortrait ? (float)Screen.width / Screen.height : (float)Screen.height / Screen.width;
            Rect = new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height));
        }
    }
}