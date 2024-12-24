using UnityEngine;
using UnityEngine.UI;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        public static class Canvas
        {
            private static readonly EditorHiddenObjectPool<Text> texts = new(shouldStoreActive: true);
            private static UnityEngine.Canvas canvas;

            private static UnityEngine.Canvas Canvas
            {
                get
                {
                    if (canvas == null)
                    {
                        canvas = new GameObject("PreviewCanvas").AddComponent<UnityEngine.Canvas>();
                        canvas.gameObject.hideFlags = HideFlags.HideAndDontSave;
                        AddGameObject(canvas);
                    }
                    
                    return canvas;
                }
            }

            static Canvas()
            {
                texts.Created += AddCanvasGameObject;
                texts.Got += text => text.enabled = true;
                texts.Released += text => text.enabled = false;
                releasePools += texts.ReleaseAll;
            }

            private static void AddCanvasGameObject<T>(T comp) where T : Component
            {
                AddGameObject(comp);
                comp.transform.SetParent(Canvas.transform);
            }
            
            private static Text GetText(Color color, int fontSize)
            {
                var text = texts.Get();
                text.color = color;
                text.fontSize = fontSize;
                
                return text;
            }
        }
    }
}