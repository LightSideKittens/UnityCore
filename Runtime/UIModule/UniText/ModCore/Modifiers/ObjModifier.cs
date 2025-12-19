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
    public Vector2 anchoredPosition;
    public Vector2 pivot;
    public Vector2 sizeDelta;
    private bool created;
    public bool isDirty;

    public void Setup()
    {
        if (!isDirty) return;
        isDirty = false;
        if (!created)
        {
            created = true;
            instance = Object.Instantiate(prefab, parent);
#if UNITY_EDITOR
            instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
            ObjTracker.Track(instance.gameObject, this);
#endif
        }

        instance.localScale = Vector3.one;
        instance.anchorMin = new Vector2(0, 1);
        instance.anchorMax = new Vector2(0, 1);
        instance.anchoredPosition = anchoredPosition;
        instance.pivot = pivot;
        instance.sizeDelta = sizeDelta;

        instance.GetComponentsInChildren(canvasElementsBuffer);
        for (var i = 0; i <= (int)CanvasUpdate.PostLayout; i++)
        for (var j = 0; j < canvasElementsBuffer.Count; j++)
            canvasElementsBuffer[j].Rebuild((CanvasUpdate)i);

        for (var i = (int)CanvasUpdate.PreRender; i < (int)CanvasUpdate.LatePreRender; i++)
        for (var j = 0; j < canvasElementsBuffer.Count; j++)
            canvasElementsBuffer[j].Rebuild((CanvasUpdate)i);
    }


    public void Destroy()
    {
        if (Application.isPlaying) Object.Destroy(instance.gameObject);
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

        if (activeCount <= instances.Count) return instances[activeCount - 1];

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
            for (var i = 0; i < diff; i++)
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


    protected override void CreateBuffers()
    {
        clusterToObj = new Dictionary<int, InlineObject>(16);
        objLookup = new Dictionary<string, InlineObject>(objects.Count);
        for (var i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            if (!string.IsNullOrEmpty(obj.name))
                objLookup[obj.name] = obj;
        }
    }

    protected override void Subscribe()
    {
        CanvasUpdateRegistry.Updated += OnPrerender;
        uniText.TextProcessor.Shaped += OnShaped;
        uniText.MeshGenerator.OnRebuildStart += OnRebuildStart;
        uniText.MeshGenerator.OnRebuildEnd += OnRebuildEnd;
    }

    protected override void Unsubscribe()
    {
        CanvasUpdateRegistry.Updated -= OnPrerender;
        uniText.TextProcessor.Shaped -= OnShaped;
        uniText.MeshGenerator.OnRebuildStart -= OnRebuildStart;
        uniText.MeshGenerator.OnRebuildEnd -= OnRebuildEnd;
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
        for (var i = 0; i < objects.Count; i++)
            objects[i].activeCount = 0;
    }

    private void OnShaped()
    {
        if (clusterToObj == null || clusterToObj.Count == 0) return;

        var buf = buffers;
        var fontSize = buf.shapingFontSize > 0 ? buf.shapingFontSize : uniText.FontSize;
        var glyphs = buf.shapedGlyphs;
        var runs = buf.shapedRuns;
        var runCount = buf.shapedRunCount;

        for (var r = 0; r < runCount; r++)
        {
            ref var run = ref runs[r];
            var clusterOffset = run.range.start;
            var glyphEnd = run.glyphStart + run.glyphCount;
            float width = 0;

            for (var g = run.glyphStart; g < glyphEnd; g++)
            {
                var globalCluster = glyphs[g].cluster + clusterOffset;

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

    private void OnRebuildEnd()
    {
        if (clusterToObj == null || clusterToObj.Count == 0) return;

        var glyphs = uniText.LastResultGlyphs;
        var scale = UniTextMeshGenerator.scale;

        for (var i = 0; i < glyphs.Length; i++)
            if (clusterToObj.TryGetValue(glyphs[i].cluster, out var obj))
            {
                if (obj.prefab == null) continue;
                var glyph = glyphs[i];
                CreateObjectInstance(obj,
                    glyph.x + obj.bearingX * scale,
                    -glyph.y + obj.bearingY * scale,
                    obj.width * scale,
                    obj.height * scale);
            }
    }

    private void CreateObjectInstance(InlineObject obj, float x, float y, float w, float h)
    {
        if (uniText == null) return;

        var wrapper = obj.GetOrCreate(uniText.transform);
        wrapper.isDirty = true;
        var pivot = wrapper.pivot;
        wrapper.anchoredPosition = new Vector2(x + w * pivot.x, y + h * pivot.y);
        wrapper.sizeDelta = new Vector2(w, h);
    }

    private void DestroyAllObjects()
    {
        if (uniText == null) return;
        for (var i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            obj.activeCount = 0;
        }

        CanvasUpdateRegistry.Updated += Destro;

        void Destro()
        {
            CanvasUpdateRegistry.Updated -= Destro;
            if (uniText == null) return;
            for (var i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                obj.UpdateInstances();
            }
        }
    }
}