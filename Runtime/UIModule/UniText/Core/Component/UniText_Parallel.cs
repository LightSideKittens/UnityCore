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

    // Heuristics for parallelism
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

        // Check if already registered
        for (var i = 0; i < componentsCount; i++)
        {
            if (componentsBuffer[i] == component)
                return;
        }

        // Grow buffer if needed
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
                // Swap with last and shrink
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

        // Prepare all components on main thread
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

        // Execute FirstPass (parallel or sequential)
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

        // Phase 1: Rasterization (main thread - FontEngine)
        Profiler.BeginSample("Rasterization");
        for (var i = 0; i < count; i++)
        {
            componentsBuffer[i].PrepareForParallel();
            componentsBuffer[i].DoEnsureGlyphsInAtlas();
        }
        Profiler.EndSample();

        // Phase 2: Mesh data generation (parallel with same thread affinity as FirstPass)
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

        // Phase 3: Apply meshes (main thread - Unity Mesh API)
        Profiler.BeginSample("ApplyMeshes");
        
        for (var i = 0; i < count; i++)
        {
            componentsBuffer[i].DoApplyMesh();
        }

        Profiler.EndSample();

        // Clear buffer
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

        // Update modifier ThreadStatic buffers to point to this component's data
        Rebuilding?.Invoke();

        // Use cached data (thread-safe)
        ref readonly var cached = ref cachedTransformData;

        // EnsurePositions если ещё не сделано
        if (!textProcessor.HasValidPositionedGlyphs)
        {
            var settings = CreateProcessSettings(cached.rect, cachedEffectiveFontSize);
            textProcessor.EnsurePositions(settings);
        }

        var glyphs = textProcessor.PositionedGlyphs;
        if (glyphs.IsEmpty) return;

        var effectiveFontSize = enableAutoSize ? autoSizedFontSize : fontSize;
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

        // Update last result metrics
        if (textProcessor != null)
        {
            lastResultWidth = textProcessor.ResultWidth;
            lastResultHeight = textProcessor.ResultHeight;
        }

        UpdateRendering();
    }

    #endregion
}
