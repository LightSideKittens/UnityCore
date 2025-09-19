using System.Collections.Generic;
using System.Linq;
using LSCore;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
internal static class LottieUpdater
{
    public static List<BaseLottieManager> managers = new(64);

    public static float[] fpsSteps =
    {
        0.01f,
        0.018f,
        0.023f,
    };
    

    public static uint sizeDivider = 1;
    public static int framesToSkip;
    
    public static float[] lastResults;
    public static long[] minPixelCount =
    {
        long.MaxValue,
        long.MaxValue,
        long.MaxValue,
        long.MaxValue,
    };

    private static void ResetMinPixelCount()
    {
        minPixelCount = new[]
        {
            long.MaxValue,
            long.MaxValue,
            long.MaxValue,
            long.MaxValue,
        };
    }

    static LottieUpdater()
    {
#if UNITY_EDITOR
        World.Created += Reset;
        World.Destroyed += Reset;
        
        void Reset()
        {
            fpsSteps = new float[3];
            ResetMinPixelCount();
            InitFpsSteps();
            lastResults = null;
            lastPixels = -1;
            lastReduceLevel = -1;
            currentPixels = 0;
            reduceLevel = 0;
            time = 0;
        }
        
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.update += OnEditorUpdate;

        
        void OnSelectionChanged()
        {
            EditorWorld.Updated -= OnUpdate;
            if (Selection.gameObjects.Any(go => go && (
                    go.GetComponent<LottieRenderer>() || go.GetComponent<LottieImage>())))
            {
                EditorWorld.Updated += OnUpdate;
            }
        }

        void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            OnSelectionChanged();
        }
#else
        InitFpsSteps();
#endif
        World.Updated += OnUpdate;

        void InitFpsSteps()
        {
            var targetFrameRate = 1f / Application.targetFrameRate;
            fpsSteps[0] = targetFrameRate * 1.1f;
            fpsSteps[1] = targetFrameRate * 1.8f;
            fpsSteps[2] = targetFrameRate * 2.3f;
        }
    }
    
    private static long currentPixels = 0;
    private static long lastPixels = -1;
    private static int reduceLevel = 0;
    private static int lastReduceLevel = -1;
    private static float time;
    private static float refreshTime = 10;

    private static (int framesToSkip, uint sizeDivider)[] reduceLevels =
    {
        (0, 1),
        (1, 1),
        (2, 1),
        (1, 2)
    };
    
    private static void OnUpdate()
    {
        var sdt = Time.smoothDeltaTime;
        time += Time.deltaTime;
        
        if (lastResults == null)
        {
            lastResults = new float[5];
            for (var i = 0; i < 5; i++)
            {
                lastResults[i] = sdt;
            }
        }
        
        var length = lastResults.Length;
        lastResults[Time.frameCount % length] = sdt;
        var avrDt = 0f;
        for (var i = 0; i < length; i++)
        {
            avrDt += lastResults[i];
        }
        avrDt /= length;

        if (currentPixels > 0 && Time.frameCount > length)
        {
            if (avrDt > fpsSteps[2])
            {
                UpdateMinPixels(3);
            }
            else if(avrDt > fpsSteps[1])
            {
                UpdateMinPixels(2);
            }
            else if(avrDt > fpsSteps[0])
            {
                UpdateMinPixels(1);
            }
            else
            {
                UpdateMinPixels(0);
            }
        }

        if (lastReduceLevel != reduceLevel && lastPixels != currentPixels)
        {
            for (var i = 0; i < reduceLevels.Length; i++)
            {
                var min = minPixelCount[i];
                if (min != long.MaxValue && currentPixels <= min)
                {
                    reduceLevel = i;
                    break;
                }
            }
            
            lastPixels = currentPixels;
            lastReduceLevel = reduceLevel;
        }
        
        var level = reduceLevels[lastReduceLevel];
        sizeDivider = level.sizeDivider;
        framesToSkip = level.framesToSkip;
        Update();

        if (time > refreshTime)
        {
            time = 0;
            lastPixels = -1;
            lastReduceLevel = -1;
            ResetMinPixelCount();
        }
        
        void UpdateMinPixels(int index)
        {
            reduceLevel = index;
            var min = minPixelCount[index];
            if (currentPixels < min)
            {
                minPixelCount[index] = currentPixels;
            }
        }
    }
    
    public static void Update()
    {
        if (managers.Count > 0)
        {
            int i = 0;
            currentPixels = 0;
            for (; i < managers.Count; i++)
            {
                var m = managers[i];
                m.ApplySettings();
                m.Update();
                var size = m.lottie.size;
                currentPixels += size.x * size.y / (m.framesToSkip + 1);
            }
        }
    }
}