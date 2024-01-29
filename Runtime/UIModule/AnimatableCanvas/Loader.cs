using System;
using UnityEngine;
using static Animatable.AnimatableCanvas;
using Object = UnityEngine.Object;

namespace Animatable
{
    [Serializable]
    public class Loader
    {
        [SerializeField] private GameObject gameObject;

        public static Loader Create()
        {
            var animText = AnimatableCanvas.Loader;

            var scale = animText.gameObject.transform.localScale;
            var obj = Object.Instantiate(animText.gameObject, SpawnPoint);
            obj.transform.localScale = scale;

            return new Loader
            {
                gameObject = obj,
            };
        }

        public void Destroy()
        {
            Object.Destroy(gameObject);
        }
    }
}