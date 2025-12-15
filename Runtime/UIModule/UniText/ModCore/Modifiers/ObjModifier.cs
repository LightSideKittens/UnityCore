using System;
using System.Collections.Generic;
using LSCore;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class RectTransformWrapper
{
    private static List<ICanvasElement> canvasElementsBuffer = new();
    public RectTransform instance;
    public RectTransform prefab;
    public Transform parent;
    public Vector3 localScale;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 anchoredPosition;
    public Vector2 pivot;
    public Vector2 sizeDelta;
    private bool created;
    public bool isDirty;
    
    public void Setup()
    {
        if(!isDirty) return;
        isDirty = false;
        if (!created)
        {
            created = true;
            instance = Object.Instantiate(prefab, parent);
            instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
#if UNITY_EDITOR
            ObjTracker.Track(instance.gameObject, this);
#endif
        }
        
        instance.localScale = localScale;
        instance.anchorMin = anchorMin;
        instance.anchorMax = anchorMax;
        instance.anchoredPosition = anchoredPosition;
        instance.pivot = pivot;
        instance.sizeDelta = sizeDelta;
        
        instance.GetComponentsInChildren(canvasElementsBuffer);
        for (int i = 0; i <= (int)CanvasUpdate.PostLayout; i++)
        for (int j = 0; j < canvasElementsBuffer.Count; j++)
            canvasElementsBuffer[j].Rebuild((CanvasUpdate)i);

        for (int i = (int)CanvasUpdate.PreRender; i < (int)CanvasUpdate.LatePreRender; i++)
        for (int j = 0; j < canvasElementsBuffer.Count; j++)
            canvasElementsBuffer[j].Rebuild((CanvasUpdate)i);
    }

    
    public void Destroy()
    {
        if(Application.isPlaying) Object.Destroy(instance.gameObject);
        else Object.DestroyImmediate(instance.gameObject);
    }
}

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

    [NonSerialized] public int activeCount;
    [NonSerialized] public List<RectTransformWrapper> instances = new();

    public RectTransformWrapper GetOrCreate(Transform parent)
    {
        activeCount++;
        
        if (activeCount <= instances.Count)
        {
            return instances[activeCount - 1];
        }
        
        var wrapper = new RectTransformWrapper();
        wrapper.prefab = prefab;
        wrapper.parent = parent;
        instances.Add(wrapper);
        return wrapper;
    }
    
    public void UpdateInstances()
    {
        var diff = activeCount - instances.Count;
        
        if (diff < 0)
        {
            diff *= -1;
            for (int i = 0; i < diff; i++)
            {
                var last = instances.Count - 1;
                instances[last].Destroy();
                instances.RemoveAt(last);
            }
        }

        for (var i = 0; i < instances.Count; i++)
        {
            var instance = instances[i];
            instance.Setup();
        }
    }
}

[Serializable]
public class ObjModifier : BaseModifier
{
    public List<InlineObject> objects = new();

    private Dictionary<int, InlineObject> clusterToObj;
    private Dictionary<string, InlineObject> objLookup;
    private bool objectsPositioned;


    protected override void CreateBuffers()
    {
        clusterToObj = new Dictionary<int, InlineObject>(16);
        objLookup = new Dictionary<string, InlineObject>(objects.Count);
        for (int i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            if (!string.IsNullOrEmpty(obj.name))
                objLookup[obj.name] = obj;
        }
    }

    protected override void Subscribe()
    {
        Canvas.willRenderCanvases += OnPrerender;
        cachedUniText.TextProcessor.Shaped += OnShaped;
        cachedUniText.MeshGenerator.OnRebuildStart += OnRebuildStart;
        cachedUniText.MeshGenerator.OnAfterGlyphs += OnAfterGlyphs;
    }

    protected override void Unsubscribe()
    {
        Canvas.willRenderCanvases -= OnPrerender;
        cachedUniText.TextProcessor.Shaped -= OnShaped;
        cachedUniText.MeshGenerator.OnRebuildStart -= OnRebuildStart;
        cachedUniText.MeshGenerator.OnAfterGlyphs -= OnAfterGlyphs;
    }

    private void OnPrerender()
    {
        for (var i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            obj.UpdateInstances();
        }
    }

    protected override void ReleaseBuffers()
    {
        DestroyAllObjects();
        clusterToObj.Clear();
        objLookup.Clear();
    }

    protected override void ClearBuffers()
    {
        clusterToObj?.Clear();
    }

    protected override void ApplyModifier(int start, int end, string parameter)
    {
        if (string.IsNullOrEmpty(parameter)) return;
        if (objLookup == null || !objLookup.TryGetValue(parameter, out var obj)) return;
        clusterToObj[start] = obj;
    }

    private void OnRebuildStart()
    {
        objectsPositioned = false;
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
        if (objectsPositioned) return;
        objectsPositioned = true;

        if (cachedUniText == null) return;

        var glyphs = cachedUniText.LastResultGlyphs;
        float scale = UniTextMeshGenerator.scale;

        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].activeCount = 0;
        }
        
        // Create or update objects
        foreach (var kvp in clusterToObj)
        {
            int cluster = kvp.Key;
            var obj = kvp.Value;
            if (obj.prefab == null) continue;

            int idx = FindGlyphByCluster(glyphs, cluster);
            if (idx < 0) continue;

            var glyph = glyphs[idx];
            float w = obj.width * scale;
            float h = obj.height * scale;
            float x = glyph.x + obj.bearingX * scale;
            float y = -glyph.y + obj.bearingY * scale;
            CreateObjectInstance(obj, x, y, w, h);
        }
    }

    private static int FindGlyphByCluster(ReadOnlySpan<PositionedGlyph> glyphs, int cluster)
    {
        for (int i = 0; i < glyphs.Length; i++)
            if (glyphs[i].cluster == cluster)
                return i;
        return -1;
    }

    private void CreateObjectInstance(InlineObject obj, float x, float y, float w, float h)
    {
        if (cachedUniText == null) return;
        
        var wrapper = obj.GetOrCreate(cachedUniText.transform);
        wrapper.isDirty = true;
        wrapper.localScale = Vector3.one;

        wrapper.anchorMin = new Vector2(0, 1);
        wrapper.anchorMax = new Vector2(0, 1);
        Vector2 pivot = wrapper.pivot;
        wrapper.anchoredPosition = new Vector2(x + w * pivot.x, y + h * pivot.y);
        wrapper.sizeDelta = new Vector2(w, h);
    }

    private void DestroyAllObjects()
    {
        if(cachedUniText == null) return;
        for (var i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            obj.activeCount = 0;
        }
        Canvas.willRenderCanvases += Destro;
        
        void Destro()
        {
            Canvas.willRenderCanvases -= Destro;
            if(cachedUniText == null) return;
            for (var i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                obj.UpdateInstances();
            }
        }
    }
}

