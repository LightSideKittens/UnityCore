using LSCore.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Battle.Data
{
    public class Location : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private bool useChunk;
        
        [HideIf("useChunk")] [SerializeField] private GameObject[] chunks;
        [HideIf("useChunk")] [SerializeField] private float chunkSideSize = 20;
        [HideIf("useChunk")] [SerializeField] private int xChunkCount = 10;
        [HideIf("useChunk")] [SerializeField] private int yChunkCount = 10;
        
        [Button]
        public void Generate()
        {
            var ground = Instantiate(prefab);
            if(!useChunk) return;

            var defaultX = -(chunkSideSize * xChunkCount / 2);
            var defaultY = chunkSideSize * yChunkCount / 2;
            var position = new Vector2(defaultX, defaultY);
            
            for (int x = 0; x < xChunkCount; x++)
            {
                position.y = defaultY;
                for (int y = 0; y < yChunkCount; y++)
                {
                    Instantiate(chunks.Random(), position, Quaternion.identity, ground.transform);
                    position.y -= chunkSideSize;
                }

                position.x += chunkSideSize;
            }
        }
    }
}