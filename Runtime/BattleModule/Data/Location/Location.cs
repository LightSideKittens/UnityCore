using LSCore.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Battle.Data
{
    public class Location : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private GameObject[] chunks;
        [SerializeField] private float chunkSideSize = 20;
        [SerializeField] private int xChunkCount = 10;
        [SerializeField] private int yChunkCount = 10;
        
        [Button]
        public void Generate()
        {
            var ground = Instantiate(prefab);
            if(chunks.Length == 0) return;

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