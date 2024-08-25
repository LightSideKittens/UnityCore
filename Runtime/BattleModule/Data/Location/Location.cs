using System.Collections.Generic;
using LSCore;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class Location : SerializedScriptableObject
    {
        [SerializeField] private bool useChunk;
        [SerializeField] private bool dynamicGeneration;

        [HideIf("useChunk")]
        [SerializeField] private Transform prefab;

        [ShowIf("useChunk")] [SerializeField] [TableMatrix] public Transform[,] chunks;
        [ShowIf("useChunk")] [SerializeField] private float chunkSideSize = 20;
        [ShowIf("@useChunk && !dynamicGeneration")] [SerializeField] private int xChunkCount = 10;
        [ShowIf("@useChunk && !dynamicGeneration")] [SerializeField] private int yChunkCount = 10;
        
        private Vector3 startCameraPosition;
        private Dictionary<Vector2Int, Transform> chunksByIndex = new();
        private Camera cam;
        private Vector2Int lastStartIndex;
        private Vector2Int lastSize;
        private List<Vector2Int> keysToRemove = new();
        private Transform locationParent;

        private void StartDynamicGeneration()
        {
            lastStartIndex = Vector2Int.one * int.MaxValue;
            cam = Camera.main;
            startCameraPosition = cam.transform.position;
            chunksByIndex.Clear();
            World.Updated += DynamicGeneration;
        }

        private void DynamicGeneration()
        {
            var rect = cam.GetRect().Expand(chunkSideSize * 1.5f);
            var size = new Vector2Int((int)(rect.width / 2 / chunkSideSize), (int)(rect.height / 2 / chunkSideSize));
            size += Vector2Int.one;
            var pos = cam.transform.position - startCameraPosition;
            var startIndex = new Vector2Int((int)(pos.x / chunkSideSize), (int)(pos.y / chunkSideSize));

            if (lastStartIndex == startIndex && lastSize == size)
            {
                return;
            }
            
            Vector2Int min = startIndex - size;
            Vector2Int max = startIndex + size;
            
            foreach (var kvp in chunksByIndex)
            {
                var key = kvp.Key;
                if (key.x < min.x || key.y < min.y || key.x > max.x || key.y > max.y)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                if (chunksByIndex.Remove(key, out var chunk))
                {
                    Destroy(chunk.gameObject);
                }
            }
            
            Fill(true,true);
            Fill(false,true);
            Fill(true,false);
            Fill(false,false);
            
            lastStartIndex = startIndex;
            lastSize = size;
            
            return;
            
            void Fill(bool xPositive, bool yPositive)
            {
                Vector2Int xy = startIndex;
                int xFactor = xPositive ? 1 : -1;
                int yFactor = yPositive ? 1 : -1;

                int xEnd = size.x;
                int yEnd = size.y;

                for (int j = 0; j < xEnd; j++)
                {
                    for (int i = 0; i < yEnd; i++)
                    {
                        if (!chunksByIndex.ContainsKey(xy))
                        {
                            var chunk = Instantiate(chunks.GetCyclic(xy.x, xy.y), locationParent);
                            
                            chunk.position = new Vector3(xy.x * chunkSideSize + startCameraPosition.x, xy.y * chunkSideSize + startCameraPosition.y, 0);
                            chunksByIndex.Add(xy, chunk);
                        }
                        
                        xy.y += yFactor;
                    }

                    xy.y = startIndex.y;
                    xy.x += xFactor;
                }
            }
        }
        

        public void Dispose()
        {
            World.Updated -= DynamicGeneration;
        }

        [Button]
        public void Generate()
        {
            locationParent = new GameObject(name).transform;
            if (dynamicGeneration)
            {
                StartDynamicGeneration();
                return;
            }
            
            if (!useChunk)
            {
                Instantiate(prefab, locationParent);
                return;
            }

            var defaultX = -(chunkSideSize * xChunkCount / 2);
            var defaultY = chunkSideSize * yChunkCount / 2;
            var position = new Vector2(defaultX, defaultY);
            
            for (int x = 0; x < xChunkCount; x++)
            {
                position.y = defaultY;
                for (int y = 0; y < yChunkCount; y++)
                {
                    Instantiate(chunks.Random(), position, Quaternion.identity, locationParent);
                    position.y -= chunkSideSize;
                }

                position.x += chunkSideSize;
            }
        }
    }
}