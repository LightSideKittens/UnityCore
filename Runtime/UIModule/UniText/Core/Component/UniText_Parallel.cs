using System;
using UnityEngine;
using UnityEngine.UI;


public partial class UniText
{
    #region Cached Data for Parallel
    
    public struct CachedTransformData
    {
        public Rect rect;
        public float lossyScale;
        public bool hasWorldCamera;
    }

    public CachedTransformData cachedTransformData;
    
    private void PrepareForParallel()
    {
        cachedTransformData = new CachedTransformData
        {
            rect = rectTransform.rect,
            lossyScale = transform.lossyScale.x,
            hasWorldCamera = canvas != null && canvas.worldCamera != null
        };
    }

    #endregion

    #region Static Batch Processing

    public static bool UseParallel { get; set; } = true;
    private static PooledBuffer<UniText> componentsBuffer;
    private static bool isSubscribed;
    private static bool useParallel;

    private const int ParallelCharacterThreshold = 500;
    private const int MinComponentsForParallel = 3;

    private static void EnsureSubscribed()
    {
        if (isSubscribed) return;
        var d = CanvasUpdateRegistry.instance;
        Canvas.preWillRenderCanvases += OnPreWillRenderCanvases;
        Canvas.willRenderCanvases += OnWillRenderCanvases;
        componentsBuffer.EnsureCapacity(64);
        isSubscribed = true;
    }

    private static void RegisterDirty(UniText component)
    {
        EnsureSubscribed();

        for (var i = 0; i < componentsBuffer.count; i++)
        {
            if (componentsBuffer[i] == component)
                return;
        }

        componentsBuffer.Add(component);
    }

    private static void UnregisterDirty(UniText component)
    {
        for (var i = 0; i < componentsBuffer.count; i++)
        {
            if (componentsBuffer[i] == component)
            {
                componentsBuffer.SwapRemoveAt(i);
                return;
            }
        }
    }

    private static bool CanWork
    {
        get
        {
            if (!UnicodeData.IsInitialized)
            {
                UnicodeData.EnsureInitialized();
                if (!UnicodeData.IsInitialized)
                {
                    Debug.LogError("UniText: Unicode data not initialized.");
                    UniTextDebug.EndSample();
                    return false;
                }
            }

            return true;
        }
    }
    
    private static void OnPreWillRenderCanvases()
    {
        if (componentsBuffer.count == 0) return;
        if (!CanWork) return;

        UniTextDebug.BeginSample("UniText.PreWillRender.FirstPass");

        var count = componentsBuffer.count;
        var totalChars = 0;

        for (var i = 0; i < count; i++)
        {
            var comp = componentsBuffer[i];
            var t = comp.text;
            if (t != null)
            {
                comp.ValidateAndInitialize();
                comp.PrepareForParallel();
                totalChars += t.Length;
            }
        }

        useParallel = totalChars > ParallelCharacterThreshold &&
                      count >= MinComponentsForParallel &&
                      UniTextWorkerPool.IsParallelSupported;

        if (useParallel && UseParallel)
        {
            UniTextWorkerPool.Execute(componentsBuffer.data, count, static comp => comp.DoFirstPass());
        }
        else
        {
            for (var i = 0; i < count; i++)
            {
                componentsBuffer[i].DoFirstPass();
            }
        }

        UniTextDebug.EndSample();
    }
    
    private static void OnWillRenderCanvases()
    {
        if (componentsBuffer.count == 0) return;
        if (!CanWork) return;

        UniTextDebug.BeginSample("UniText.WillRender.MeshGeneration");

        var count = componentsBuffer.count;

        UniTextDebug.BeginSample("Rasterization");
        for (var i = 0; i < count; i++)
        {
            componentsBuffer[i].PrepareForParallel();
            componentsBuffer[i].DoEnsureGlyphsInAtlas();
        }
        UniTextDebug.EndSample();

        UniTextDebug.BeginSample("MeshDataGeneration");
        if (useParallel && UseParallel)
        {
            UniTextWorkerPool.Execute(componentsBuffer.data, count, static comp => comp.DoGenerateMeshData());
        }
        else
        {
            for (var i = 0; i < count; i++)
            {
                componentsBuffer[i].DoGenerateMeshData();
            }
        }
        UniTextDebug.EndSample();

        UniTextDebug.BeginSample("ApplyMeshes");

        for (var i = 0; i < count; i++)
        {
            componentsBuffer[i].DoApplyMesh();
        }

        UniTextDebug.EndSample();

        componentsBuffer.Clear();

        UniTextDebug.EndSample();
    }

    #endregion

    #region Instance Batch Methods
    
    private void DoFirstPass()
    {
        if (string.IsNullOrEmpty(text)) return;

        var textSpan = ParseOrGetParsedAttributes();
        var shapingFontSize = enableAutoSize ? maxFontSize : fontSize;
        var settings = new TextProcessSettings
        {
            fontSize = shapingFontSize,
            baseDirection = baseDirection
        };
        textProcessor.EnsureFirstPass(textSpan, settings);
    }
    
    private void DoEnsureGlyphsInAtlas()
    {
        if (textProcessor == null || !textProcessor.HasValidFirstPassData) return;
        textProcessor.EnsureGlyphsInAtlas();
    }
    
    private void DoGenerateMeshData()
    {
        if (textProcessor == null || !textProcessor.HasValidFirstPassData) return;
        if (meshGenerator == null) return;

        Rebuilding?.Invoke();

        ref readonly var cached = ref cachedTransformData;

        var effectiveFontSize = enableAutoSize
            ? (cachedEffectiveFontSize > 0 ? cachedEffectiveFontSize : maxFontSize)
            : fontSize;

        if (!textProcessor.HasValidPositionedGlyphs)
        {
            var settings = CreateProcessSettings(cached.rect, effectiveFontSize);
            textProcessor.EnsurePositions(settings);
        }

        var glyphs = textProcessor.PositionedGlyphs;
        if (glyphs.IsEmpty) return;
        meshGenerator.FontSize = effectiveFontSize;
        meshGenerator.defaultColor = color;
        meshGenerator.SetCanvasParametersCached(cached.lossyScale, cached.hasWorldCamera);
        meshGenerator.SetRectOffset(cached.rect);
        meshGenerator.SetHorizontalAlignment(horizontalAlignment);

        meshGenerator.GenerateMeshDataOnly(glyphs);
    }
    
    private void DoApplyMesh()
    {
        if (meshGenerator == null || !meshGenerator.HasGeneratedData) return;

        ReleaseMeshes();
        lastMeshPairs = meshGenerator.ApplyMeshesToUnity(GetPooledMeshForText);
        meshGenerator.ReturnInstanceBuffers();
        
        if (textProcessor != null)
        {
            lastResultWidth = textProcessor.ResultWidth;
            lastResultHeight = textProcessor.ResultHeight;
        }

        UpdateRendering();

        dirtyFlags = DirtyFlags.None;
    }

    #endregion
}
