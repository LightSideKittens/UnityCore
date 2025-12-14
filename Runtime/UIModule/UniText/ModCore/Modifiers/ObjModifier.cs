using System;
using System.Collections.Generic;
using LSCore;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class InlineObject
{
    public string name;
    public RectTransform prefab;
    public float width = 100;
    public float height = 100;
    public float bearingX;
    public float bearingY;
    public float advance = 1;
    
    [NonSerialized] public RectTransform instance;
    public OnOffPool<RectTransform> pool;
}

[Serializable]
public class ObjModifier : BaseModifier
{
    public List<InlineObject> objects = new();

    private Dictionary<int, InlineObject> clusterToObj;
    private Dictionary<string, InlineObject> objLookup;
    private bool objectsCreated;
    private List<ICanvasElement> canvasElementsBuffer;

    protected override void CreateBuffers()
    {
        clusterToObj = new Dictionary<int, InlineObject>(16);
        objLookup = new Dictionary<string, InlineObject>(objects.Count);
        canvasElementsBuffer = new List<ICanvasElement>();
        for (int i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            if (!string.IsNullOrEmpty(obj.name))
                objLookup[obj.name] = obj;
        }
    }

    protected override void Subscribe()
    {
        cachedUniText.Rebuilding += OnRebuilding;
        cachedUniText.TextProcessor.Shaped += OnShaped;
        cachedUniText.MeshGenerator.OnRebuildStart += OnRebuildStart;
        cachedUniText.MeshGenerator.OnAfterGlyphs += OnAfterGlyphs;
    }

    private void OnRebuilding()
    {
        ClearSpawnedObjects(false);
    }

    protected override void Unsubscribe()
    {
        cachedUniText.Rebuilding -= OnRebuilding;
        cachedUniText.TextProcessor.Shaped -= OnShaped;
        cachedUniText.MeshGenerator.OnRebuildStart -= OnRebuildStart;
        cachedUniText.MeshGenerator.OnAfterGlyphs -= OnAfterGlyphs;
    }

    protected override void ReleaseBuffers()
    {
        ClearSpawnedObjects(false);
        clusterToObj.Clear();
        objLookup.Clear();
        canvasElementsBuffer.Clear();
    }

    protected override void ClearBuffers()
    {
        clusterToObj?.Clear();
        canvasElementsBuffer.Clear();
    }

    protected override void ApplyModifier(int start, int end, string parameter)
    {
        if (string.IsNullOrEmpty(parameter)) return;
        if (objLookup == null || !objLookup.TryGetValue(parameter, out var obj)) return;
        clusterToObj[start] = obj;
    }

    public override void Destroy()
    {
        ClearSpawnedObjects(true);
    }

    private void OnRebuildStart()
    {
        objectsCreated = false;
    }

    private void OnShaped()
    {
        if (clusterToObj == null || clusterToObj.Count == 0) return;

        float fontSize = cachedUniText.FontSize;
        var buf = CommonData.Current;
        var glyphs = buf.shapedGlyphs;
        var runs = buf.shapedRuns;
        int runCount = buf.shapedRunCount;

        // Iterate through runs to properly convert run-local cluster to global cluster
        for (int r = 0; r < runCount; r++)
        {
            ref var run = ref runs[r];
            int clusterOffset = run.range.start; // Global offset for this run
            int glyphEnd = run.glyphStart + run.glyphCount;
            float width = 0;

            for (int g = run.glyphStart; g < glyphEnd; g++)
            {
                // Convert run-local cluster to global cluster index
                int globalCluster = glyphs[g].cluster + clusterOffset;

                if (clusterToObj.TryGetValue(globalCluster, out var obj))
                {
                    glyphs[g].advanceX = obj.advance * fontSize;
                    glyphs[g].offsetX = obj.bearingX * fontSize;
                    glyphs[g].offsetY = obj.bearingY * fontSize;
                }

                width += glyphs[g].advanceX;
            }

            run.width = width;
        }
    }

    private void OnAfterGlyphs()
    {
        if (objectsCreated) return;
        if (clusterToObj == null || clusterToObj.Count == 0 || cachedUniText == null)
            return;
        
        objectsCreated = true;
        var glyphs = cachedUniText.LastResultGlyphs;
        float scale = UniTextMeshGenerator.scale;

        foreach (var kvp in clusterToObj)
        {
            int cluster = kvp.Key;
            var obj = kvp.Value;

            if (obj.prefab == null) continue;

            int idx = -1;
            for (int i = 0; i < glyphs.Length; i++)
            {
                if (glyphs[i].cluster == cluster)
                {
                    idx = i;
                    break;
                }
            }
            if (idx < 0) continue;

            var glyph = glyphs[idx];
            float w = obj.width * scale;
            float h = obj.height * scale;
            float x = glyph.x + obj.bearingX * scale;
            float y = -glyph.y + obj.bearingY * scale;

            CanvasUpdateRegistry.Updated += Instantiate;

            void Instantiate()
            {
                CanvasUpdateRegistry.Updated -= Instantiate;
                var pool = OnOffPool<RectTransform>.GetOrCreatePool(obj.prefab, cachedUniText.transform, shouldStoreActive: true);
                
                var instance = pool.Get();
                obj.instance = instance;
                obj.pool = pool;
                instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
#if UNITY_EDITOR
                PoolTracker.Track(instance.gameObject, pool);
#endif

                instance.anchorMin = new Vector2(0, 1);
                instance.anchorMax = new Vector2(0, 1);
                var pivot = instance.pivot;
                instance.anchoredPosition = new Vector2(x + w * pivot.x, y + h * pivot.y);
                instance.sizeDelta = new Vector2(w, h); 
                instance.GetComponentsInChildren(canvasElementsBuffer);
                
                for (int i = 0; i <= (int)CanvasUpdate.PostLayout; i++)
                {
                    for (var j = 0; j < canvasElementsBuffer.Count; j++)
                    {
                        canvasElementsBuffer[j].Rebuild((CanvasUpdate)i);
                    }
                }

                for (var i = (int)CanvasUpdate.PreRender; i < (int)CanvasUpdate.LatePreRender; i++)
                {
                    for (var j = 0; j < canvasElementsBuffer.Count; j++)
                    {
                        canvasElementsBuffer[j].Rebuild((CanvasUpdate)i);
                    }
                }
            }
        }
    }
    
    private void ClearSpawnedObjects(bool isDestroying)
    {
        if(!objectsCreated) return;
        
        CanvasUpdateRegistry.Updated += Clear;

        void Clear()
        {
            CanvasUpdateRegistry.Updated -= Clear;
            if (isDestroying || cachedUniText == null)
            {
                foreach (var obj in objects)
                {
                    OnOffPool<RectTransform>.RemovePool(obj.prefab);
                }
            }
            else
            {
                foreach (var obj in objects)
                {
                    obj.pool?.Release(obj.instance);
                }
            }
        }
    }
}
