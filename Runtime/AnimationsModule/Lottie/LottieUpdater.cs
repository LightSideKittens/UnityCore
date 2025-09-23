using System;
using System.Collections.Generic;
using System.Linq;
using LSCore;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
internal static class LottieUpdater
{
    public static List<BaseLottieManager> managers = new(64);
    public static Action[] PreRendering = new Action[4];
    public static Action[] CanvasPreRendering = new Action[4];
    private static Action preUpdated;
    public static event Action TextureApplyTime
    {
        add
        {
            if (isPreUpdate)
            {
                preUpdated += value;
            }
            else
            {
                PreRendering[2] += value;
                CanvasPreRendering[2] += value;
            }
        }
        remove
        {
            if (isPreUpdate)
            {
                preUpdated -= value;
            }
            else
            {
                PreRendering[2] -= value;
                CanvasPreRendering[2] -= value;
            }
        }
    }
    
    static LottieUpdater()
    {
#if UNITY_EDITOR
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
#endif
        World.PreRendering += OnPreRendering;
        World.CanvasPreRendering += OnCanvasPreRendering;
        World.Updated += OnUpdate;
    }
    
    private static void OnPreRendering()
    {
        for (int i = 0; i < PreRendering.Length; i++)
        {
            PreRendering[i]?.Invoke();
        }
    }

    private static void OnCanvasPreRendering()
    {
        for (int i = 0; i < CanvasPreRendering.Length; i++)
        {
            CanvasPreRendering[i]?.Invoke();
        }
    }

    private static void OnUpdate()
    {
        Update();
    }

    private static bool isPreUpdate;
    
    public static void Update()
    {
        isPreUpdate = true;
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].PreUpdate();
        }
        preUpdated?.Invoke();
        isPreUpdate = false;
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].Update();
        }
    }
}