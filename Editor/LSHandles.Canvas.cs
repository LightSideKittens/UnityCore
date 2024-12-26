using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.Rendering;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        public static class Canvas
        {
            private static readonly EditorHiddenObjectPool<TextMesh> texts = new(shouldStoreActive: true);

            static Canvas()
            {
                texts.Created += AddCanvasGameObject;
                texts.Got += text => text.GetComponent<MeshRenderer>().enabled = true;
                texts.Released += text => text.GetComponent<MeshRenderer>().enabled = false;
                releasePools += texts.ReleaseAll;
            }

            private static void AddCanvasGameObject<T>(T comp) where T : Component
            {
                AddGameObject(comp);
                comp.gameObject.AddComponent<SortingGroup>();
                comp.transform.localScale = Vector3.one / 3;
            }
            
            public static TextMesh GetText(string message, int fontSize, bool dependsOnCam = true)
            {
                var text = texts.Get();
                currentDrawLayer += 10;
                text.GetComponent<SortingGroup>().sortingOrder = currentDrawLayer;
                text.text = message;
                text.fontSize = fontSize;
                var scale = LSVector3.one;
                
                if (dependsOnCam)
                {
                    scale *= (cam.orthographicSize / 100);
                }

                scale *= ScaleMultiplier;
                text.transform.localScale = scale;
                return text;
            }
        }
    }
}