using System;
using LSCore;
using UnityEngine;

namespace Animatable
{
    [Serializable]
    public class BlockCount
    {
        public Transform prefab;
        private OnOffPool<Transform> pool;
        
        internal void Init()
        {
            pool = new OnOffPool<Transform>(prefab, AnimatableCanvas.SpawnPoint, shouldStoreActive: true);
        }

        internal void ReleaseAll()
        {
            pool.ReleaseAll();
        }

        public static ParticlesAttractor Create()
        {
            return null;
        }
    }
}