using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LSCore.Extensions.Unity
{
    public static class LSScreen
    {
        public static float Aspect { get; private set; }
        
        public static int Width { get; private set; }
        private static int width
        {
            get
            {
#if UNITY_EDITOR
                int width;
                if (!Application.isPlaying)
                {
                    var cam = Camera.main;
                    width = cam.pixelWidth;
                }
                else
                {
                    width = Screen.width;
                }

                return width;
#else
                return Screen.width;
#endif
            }
        }
        
        public static int Height { get; private set; }
        private static int height
        {
            get
            {
#if UNITY_EDITOR
                int height;
                if (!Application.isPlaying)
                {
                    var cam = Camera.main;
                    height = cam.pixelHeight;
                }
                else
                {
                    height = Screen.height;
                }

                return height;
#else
                return Screen.height;
#endif
            }
        }
        
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
#if UNITY_EDITOR
            EditorApplication.update -= Init;
#endif
            Width = width;
            Height = height;
            Aspect = IsPortrait ? (float)Width/ Height : (float)Height / Width;
            Rect = new Rect(Vector2.zero, new Vector2(Width, Height));
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Editor_Init()
        {
            EditorApplication.update += Init;
        }
#endif
    }
}