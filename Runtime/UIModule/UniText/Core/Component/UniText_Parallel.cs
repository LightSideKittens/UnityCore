using System;
using UnityEngine;
using UnityEngine.Profiling;


public partial class UniText
{
    #region Cached Data for Parallel

    /// <summary>
    /// Cached Unity object data for thread-safe parallel access.
    /// Must be populated on main thread before parallel execution.
    /// </summary>
    public struct CachedTransformData
    {
        public Rect rect;
        public float lossyScale;
        public bool hasWorldCamera;
    }

    public CachedTransformData cachedTransformData;

    /// <summary>
    /// Cache Unity object data on main thread for parallel access.
    /// </summary>
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

    private static UniText[] componentsBuffer = new UniText[64];
    private static int componentsCount;
    private static bool isSubscribed;
    private static bool useParallel;

    private const int ParallelCharacterThreshold = 500;
    private const int MinComponentsForParallel = 3;

    private static void EnsureSubscribed()
    {
        if (isSubscribed) return;
        Canvas.preWillRenderCanvases += OnPreWillRenderCanvases;
        Canvas.willRenderCanvases += OnWillRenderCanvases;
        isSubscribed = true;
    }

    private static void RegisterDirty(UniText component)
    {
        EnsureSubscribed();

        for (var i = 0; i < componentsCount; i++)
        {
            if (componentsBuffer[i] == component)
                return;
        }

        if (componentsCount >= componentsBuffer.Length)
            Array.Resize(ref componentsBuffer, componentsBuffer.Length * 2);

        componentsBuffer[componentsCount++] = component;
    }

    private static void UnregisterDirty(UniText component)
    {
        for (var i = 0; i < componentsCount; i++)
        {
            if (componentsBuffer[i] == component)
            {
                componentsBuffer[i] = componentsBuffer[--componentsCount];
                componentsBuffer[componentsCount] = null;
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
                    Profiler.EndSample();
                    return false;
                }
            }

            return true;
        }
    }
    
    /// <summary>
    /// Called BEFORE Layout phase.
    /// Parallel FirstPass: Parse, BiDi, Script, Shaping.
    /// </summary>
    private static void OnPreWillRenderCanvases()
    {
        if (componentsCount == 0) return;
        if (!CanWork) return;

        Profiler.BeginSample("UniText.PreWillRender.FirstPass");

        var count = componentsCount;
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

        if (useParallel)
        {
            UniTextWorkerPool.Execute(componentsBuffer, count, static comp => comp.DoFirstPass());
        }
        else
        {
            for (var i = 0; i < count; i++)
            {
                componentsBuffer[i].DoFirstPass();
            }
        }

        Profiler.EndSample();
    }

    
    /// <summary>
    /// Called AFTER Layout phase (ILayoutElement already used ready data).
    /// Rasterization (main thread), Mesh generation (parallel), Apply (main thread).
    /// </summary>
    private static void OnWillRenderCanvases()
    {
        if (componentsCount == 0) return;
        if (!CanWork) return;

        Profiler.BeginSample("UniText.WillRender.MeshGeneration");

        var count = componentsCount;

        Profiler.BeginSample("Rasterization");
        for (var i = 0; i < count; i++)
        {
            componentsBuffer[i].PrepareForParallel();
            componentsBuffer[i].DoEnsureGlyphsInAtlas();
        }
        Profiler.EndSample();

        Profiler.BeginSample("MeshDataGeneration");
        if (useParallel)
        {
            UniTextWorkerPool.Execute(componentsBuffer, count, static comp => comp.DoGenerateMeshData());
        }
        else
        {
            for (var i = 0; i < count; i++)
            {
                componentsBuffer[i].DoGenerateMeshData();
            }
        }
        Profiler.EndSample();

        Profiler.BeginSample("ApplyMeshes");
        
        for (var i = 0; i < count; i++)
        {
            componentsBuffer[i].DoApplyMesh();
        }

        Profiler.EndSample();

        for (var i = 0; i < count; i++)
        {
            componentsBuffer[i] = null;
        }

        componentsCount = 0;

        Profiler.EndSample();
    }

    #endregion

    #region Instance Batch Methods

    /// <summary>
    /// FirstPass: Parse, BiDi, Script analysis, Shaping.
    /// Called in parallel from preWillRenderCanvases.
    /// </summary>
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

    /// <summary>
    /// Rasterization: Add glyphs to font atlas.
    /// Must be called on main thread (FontEngine requirement).
    /// </summary>
    private void DoEnsureGlyphsInAtlas()
    {
        if (textProcessor == null || !textProcessor.HasValidFirstPassData) return;
        textProcessor.EnsureGlyphsInAtlas();
    }

    /// <summary>
    /// Generate mesh data into instance buffers.
    /// Can be called in parallel.
    /// </summary>
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
        meshGenerator.DefaultColor = color;
        meshGenerator.SetCanvasParametersCached(cached.lossyScale, cached.hasWorldCamera);
        meshGenerator.SetRectOffset(cached.rect);
        meshGenerator.SetHorizontalAlignment(horizontalAlignment);

        meshGenerator.GenerateMeshDataOnly(glyphs);
    }

    /// <summary>
    /// Apply generated mesh data to Unity Mesh.
    /// Must be called on main thread (Unity Mesh API requirement).
    /// </summary>
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
