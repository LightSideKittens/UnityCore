using System;
using LSCore;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animatable
{
    [Serializable]
    public class ImageAnim
    {
        public LSImage image;
        private OnOffPool<LSImage> pool;
        
        internal void Init()
        {
            pool = new OnOffPool<LSImage>(image, AnimatableCanvas.SpawnPoint, shouldStoreActive: true);
        }

        internal void ReleaseAll()
        {
            pool.ReleaseAll();
        }
        
        public void Release(LSImage image)
        {
            pool.Release(image);
        }

        public static ImageAnim Create(Sprite sprite)
        {
            var template = AnimatableCanvas.ImageAnim;
            var instance = template.pool.Get();
            instance.sprite = sprite;
            
            return new ImageAnim()
            {
                pool =  template.pool, 
                image = instance,
            };
        }
        
        public static ImageAnim Create(LSImage image)
        {
            var template = AnimatableCanvas.ImageAnim;
            var instance = Object.Instantiate(image, AnimatableCanvas.SpawnPoint);
            
            return new ImageAnim()
            {
                image = instance,
            };
        }
    }
}